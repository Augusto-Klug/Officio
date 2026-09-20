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
