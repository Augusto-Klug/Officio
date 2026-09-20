export type StatusFaturamento = 'ABERTO' | 'FECHADO' | 'CANCELADO'

export interface FaturamentoMensal {
  id: string
  contratoId: string
  numeroContrato: string
  cliente: string
  competencia: string
  status: StatusFaturamento
  quantidadeFuncionarios: number
  quantidadeProLabores: number
  quantidadeFolhas: number
  valorHonorarioBase: number
  valorPorFolha: number
  valorTotal: number
  calculadoEm: string
}

export interface ResultadoGeracaoFaturamento {
  competencia: string
  gerados: number
  ignorados: number
  valorTotal: number
}

export interface ResultadoRecalculoFaturamento {
  competencia: string
  recalculados: number
  ignorados: number
  valorTotal: number
}

export interface ResumoFaturamentoCompetencia {
  competencia: string
  totalRegistros: number
  abertos: number
  fechados: number
  cancelados: number
  quantidadeFolhas: number
  valorAberto: number
  valorFechado: number
  valorTotalAtivo: number
}
