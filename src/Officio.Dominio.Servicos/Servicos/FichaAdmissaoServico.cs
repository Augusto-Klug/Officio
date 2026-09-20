using System.Security.Cryptography;
using System.Text;
using Officio.Dominio.Admissoes;
using Officio.Dominio.Contratos;
using Officio.Dominio.Contratos.Dtos;
using Officio.Dominio.Contratos.Response;
using Officio.Dominio.Entidades;
using Officio.Dominio.Enumeradores;
using Officio.Dominio.Interfaces;

namespace Officio.Dominio.Servicos.Servicos;

public sealed class FichaAdmissaoServico(
    IAdmissaoRepositorio repositorio,
    IAdmissaoDocumentoServico documentoServico,
    IEmpresaAtual empresaAtual) : IFichaAdmissaoServico
{
    private static readonly TimeSpan ValidadeLink = TimeSpan.FromDays(7);

    public async Task<Resultado<GerarLinkFichaAdmissaoDto>> GerarLinkAsync(
        Guid admissaoId,
        CancellationToken ct = default)
    {
        var admissao = await repositorio.ObterParaAlteracaoAsync(admissaoId, ct);
        if (admissao is null)
            return Resultado<GerarLinkFichaAdmissaoDto>.CriarFalha("Admissao nao encontrada.");
        if (StatusAdmissaoCodigos.Terminal(admissao.StatusAdmissao.Codigo))
            return Resultado<GerarLinkFichaAdmissaoDto>.CriarFalha("Nao e possivel gerar link para uma admissao encerrada.");

        var token = GerarToken();
        var expiraEm = DateTime.UtcNow.Add(ValidadeLink);
        admissao.GerarAcessoTemporario(CalcularHash(token), expiraEm);
        var status = await repositorio.ObterOuCriarStatusAsync(
            StatusAdmissaoCodigos.AguardandoPreenchimento, "Aguardando preenchimento", 2, ct);
        admissao.AlterarStatus(status.Id);
        await repositorio.SalvarAlteracoesAsync(ct);
        return Resultado<GerarLinkFichaAdmissaoDto>.CriarSucesso(new(token, expiraEm));
    }

    public async Task<Resultado<bool>> RevogarLinkAsync(Guid admissaoId, CancellationToken ct = default)
    {
        var admissao = await repositorio.ObterParaAlteracaoAsync(admissaoId, ct);
        if (admissao is null)
            return Resultado<bool>.CriarFalha("Admissao nao encontrada.");
        admissao.RevogarAcessoTemporario();
        if (admissao.StatusAdmissao.Codigo == StatusAdmissaoCodigos.AguardandoPreenchimento &&
            !admissao.PreenchimentoFuncionarioConcluidoEm.HasValue)
        {
            var status = await repositorio.ObterOuCriarStatusAsync(
                StatusAdmissaoCodigos.Rascunho, "Rascunho", 1, ct);
            admissao.AlterarStatus(status.Id);
        }
        await repositorio.SalvarAlteracoesAsync(ct);
        return Resultado<bool>.CriarSucesso(true);
    }

    public async Task<Resultado<FichaAdmissaoPublicaDto>> ObterAsync(string token, CancellationToken ct = default)
    {
        var resultado = await ObterPorTokenAsync(token, rastreamento: false, exigirEditavel: false, ct);
        if (!resultado.Sucesso)
            return Resultado<FichaAdmissaoPublicaDto>.CriarFalha(resultado.Erro!);

        return Resultado<FichaAdmissaoPublicaDto>.CriarSucesso(Mapear(resultado.Valor!));
    }

    public async Task<Resultado<bool>> AtualizarAsync(
        string token,
        AtualizarFichaAdmissaoRequisicao requisicao,
        CancellationToken ct = default)
    {
        var resultado = await ObterPorTokenAsync(token, rastreamento: true, exigirEditavel: true, ct);
        if (!resultado.Sucesso)
            return Resultado<bool>.CriarFalha(resultado.Erro!);
        if (string.IsNullOrWhiteSpace(requisicao.NomeFuncionario))
            return Resultado<bool>.CriarFalha("Nome do funcionario e obrigatorio.");

        var admissao = resultado.Valor!;
        try
        {
            admissao.AtualizarDadosFuncionario(
                requisicao.NomeFuncionario, requisicao.Cpf, requisicao.Rg, requisicao.DataNascimento,
                requisicao.Sexo, requisicao.EstadoCivil, requisicao.Escolaridade, requisicao.Telefone,
                requisicao.Email, requisicao.Pis, requisicao.Banco, requisicao.Agencia, requisicao.Conta,
                requisicao.TipoConta, requisicao.UtilizaValeTransporte);
            admissao.SubstituirDependentes(requisicao.Dependentes.Select(item =>
                AdmissaoDependente.Criar(admissao.Id, item.Nome, item.Cpf, item.DataNascimento,
                    item.Parentesco, item.DependenteIr, item.SalarioFamilia)));
        }
        catch (ArgumentException exception)
        {
            return Resultado<bool>.CriarFalha(exception.Message);
        }

        await repositorio.AdicionarDependentesAsync(admissao.Dependentes, ct);
        await repositorio.SalvarAlteracoesAsync(ct);
        return Resultado<bool>.CriarSucesso(true);
    }

    public async Task<Resultado<bool>> ConcluirAsync(string token, CancellationToken ct = default)
    {
        var resultado = await ObterPorTokenAsync(token, rastreamento: true, exigirEditavel: true, ct);
        if (!resultado.Sucesso)
            return Resultado<bool>.CriarFalha(resultado.Erro!);

        var admissao = resultado.Valor!;
        if (string.IsNullOrWhiteSpace(admissao.Cpf) || admissao.DataNascimento is null ||
            (string.IsNullOrWhiteSpace(admissao.Email) && string.IsNullOrWhiteSpace(admissao.Telefone)))
            return Resultado<bool>.CriarFalha("Preencha CPF, data de nascimento e ao menos um contato antes de finalizar.");
        if (admissao.Documentos.Any(item => item.Obrigatorio && item.DocumentoId is null))
            return Resultado<bool>.CriarFalha("Envie todos os documentos obrigatorios antes de finalizar.");

        var status = await repositorio.ObterOuCriarStatusAsync(
            StatusAdmissaoCodigos.EmValidacao, "Em validacao", 3, ct);
        admissao.MarcarPreenchimentoFuncionarioConcluido();
        admissao.AlterarStatus(status.Id);
        await repositorio.SalvarAlteracoesAsync(ct);
        return Resultado<bool>.CriarSucesso(true);
    }

    public async Task<Resultado<ResultadoDocumentoAdmissaoDto>> EnviarDocumentoAsync(
        string token,
        Guid itemId,
        Stream arquivo,
        string nomeArquivo,
        string mimeType,
        long tamanhoBytes,
        CancellationToken ct = default)
    {
        var resultado = await ObterPorTokenAsync(token, rastreamento: false, exigirEditavel: true, ct);
        if (!resultado.Sucesso)
            return Resultado<ResultadoDocumentoAdmissaoDto>.CriarFalha(resultado.Erro!);
        var admissao = resultado.Valor!;
        if (admissao.Documentos.All(item => item.Id != itemId))
            return Resultado<ResultadoDocumentoAdmissaoDto>.CriarFalha("Documento da admissao nao encontrado.");

        return await documentoServico.EnviarAsync(
            new(admissao.Id, itemId, arquivo, nomeArquivo, mimeType, tamanhoBytes), ct);
    }

    public async Task<Resultado<ResultadoUrlDocumentoAdmissaoDto>> ObterUrlDocumentoAsync(
        string token,
        Guid itemId,
        CancellationToken ct = default)
    {
        var resultado = await ObterPorTokenAsync(token, rastreamento: false, exigirEditavel: false, ct);
        if (!resultado.Sucesso)
            return Resultado<ResultadoUrlDocumentoAdmissaoDto>.CriarFalha(resultado.Erro!);
        var admissao = resultado.Valor!;
        if (admissao.Documentos.All(item => item.Id != itemId))
            return Resultado<ResultadoUrlDocumentoAdmissaoDto>.CriarFalha("Documento da admissao nao encontrado.");
        return await documentoServico.ObterUrlAsync(admissao.Id, itemId, ct);
    }

    private async Task<Resultado<Admissao>> ObterPorTokenAsync(
        string token,
        bool rastreamento,
        bool exigirEditavel,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(token))
            return Resultado<Admissao>.CriarFalha("Link de admissao invalido ou expirado.");
        var admissao = await repositorio.ObterPorTokenHashAsync(CalcularHash(token), rastreamento, ct);
        if (admissao is null || admissao.TokenExpiraEm is null || admissao.TokenExpiraEm <= DateTime.UtcNow)
            return Resultado<Admissao>.CriarFalha("Link de admissao invalido ou expirado.");
        if (StatusAdmissaoCodigos.Terminal(admissao.StatusAdmissao.Codigo))
            return Resultado<Admissao>.CriarFalha("Esta admissao ja foi encerrada.");
        if (exigirEditavel && admissao.PreenchimentoFuncionarioConcluidoEm.HasValue)
            return Resultado<Admissao>.CriarFalha("A ficha ja foi finalizada e nao pode mais ser alterada.");

        empresaAtual.SetTenant(admissao.TenantId);
        return Resultado<Admissao>.CriarSucesso(admissao);
    }

    private static FichaAdmissaoPublicaDto Mapear(Admissao admissao)
        => new(
            admissao.Id, admissao.Empresa.RazaoSocial, admissao.NomeFuncionario, admissao.Cpf,
            admissao.Rg, admissao.DataNascimento, admissao.Sexo, admissao.EstadoCivil,
            admissao.Escolaridade, admissao.Telefone, admissao.Email, admissao.Pis, admissao.Banco,
            admissao.Agencia, admissao.Conta, admissao.TipoConta, admissao.UtilizaValeTransporte,
            admissao.DataAdmissaoPrevista, admissao.Funcao,
            admissao.PreenchimentoFuncionarioConcluidoEm.HasValue, admissao.TokenExpiraEm!.Value,
            admissao.Dependentes.Select(item => new AdmissaoDependenteDto(
                item.Id, item.Nome, item.Cpf, item.DataNascimento, item.Parentesco,
                item.DependenteIr, item.SalarioFamilia)).ToList(),
            admissao.Documentos.OrderBy(item => item.Ordem).Select(MapearDocumento).ToList());

    private static AdmissaoDocumentoDto MapearDocumento(AdmissaoDocumento item)
    {
        var validacao = item.Documento?.Validacoes.OrderByDescending(x => x.CriadoEm).FirstOrDefault();
        var status = item.Documento is null ? "PENDENTE_ENVIO" : validacao?.Status switch
        {
            StatusValidacao.Aprovado => "APROVADO",
            StatusValidacao.Reprovado => "REPROVADO",
            StatusValidacao.RevisaoManual => "REVISAO_MANUAL",
            StatusValidacao.Pendente => "PENDENTE_VALIDACAO",
            _ => "ENVIADO"
        };
        return new(item.Id, item.TipoDocumentoId, item.TipoDocumento.Codigo, item.TipoDocumento.Descricao,
            item.RotuloExibicao ?? item.TipoDocumento.Descricao, item.Obrigatorio, item.Ordem,
            item.DocumentoId, item.Documento?.NomeArquivo, item.Documento?.MimeType,
            item.Documento?.TamanhoBytes, item.Documento?.QualidadeEstimada, item.Documento?.CriadoEm,
            status, validacao?.ConfiancaOcr, validacao?.Observacao);
    }

    private static string GerarToken()
        => Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))
            .TrimEnd('=').Replace('+', '-').Replace('/', '_');

    private static string CalcularHash(string token)
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token.Trim())));
}
