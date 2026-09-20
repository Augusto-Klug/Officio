using Officio.Dominio.Contratos;
using Officio.Dominio.Contratos.Dtos;
using Officio.Dominio.Contratos.Response;
using Officio.Dominio.Entidades;
using Officio.Dominio.Enumeradores;
using Officio.Dominio.Interfaces;

namespace Officio.Dominio.Servicos.Servicos;

public class FaturamentoServico(IFaturamentoRepositorio repositorio) : IFaturamentoServico
{
    public async Task<Resultado<ResultadoGeracaoFaturamentoDto>> GerarCompetenciaAsync(
        GerarFaturamentoCompetenciaRequisicao requisicao,
        CancellationToken ct = default)
    {
        var competencia = NormalizarCompetencia(requisicao.Competencia);
        var statusAberto = await repositorio.ObterOuCriarStatusAsync("ABERTO", "Aberto", ct);
        var contratos = await repositorio.ListarContratosAptosAsync(competencia, ct);
        var calculados = await repositorio.ListarContratosJaCalculadosAsync(competencia, ct);
        var gerados = new List<FaturamentoMensal>();

        foreach (var contrato in contratos.Where(item => !calculados.Contains(item.Id)))
        {
            var empresa = contrato.Cliente?.Empresa ?? contrato.Empresa ?? contrato.Prospecto?.Empresa;
            var orcamento = contrato.Orcamento;
            var usaCamposLegados = orcamento is not null
                && (orcamento.ValorMensalidade > 0 || orcamento.ValorPorFuncionario > 0);
            var honorarioBase = usaCamposLegados
                ? orcamento!.ValorMensalidade
                : orcamento?.Itens
                    .Where(item => item.NaturezaCobranca == NaturezaCobrancaItemOrcamento.HonorarioMensal)
                    .Sum(item => item.ValorTotal) ?? contrato.ValorTotal;
            var valorPorFolha = usaCamposLegados
                ? orcamento!.ValorPorFuncionario
                : orcamento?.Itens
                    .Where(item => item.NaturezaCobranca == NaturezaCobrancaItemOrcamento.PorFolha)
                    .Sum(item => item.ValorTotal) ?? 0m;

            var faturamento = FaturamentoMensal.Criar(
                contrato.TenantId,
                contrato.Id,
                statusAberto.Id,
                competencia,
                empresa?.QuantidadeFuncionariosAtual ?? 0,
                empresa?.QuantidadeProLaboresAtual ?? 0,
                honorarioBase,
                valorPorFolha);
            await repositorio.AdicionarAsync(faturamento, ct);
            gerados.Add(faturamento);
        }

        await repositorio.SalvarAlteracoesAsync(ct);
        return Resultado<ResultadoGeracaoFaturamentoDto>.CriarSucesso(new(
            competencia,
            gerados.Count,
            contratos.Count - gerados.Count,
            gerados.Sum(item => item.ValorTotal)));
    }

    public async Task<Resultado<List<FaturamentoMensalDto>>> ListarAsync(
        DateOnly? competencia,
        string? status,
        CancellationToken ct = default)
    {
        DateOnly? normalizada = competencia.HasValue ? NormalizarCompetencia(competencia.Value) : null;
        var faturamentos = await repositorio.ListarAsync(normalizada, status, ct);
        return Resultado<List<FaturamentoMensalDto>>.CriarSucesso(faturamentos.Select(Mapear).ToList());
    }

    public Task<Resultado<bool>> FecharAsync(Guid id, CancellationToken ct = default)
        => AlterarStatusAsync(id, "FECHADO", "Fechado", true, ct);

    public Task<Resultado<bool>> CancelarAsync(Guid id, CancellationToken ct = default)
        => AlterarStatusAsync(id, "CANCELADO", "Cancelado", false, ct);

    private async Task<Resultado<bool>> AlterarStatusAsync(
        Guid id,
        string codigo,
        string descricao,
        bool fechar,
        CancellationToken ct)
    {
        var faturamento = await repositorio.ObterPorIdAsync(id, ct);
        if (faturamento is null)
            return Resultado<bool>.CriarFalha("Faturamento nao encontrado.");

        var status = await repositorio.ObterOuCriarStatusAsync(codigo, descricao, ct);
        try
        {
            if (fechar) faturamento.Fechar(status.Id);
            else faturamento.Cancelar(status.Id);
        }
        catch (InvalidOperationException exception)
        {
            return Resultado<bool>.CriarFalha(exception.Message);
        }

        await repositorio.SalvarAlteracoesAsync(ct);
        return Resultado<bool>.CriarSucesso(true);
    }

    private static DateOnly NormalizarCompetencia(DateOnly competencia)
        => new(competencia.Year, competencia.Month, 1);

    private static FaturamentoMensalDto Mapear(FaturamentoMensal item)
        => new(
            item.Id,
            item.ContratoId,
            item.Contrato.Numero,
            item.Contrato.Cliente?.RazaoSocial ?? item.Contrato.Prospecto.RazaoSocial,
            item.Competencia,
            item.StatusFaturamento.Codigo,
            item.QuantidadeFuncionarios,
            item.QuantidadeProLabores,
            item.QuantidadeFolhas,
            item.ValorHonorarioBase,
            item.ValorPorFolha,
            item.ValorTotal,
            item.CalculadoEm);
}
