using Officio.Dominio.Contratos;
using Officio.Dominio.Contratos.Dtos;
using Officio.Dominio.Contratos.Response;
using Officio.Dominio.Entidades;
using Officio.Dominio.Interfaces;

namespace Officio.Dominio.Servicos.Servicos;

public class FormularioAberturaServico(IFormularioAberturaRepositorio repositorio) : IFormularioAberturaServico
{
    private const string StatusRascunho = "RASCUNHO";

    public async Task<Resultado<FormularioAberturaDto>> ObterPorProspectoAsync(Guid prospectoId, CancellationToken ct = default)
    {
        var formulario = await repositorio.ObterPorProspectoAsync(prospectoId, ct);
        return formulario is null
            ? Resultado<FormularioAberturaDto>.CriarFalha("Formulario de abertura nao encontrado.")
            : Resultado<FormularioAberturaDto>.CriarSucesso(Mapear(formulario));
    }

    public async Task<Resultado<FormularioAberturaDto>> AtualizarAsync(
        AtualizarFormularioAberturaRequisicao requisicao,
        CancellationToken ct = default)
    {
        var prospecto = await repositorio.ObterProspectoAsync(requisicao.ProspectoId, ct);
        if (prospecto is null)
            return Resultado<FormularioAberturaDto>.CriarFalha("Prospecto nao encontrado.");
        if (!string.Equals(prospecto.TipoProspect, "NOVA_EMPRESA", StringComparison.Ordinal))
            return Resultado<FormularioAberturaDto>.CriarFalha("Formulario de abertura esta disponivel somente para prospectos de nova empresa.");

        var formulario = await repositorio.ObterPorProspectoAsync(requisicao.ProspectoId, ct);
        if (formulario is null)
        {
            var status = await repositorio.ObterOuCriarStatusAsync(StatusRascunho, "Rascunho", 1, ct);
            formulario = FormularioAbertura.Criar(requisicao.ProspectoId, status.Id);
            await repositorio.AdicionarAsync(formulario, ct);
        }

        try
        {
            var atividades = requisicao.Atividades
                .Select(item => FormularioAberturaAtividade.Criar(
                    formulario.Id,
                    item.DescricaoAtividade,
                    item.Cnae,
                    item.Percentual,
                    item.RealizaNoEndereco,
                    item.Ordem))
                .ToList();

            formulario.Atualizar(
                requisicao.PessoaResponsavelId,
                requisicao.EnderecoId,
                requisicao.NomeEmpresaPretendido,
                requisicao.NomeFantasiaPretendido,
                requisicao.CadastroIptu,
                requisicao.NomeEdificacao,
                requisicao.ProprietarioImovelNome,
                requisicao.ProprietarioImovelCpfCnpj,
                requisicao.AreaTotalEdificacaoM2,
                requisicao.AreaOcupadaEmpresaM2,
                requisicao.CapitalSocialPrevisto,
                requisicao.EmailEmpresaPretendido,
                requisicao.TelefoneEmpresaPretendido,
                requisicao.NumeroFuncionariosPrevisto,
                requisicao.NumeroProLaboresPrevisto,
                requisicao.LotacaoMaximaPublico,
                requisicao.HorarioFuncionamento,
                requisicao.DiasTrabalhadosMes,
                requisicao.SituacaoImovel,
                requisicao.NumeroMaquinas,
                requisicao.TipoMaquinas,
                requisicao.EnquadramentoPretendido,
                requisicao.Observacoes,
                requisicao.TipoEmpresaPretendido,
                requisicao.RegimeTributarioPretendido);
            await repositorio.RemoverAtividadesAsync(formulario.Atividades, ct);
            formulario.SubstituirAtividades(atividades);
            await repositorio.AdicionarAtividadesAsync(atividades, ct);
        }
        catch (ArgumentException exception)
        {
            return Resultado<FormularioAberturaDto>.CriarFalha(exception.Message);
        }

        await repositorio.SalvarAlteracoesAsync(ct);
        return Resultado<FormularioAberturaDto>.CriarSucesso(Mapear(formulario));
    }

    private static FormularioAberturaDto Mapear(FormularioAbertura item)
        => new(
            item.Id, item.ProspectId, item.StatusFormulario.Codigo, item.PessoaResponsavelId, item.EnderecoId,
            item.NomeEmpresaPretendido, item.NomeFantasiaPretendido, item.CadastroIptu, item.NomeEdificacao,
            item.ProprietarioImovelNome, item.ProprietarioImovelCpfCnpj, item.AreaTotalEdificacaoM2,
            item.AreaOcupadaEmpresaM2, item.CapitalSocialPrevisto, item.EmailEmpresaPretendido,
            item.TelefoneEmpresaPretendido, item.NumeroFuncionariosPrevisto, item.NumeroProLaboresPrevisto,
            item.LotacaoMaximaPublico, item.HorarioFuncionamento, item.DiasTrabalhadosMes, item.SituacaoImovel,
            item.NumeroMaquinas, item.TipoMaquinas, item.EnquadramentoPretendido, item.Observacoes,
            item.TipoEmpresaPretendido, item.RegimeTributarioPretendido,
            item.Atividades.OrderBy(activity => activity.Ordem ?? int.MaxValue).Select(activity => new AtividadeFormularioAberturaDto(
                activity.Id, activity.DescricaoAtividade, activity.Cnae, activity.Percentual,
                activity.RealizaNoEndereco, activity.Ordem)).ToList(),
            item.CriadoEm, item.AtualizadoEm);
}
