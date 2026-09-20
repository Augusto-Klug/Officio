import { field, record, requiredString } from '../../../api/contracts'
import { http } from '../../../api/http'
import type { FaturamentoMensal, ResultadoGeracaoFaturamento, ResultadoRecalculoFaturamento, ResumoFaturamentoCompetencia, StatusFaturamento } from '../types/faturamento'

function numero(value: unknown): number {
  if (typeof value !== 'number' || !Number.isFinite(value) || value < 0) throw new Error('A API retornou um valor de faturamento inválido.')
  return value
}

function inteiro(value: unknown): number {
  const resultado = numero(value)
  if (!Number.isInteger(resultado)) throw new Error('A API retornou uma quantidade de faturamento inválida.')
  return resultado
}

function status(value: unknown): StatusFaturamento {
  if (value !== 'ABERTO' && value !== 'FECHADO' && value !== 'CANCELADO') throw new Error('A API retornou um status de faturamento inválido.')
  return value
}

export function normalizarFaturamentos(value: unknown): FaturamentoMensal[] {
  if (!Array.isArray(value)) throw new Error('A API retornou os faturamentos em formato inesperado.')
  return value.map((conteudo) => {
    const item = record(conteudo)
    return {
      id: requiredString(field(item, 'id')),
      contratoId: requiredString(field(item, 'contratoId')),
      numeroContrato: requiredString(field(item, 'numeroContrato')),
      cliente: requiredString(field(item, 'cliente')),
      competencia: requiredString(field(item, 'competencia')),
      status: status(field(item, 'status')),
      quantidadeFuncionarios: inteiro(field(item, 'quantidadeFuncionarios')),
      quantidadeProLabores: inteiro(field(item, 'quantidadeProLabores')),
      quantidadeFolhas: inteiro(field(item, 'quantidadeFolhas')),
      valorHonorarioBase: numero(field(item, 'valorHonorarioBase')),
      valorPorFolha: numero(field(item, 'valorPorFolha')),
      valorTotal: numero(field(item, 'valorTotal')),
      calculadoEm: requiredString(field(item, 'calculadoEm')),
    }
  })
}

export async function listarFaturamentos(competencia?: string, situacao?: string, signal?: AbortSignal) {
  const { data } = await http.get<unknown>('/v1/faturamentos/Listar', {
    params: { competencia: competencia ? `${competencia}-01` : undefined, status: situacao || undefined },
    signal,
  })
  return normalizarFaturamentos(data)
}

export async function gerarCompetencia(competencia: string): Promise<ResultadoGeracaoFaturamento> {
  const { data } = await http.post<unknown>('/v1/faturamentos/GerarCompetencia', { competencia: `${competencia}-01` })
  const item = record(data)
  return {
    competencia: requiredString(field(item, 'competencia')),
    gerados: inteiro(field(item, 'gerados')),
    ignorados: inteiro(field(item, 'ignorados')),
    valorTotal: numero(field(item, 'valorTotal')),
  }
}

export async function recalcularCompetencia(competencia: string): Promise<ResultadoRecalculoFaturamento> {
  const { data } = await http.post<unknown>('/v1/faturamentos/RecalcularCompetencia', { competencia: `${competencia}-01` })
  const item = record(data)
  return {
    competencia: requiredString(field(item, 'competencia')),
    recalculados: inteiro(field(item, 'recalculados')),
    ignorados: inteiro(field(item, 'ignorados')),
    valorTotal: numero(field(item, 'valorTotal')),
  }
}

export async function obterResumoFaturamento(
  competencia: string,
  signal?: AbortSignal,
): Promise<ResumoFaturamentoCompetencia> {
  const { data } = await http.get<unknown>('/v1/faturamentos/Resumo', {
    params: { competencia: `${competencia}-01` },
    signal,
  })
  const item = record(data)
  return {
    competencia: requiredString(field(item, 'competencia')),
    totalRegistros: inteiro(field(item, 'totalRegistros')),
    abertos: inteiro(field(item, 'abertos')),
    fechados: inteiro(field(item, 'fechados')),
    cancelados: inteiro(field(item, 'cancelados')),
    quantidadeFolhas: inteiro(field(item, 'quantidadeFolhas')),
    valorAberto: numero(field(item, 'valorAberto')),
    valorFechado: numero(field(item, 'valorFechado')),
    valorTotalAtivo: numero(field(item, 'valorTotalAtivo')),
  }
}

export async function fecharFaturamento(id: string): Promise<void> {
  await http.post(`/v1/faturamentos/Fechar/${id}`)
}

export async function cancelarFaturamento(id: string): Promise<void> {
  await http.post(`/v1/faturamentos/Cancelar/${id}`)
}
