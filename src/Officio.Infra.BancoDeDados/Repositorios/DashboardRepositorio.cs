using Microsoft.EntityFrameworkCore;
using Officio.Dominio.Admissoes;
using Officio.Dominio.Contratos.Dtos;
using Officio.Dominio.Enumeradores;
using Officio.Dominio.Interfaces;
using Officio.Infra.BancoDeDados.Contextos;

namespace Officio.Infra.BancoDeDados.Repositorios;

internal class DashboardRepositorio(ContextoAplicacao contexto) : IDashboardRepositorio
{
    public async Task<DashboardVisaoGeralDto> ObterVisaoGeralAsync(
        DateOnly competencia,
        CancellationToken cancellationToken = default)
    {
        var prospectos = await contexto.Prospects.CountAsync(
            item => item.Status == StatusProspecto.EmNegociacao || item.Status == StatusProspecto.PropostaEnviada,
            cancellationToken);
        var clientes = await contexto.Clients.CountAsync(item => item.Ativo, cancellationToken);
        var contratos = await contexto.Contracts.CountAsync(item => item.Status == StatusContrato.Ativo, cancellationToken);
        var admissoes = await contexto.Admissoes.CountAsync(
            item => item.StatusAdmissao.Codigo != StatusAdmissaoCodigos.Concluida
                    && item.StatusAdmissao.Codigo != StatusAdmissaoCodigos.Cancelada,
            cancellationToken);
        var orcamentos = await contexto.Budgets.CountAsync(item => item.Status == StatusOrcamento.Enviado, cancellationToken);
        var faturamentoCompetencia = await contexto.FaturamentosMensais
            .Where(item => item.Competencia == competencia
                           && item.StatusFaturamento.Codigo != "CANCELADO")
            .SumAsync(item => item.ValorTotal, cancellationToken);

        return new(
            prospectos,
            clientes,
            contratos,
            admissoes,
            orcamentos,
            faturamentoCompetencia);
    }

    public async Task<List<DashboardAlertaDto>> ListarAlertasAsync(
        DateTime inicioContratos,
        DateTime limiteContratos,
        DateTime limiteProspectosSemRetorno,
        int limite,
        CancellationToken cancellationToken = default)
    {
        var admissoes = await contexto.Admissoes
            .Where(item => item.StatusAdmissao.Codigo == StatusAdmissaoCodigos.PendenteRevisao)
            .Select(item => new DashboardAlertaDto(
                "ADMISSAO_PENDENTE_REVISAO",
                "erro",
                "Admissao aguardando revisao",
                item.NomeFuncionario,
                item.Id,
                "/admissoes",
                item.AtualizadoEm))
            .ToListAsync(cancellationToken);

        var assinaturas = await contexto.Contracts
            .Where(item => item.Status == StatusContrato.AguardandoAssinatura)
            .Select(item => new DashboardAlertaDto(
                "CONTRATO_AGUARDANDO_ASSINATURA",
                "aviso",
                "Contrato aguardando assinatura",
                item.Numero,
                item.Id,
                "/contratos",
                item.AtualizadoEm))
            .ToListAsync(cancellationToken);

        var vencimentos = await contexto.Contracts
            .Where(item => item.Status == StatusContrato.Ativo
                           && item.DataFim != null
                           && item.DataFim >= inicioContratos
                           && item.DataFim <= limiteContratos)
            .Select(item => new DashboardAlertaDto(
                "CONTRATO_PROXIMO_VENCIMENTO",
                "aviso",
                "Contrato proximo do vencimento",
                item.Numero,
                item.Id,
                "/contratos",
                item.DataFim!.Value))
            .ToListAsync(cancellationToken);

        var prospectos = await contexto.Prospects
            .Where(item => item.Status == StatusProspecto.PropostaEnviada
                           && item.AtualizadoEm <= limiteProspectosSemRetorno)
            .Select(item => new DashboardAlertaDto(
                "PROSPECTO_SEM_RETORNO",
                "info",
                "Proposta sem retorno recente",
                item.RazaoSocial,
                item.Id,
                "/prospectos",
                item.AtualizadoEm))
            .ToListAsync(cancellationToken);

        return admissoes
            .Concat(assinaturas)
            .Concat(vencimentos)
            .Concat(prospectos)
            .OrderBy(item => item.Severidade switch { "erro" => 0, "aviso" => 1, _ => 2 })
            .ThenBy(item => item.DataReferencia)
            .Take(limite)
            .ToList();
    }

    public Task<List<DashboardContratoReceitaDto>> ListarContratosParaProjecaoAsync(
        DateTime inicio,
        DateTime fim,
        CancellationToken cancellationToken = default)
        => contexto.Contracts
            .Where(item => item.Status == StatusContrato.Ativo
                           && item.DataInicio <= fim
                           && (item.DataFim == null || item.DataFim >= inicio))
            .Select(item => new DashboardContratoReceitaDto(
                item.DataInicio,
                item.DataFim,
                item.Orcamento != null && (item.Orcamento.ValorMensalidade > 0 || item.Orcamento.ValorPorFuncionario > 0)
                    ? item.Orcamento.ValorMensalidade
                      + ((item.Cliente != null && item.Cliente.Empresa != null
                              ? item.Cliente.Empresa.QuantidadeFuncionariosAtual + item.Cliente.Empresa.QuantidadeProLaboresAtual
                              : item.Empresa != null
                                  ? item.Empresa.QuantidadeFuncionariosAtual + item.Empresa.QuantidadeProLaboresAtual
                                  : item.Prospecto.Empresa != null
                                      ? item.Prospecto.Empresa.QuantidadeFuncionariosAtual + item.Prospecto.Empresa.QuantidadeProLaboresAtual
                                      : 0) * item.Orcamento.ValorPorFuncionario)
                    : item.Orcamento != null
                        ? item.Orcamento.Itens
                              .Where(itemOrcamento => itemOrcamento.NaturezaCobranca == NaturezaCobrancaItemOrcamento.HonorarioMensal)
                              .Sum(itemOrcamento => itemOrcamento.Quantidade * itemOrcamento.ValorUnitario)
                          + ((item.Cliente != null && item.Cliente.Empresa != null
                                  ? item.Cliente.Empresa.QuantidadeFuncionariosAtual + item.Cliente.Empresa.QuantidadeProLaboresAtual
                                  : item.Empresa != null
                                      ? item.Empresa.QuantidadeFuncionariosAtual + item.Empresa.QuantidadeProLaboresAtual
                                      : item.Prospecto.Empresa != null
                                          ? item.Prospecto.Empresa.QuantidadeFuncionariosAtual + item.Prospecto.Empresa.QuantidadeProLaboresAtual
                                          : 0)
                              * item.Orcamento.Itens
                                  .Where(itemOrcamento => itemOrcamento.NaturezaCobranca == NaturezaCobrancaItemOrcamento.PorFolha)
                                  .Sum(itemOrcamento => itemOrcamento.Quantidade * itemOrcamento.ValorUnitario))
                        : item.ValorTotal))
            .ToListAsync(cancellationToken);
}
