using System.Security.Cryptography;
using System.Text;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
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
    IEmpresaAtual empresaAtual,
    IServicoEmail servicoEmail,
    IConfiguration configuracao) : IFichaAdmissaoServico
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
        if (admissao.PreenchimentoFuncionarioConcluidoEm.HasValue)
            return Resultado<GerarLinkFichaAdmissaoDto>.CriarFalha("O admitido ja concluiu o preenchimento da ficha.");

        var token = GerarToken();
        var expiraEm = DateTime.UtcNow.Add(ValidadeLink);
        admissao.GerarAcessoTemporario(CalcularHash(token), expiraEm);
        var status = await repositorio.ObterOuCriarStatusAsync(
            StatusAdmissaoCodigos.AguardandoAdmitido, "Aguardando admitido", 2, ct);
        admissao.AlterarStatus(status.Id);
        await repositorio.SalvarAlteracoesAsync(ct);
        return Resultado<GerarLinkFichaAdmissaoDto>.CriarSucesso(new(token, expiraEm));
    }

    public async Task<Resultado<GerarLinkFichaAdmissaoDto>> GerarLinkEmpresaAsync(
        Guid admissaoId,
        CancellationToken ct = default)
    {
        var admissao = await repositorio.ObterParaAlteracaoAsync(admissaoId, ct);
        if (admissao is null)
            return Resultado<GerarLinkFichaAdmissaoDto>.CriarFalha("Admissao nao encontrada.");
        if (StatusAdmissaoCodigos.Terminal(admissao.StatusAdmissao.Codigo))
            return Resultado<GerarLinkFichaAdmissaoDto>.CriarFalha("Nao e possivel gerar link para uma admissao encerrada.");
        if (admissao.StatusAdmissao.Codigo != StatusAdmissaoCodigos.DocumentosValidados)
            return Resultado<GerarLinkFichaAdmissaoDto>.CriarFalha("O link do empregador sera liberado apos a contabilidade concluir a revisao documental.");
        if (admissao.PreenchimentoEmpresaConcluidoEm.HasValue)
            return Resultado<GerarLinkFichaAdmissaoDto>.CriarFalha("O empregador ja concluiu o preenchimento da ficha.");

        var token = GerarToken();
        var expiraEm = DateTime.UtcNow.Add(ValidadeLink);
        admissao.GerarAcessoEmpresa(CalcularHash(token), expiraEm);
        var status = await repositorio.ObterOuCriarStatusAsync(
            StatusAdmissaoCodigos.AguardandoEmpregador, "Aguardando empregador", 5, ct);
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
        if (admissao.StatusAdmissao.Codigo == StatusAdmissaoCodigos.AguardandoAdmitido &&
            !admissao.PreenchimentoFuncionarioConcluidoEm.HasValue)
        {
            var status = await repositorio.ObterOuCriarStatusAsync(
                StatusAdmissaoCodigos.Rascunho, "Rascunho", 1, ct);
            admissao.AlterarStatus(status.Id);
        }
        await repositorio.SalvarAlteracoesAsync(ct);
        return Resultado<bool>.CriarSucesso(true);
    }

    public async Task<Resultado<bool>> RevogarLinkEmpresaAsync(Guid admissaoId, CancellationToken ct = default)
    {
        var admissao = await repositorio.ObterParaAlteracaoAsync(admissaoId, ct);
        if (admissao is null)
            return Resultado<bool>.CriarFalha("Admissao nao encontrada.");
        admissao.RevogarAcessoEmpresa();
        if (admissao.StatusAdmissao.Codigo == StatusAdmissaoCodigos.AguardandoEmpregador &&
            !admissao.PreenchimentoEmpresaConcluidoEm.HasValue)
        {
            var status = await repositorio.ObterOuCriarStatusAsync(
                StatusAdmissaoCodigos.DocumentosValidados, "Documentos validados", 4, ct);
            admissao.AlterarStatus(status.Id);
        }
        await repositorio.SalvarAlteracoesAsync(ct);
        return Resultado<bool>.CriarSucesso(true);
    }

    public async Task<Resultado<FichaFuncionarioPublicaDto>> ObterAsync(string token, CancellationToken ct = default)
    {
        var resultado = await ObterFuncionarioPorTokenAsync(token, rastreamento: false, exigirEditavel: false, ct);
        return resultado.Sucesso
            ? Resultado<FichaFuncionarioPublicaDto>.CriarSucesso(MapearFuncionario(resultado.Valor!))
            : Resultado<FichaFuncionarioPublicaDto>.CriarFalha(resultado.Erro!);
    }

    public async Task<Resultado<bool>> AtualizarAsync(
        string token,
        AtualizarFichaFuncionarioRequisicao requisicao,
        CancellationToken ct = default)
    {
        var resultado = await ObterFuncionarioPorTokenAsync(token, rastreamento: true, exigirEditavel: true, ct);
        if (!resultado.Sucesso)
            return Resultado<bool>.CriarFalha(resultado.Erro!);

        var admissao = resultado.Valor!;
        try
        {
            admissao.AtualizarDadosFuncionario(
                requisicao.NomeFuncionario, requisicao.DataNascimento, requisicao.Sexo,
                requisicao.EstadoCivil, requisicao.Escolaridade, requisicao.Telefone, requisicao.Email);
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
        var resultado = await ObterFuncionarioPorTokenAsync(token, rastreamento: true, exigirEditavel: true, ct);
        if (!resultado.Sucesso)
            return Resultado<bool>.CriarFalha(resultado.Erro!);

        var admissao = resultado.Valor!;
        if (string.IsNullOrWhiteSpace(admissao.Cpf) || admissao.DataNascimento is null ||
            (string.IsNullOrWhiteSpace(admissao.Email) && string.IsNullOrWhiteSpace(admissao.Telefone)))
            return Resultado<bool>.CriarFalha("Preencha CPF, data de nascimento e ao menos um contato antes de finalizar.");
        if (admissao.Documentos.Any(item => item.Obrigatorio && item.DocumentoId is null &&
                item.TipoDocumento.Codigo is not ("IDENTIDADE" or "CNH")))
            return Resultado<bool>.CriarFalha("Envie todos os documentos obrigatorios antes de finalizar.");
        if (!AdmissaoDocumentoRegras.PossuiIdentificacao(admissao.Documentos))
            return Resultado<bool>.CriarFalha("Envie uma identidade (RG/CIN) ou CNH antes de finalizar.");

        var status = await repositorio.ObterOuCriarStatusAsync(
            StatusAdmissaoCodigos.AguardandoValidacao, "Aguardando validacao", 3, ct);
        admissao.MarcarPreenchimentoFuncionarioConcluido();
        admissao.AlterarStatus(status.Id);
        await repositorio.SalvarAlteracoesAsync(ct);
        return Resultado<bool>.CriarSucesso(true);
    }

    public async Task<Resultado<FichaEmpresaPublicaDto>> ObterEmpresaAsync(string token, CancellationToken ct = default)
    {
        var resultado = await ObterEmpresaPorTokenAsync(token, rastreamento: false, exigirEditavel: false, ct);
        return resultado.Sucesso
            ? Resultado<FichaEmpresaPublicaDto>.CriarSucesso(MapearEmpresa(resultado.Valor!))
            : Resultado<FichaEmpresaPublicaDto>.CriarFalha(resultado.Erro!);
    }

    public async Task<Resultado<bool>> AtualizarEmpresaAsync(
        string token,
        AtualizarFichaEmpresaRequisicao requisicao,
        CancellationToken ct = default)
    {
        var resultado = await ObterEmpresaPorTokenAsync(token, rastreamento: true, exigirEditavel: true, ct);
        if (!resultado.Sucesso)
            return Resultado<bool>.CriarFalha(resultado.Erro!);

        try
        {
            resultado.Valor!.AtualizarDadosEmpresa(
                requisicao.Pis, requisicao.DataAdmissaoPrevista, requisicao.Salario,
                requisicao.Comissao, requisicao.Funcao, requisicao.ContratoExperienciaDias,
                requisicao.UtilizaValeTransporte, requisicao.HorarioTrabalho,
                requisicao.Banco, requisicao.Agencia, requisicao.Conta, requisicao.TipoConta);
        }
        catch (ArgumentException exception)
        {
            return Resultado<bool>.CriarFalha(exception.Message);
        }

        await repositorio.SalvarAlteracoesAsync(ct);
        return Resultado<bool>.CriarSucesso(true);
    }

    public async Task<Resultado<bool>> ConcluirEmpresaAsync(string token, CancellationToken ct = default)
    {
        var resultado = await ObterEmpresaPorTokenAsync(token, rastreamento: true, exigirEditavel: true, ct);
        if (!resultado.Sucesso)
            return Resultado<bool>.CriarFalha(resultado.Erro!);

        var admissao = resultado.Valor!;
        if (admissao.DataAdmissaoPrevista is null)
            return Resultado<bool>.CriarFalha("Data de admissao e obrigatoria para concluir o preenchimento do empregador.");
        if (string.IsNullOrWhiteSpace(admissao.Funcao))
            return Resultado<bool>.CriarFalha("Funcao do funcionario e obrigatoria para concluir o preenchimento do empregador.");

        admissao.MarcarPreenchimentoEmpresaConcluido();
        var status = await repositorio.ObterOuCriarStatusAsync(
            StatusAdmissaoCodigos.LiberadoPeloEmpregador, "Liberado pelo empregador", 6, ct);
        admissao.AlterarStatus(status.Id);
        await repositorio.SalvarAlteracoesAsync(ct);
        return Resultado<bool>.CriarSucesso(true);
    }

    public async Task<Resultado<bool>> EnviarLinkPorEmailAsync(
        EnviarLinkFichaAdmissaoEmailRequisicao requisicao,
        CancellationToken ct = default)
    {
        var tenantId = empresaAtual.TenantId;
        if (tenantId == Guid.Empty)
            return Resultado<bool>.CriarFalha("Nao foi possivel identificar o parceiro da admissao.");
        if (string.IsNullOrWhiteSpace(requisicao.Email) || string.IsNullOrWhiteSpace(requisicao.Token))
            return Resultado<bool>.CriarFalha("Informe o e-mail e o link ativo da ficha.");

        MailAddress endereco;
        try
        {
            endereco = new MailAddress(requisicao.Email.Trim());
        }
        catch (FormatException)
        {
            return Resultado<bool>.CriarFalha("Informe um e-mail valido para envio do link.");
        }

        var funcionario = string.Equals(requisicao.Papel, "funcionario", StringComparison.OrdinalIgnoreCase);
        var empresa = string.Equals(requisicao.Papel, "empresa", StringComparison.OrdinalIgnoreCase);
        if (!funcionario && !empresa)
            return Resultado<bool>.CriarFalha("Tipo de ficha invalido para envio por e-mail.");

        var hash = CalcularHash(requisicao.Token);
        var admissao = funcionario
            ? await repositorio.ObterPorTokenFuncionarioHashAsync(hash, false, ct)
            : await repositorio.ObterPorTokenEmpresaHashAsync(hash, false, ct);
        var expiraEm = funcionario ? admissao?.TokenExpiraEm : admissao?.TokenEmpresaExpiraEm;
        if (admissao is null || admissao.Id != requisicao.AdmissaoId || admissao.TenantId != tenantId ||
            expiraEm is null || expiraEm <= DateTime.UtcNow ||
            StatusAdmissaoCodigos.Terminal(admissao.StatusAdmissao.Codigo))
            return Resultado<bool>.CriarFalha("O link da ficha e invalido ou expirou. Gere um novo link antes de enviar.");

        var frontend = configuracao["Acesso:FrontendBaseUrl"]?.TrimEnd('/');
        if (string.IsNullOrWhiteSpace(frontend) || !Uri.TryCreate(frontend, UriKind.Absolute, out var baseUri) ||
            baseUri.Scheme is not ("https" or "http"))
            return Resultado<bool>.CriarFalha("A URL do frontend nao esta configurada para envio de e-mails.");

        var rota = funcionario ? "ficha-admissao" : "ficha-admissao-empresa";
        var url = $"{frontend}/{rota}/{Uri.EscapeDataString(requisicao.Token)}";
        var papel = funcionario ? "admitido" : "empregador";
        var expiraEmBrasil = new DateTimeOffset(expiraEm.Value, TimeSpan.Zero)
            .ToOffset(TimeSpan.FromHours(-3));
        var corpo = $"Ola,\n\nA ficha de {papel} para {admissao.NomeFuncionario} esta disponivel para preenchimento.\n\nAcesse: {url}\n\nEste link expira em {expiraEmBrasil:dd/MM/yyyy HH:mm} (horario de Brasilia).";
        await servicoEmail.SendAsync(endereco.Address, $"Ficha de admissao de {admissao.NomeFuncionario}", corpo, ct);
        return Resultado<bool>.CriarSucesso(true);
    }

    public async Task<Resultado<ResultadoDocumentoAdmissaoDto>> EnviarDocumentoAsync(
        string token,
        Guid itemId,
        IReadOnlyList<ArquivoAdmissaoUpload> arquivos,
        CancellationToken ct = default)
    {
        var resultado = await ObterFuncionarioPorTokenAsync(token, rastreamento: false, exigirEditavel: true, ct);
        if (!resultado.Sucesso)
            return Resultado<ResultadoDocumentoAdmissaoDto>.CriarFalha(resultado.Erro!);
        var admissao = resultado.Valor!;
        if (admissao.Documentos.All(item => item.Id != itemId))
            return Resultado<ResultadoDocumentoAdmissaoDto>.CriarFalha("Documento da admissao nao encontrado.");

        return await documentoServico.EnviarAsync(new(admissao.Id, itemId, arquivos), ct);
    }

    public async Task<Resultado<ResultadoUrlDocumentoAdmissaoDto>> ObterUrlDocumentoAsync(
        string token,
        Guid itemId,
        CancellationToken ct = default)
    {
        var resultado = await ObterFuncionarioPorTokenAsync(token, rastreamento: false, exigirEditavel: false, ct);
        if (!resultado.Sucesso)
            return Resultado<ResultadoUrlDocumentoAdmissaoDto>.CriarFalha(resultado.Erro!);
        var admissao = resultado.Valor!;
        if (admissao.Documentos.All(item => item.Id != itemId))
            return Resultado<ResultadoUrlDocumentoAdmissaoDto>.CriarFalha("Documento da admissao nao encontrado.");
        return await documentoServico.ObterUrlAsync(admissao.Id, itemId, ct);
    }

    private async Task<Resultado<Admissao>> ObterFuncionarioPorTokenAsync(
        string token,
        bool rastreamento,
        bool exigirEditavel,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(token))
            return Resultado<Admissao>.CriarFalha("Link de admissao invalido ou expirado.");
        var admissao = await repositorio.ObterPorTokenFuncionarioHashAsync(CalcularHash(token), rastreamento, ct);
        if (admissao is null || admissao.TokenExpiraEm is null || admissao.TokenExpiraEm <= DateTime.UtcNow)
            return Resultado<Admissao>.CriarFalha("Link de admissao invalido ou expirado.");
        if (StatusAdmissaoCodigos.Terminal(admissao.StatusAdmissao.Codigo))
            return Resultado<Admissao>.CriarFalha("Esta admissao ja foi encerrada.");
        if (exigirEditavel && admissao.PreenchimentoFuncionarioConcluidoEm.HasValue)
            return Resultado<Admissao>.CriarFalha("A ficha do admitido ja foi finalizada e nao pode mais ser alterada.");

        empresaAtual.SetTenant(admissao.TenantId);
        return Resultado<Admissao>.CriarSucesso(admissao);
    }

    private async Task<Resultado<Admissao>> ObterEmpresaPorTokenAsync(
        string token,
        bool rastreamento,
        bool exigirEditavel,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(token))
            return Resultado<Admissao>.CriarFalha("Link de admissao invalido ou expirado.");
        var admissao = await repositorio.ObterPorTokenEmpresaHashAsync(CalcularHash(token), rastreamento, ct);
        if (admissao is null || admissao.TokenEmpresaExpiraEm is null ||
            admissao.TokenEmpresaExpiraEm <= DateTime.UtcNow)
            return Resultado<Admissao>.CriarFalha("Link de admissao invalido ou expirado.");
        if (StatusAdmissaoCodigos.Terminal(admissao.StatusAdmissao.Codigo))
            return Resultado<Admissao>.CriarFalha("Esta admissao ja foi encerrada.");
        if (!admissao.PreenchimentoFuncionarioConcluidoEm.HasValue)
            return Resultado<Admissao>.CriarFalha("O admitido ainda nao concluiu o preenchimento da ficha.");
        if (exigirEditavel && admissao.PreenchimentoEmpresaConcluidoEm.HasValue)
            return Resultado<Admissao>.CriarFalha("A ficha do empregador ja foi finalizada e nao pode mais ser alterada.");

        empresaAtual.SetTenant(admissao.TenantId);
        return Resultado<Admissao>.CriarSucesso(admissao);
    }

    private static FichaFuncionarioPublicaDto MapearFuncionario(Admissao admissao)
        => new(
            admissao.Id, admissao.Empresa.RazaoSocial, admissao.NomeFuncionario, admissao.Cpf,
            admissao.Rg, admissao.DataNascimento, admissao.Sexo, admissao.EstadoCivil,
            admissao.Escolaridade, admissao.Telefone, admissao.Email, admissao.Pis,
            admissao.Banco, admissao.Agencia, admissao.Conta, admissao.TipoConta,
            admissao.DataAdmissaoPrevista, admissao.Salario, admissao.Comissao, admissao.Funcao,
            admissao.ContratoExperienciaDias, admissao.UtilizaValeTransporte, admissao.HorarioTrabalho,
            admissao.PreenchimentoFuncionarioConcluidoEm.HasValue, admissao.TokenExpiraEm!.Value,
            admissao.Dependentes.Select(MapearDependente).ToList(),
            admissao.Documentos.OrderBy(item => item.Ordem).Select(MapearDocumento).ToList());

    private static FichaEmpresaPublicaDto MapearEmpresa(Admissao admissao)
        => new(
            admissao.Id, admissao.Empresa.RazaoSocial, admissao.NomeFuncionario, admissao.Cpf,
            admissao.Rg, admissao.DataNascimento, admissao.Sexo, admissao.EstadoCivil,
            admissao.Escolaridade, admissao.Telefone, admissao.Email, admissao.Banco,
            admissao.Agencia, admissao.Conta, admissao.TipoConta, admissao.Pis,
            admissao.DataAdmissaoPrevista, admissao.Salario, admissao.Comissao, admissao.Funcao,
            admissao.ContratoExperienciaDias, admissao.UtilizaValeTransporte, admissao.HorarioTrabalho,
            admissao.PreenchimentoEmpresaConcluidoEm.HasValue, admissao.TokenEmpresaExpiraEm!.Value);

    private static AdmissaoDependenteDto MapearDependente(AdmissaoDependente item)
        => new(item.Id, item.Nome, item.Cpf, item.DataNascimento, item.Parentesco,
            item.DependenteIr, item.SalarioFamilia);

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
            status, validacao?.ConfiancaOcr, validacao?.Observacao, [], [],
            validacao?.TipoDocumentoDetectado, validacao?.TipoDocumentoIncompativel ?? false,
            null, null, null);
    }

    private static string GerarToken()
        => Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))
            .TrimEnd('=').Replace('+', '-').Replace('/', '_');

    private static string CalcularHash(string token)
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token.Trim())));
}
