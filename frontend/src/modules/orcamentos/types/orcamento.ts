export interface ItemOrcamento {
  id: string
  itemCatalogoOrcamentoId: string | null
  descricao: string
  secao: string
  tipo: string
  quantidade: number
  valorUnitario: number
  valorTotal: number
  ordem: number
}

export interface Orcamento {
  id: string
  prospectoId: string
  titulo: string | null
  status: string
  valorTotal: number
  diaVencimento: number
  observacoes: string | null
  motivoRecusa: string | null
  itens: ItemOrcamento[]
}

export interface NovoItemOrcamento {
  itemCatalogoOrcamentoId?: string
  descricao: string
  secao: number
  tipo: number
  quantidade: number
  valorUnitario: number
  ordem: number
}

export interface CriarOrcamentoPayload {
  prospectoId: string
  titulo: string
  diaVencimento: number
  observacoes?: string
  itens: NovoItemOrcamento[]
}

export type AtualizarOrcamentoPayload = Omit<CriarOrcamentoPayload, 'prospectoId'>

export interface ConverterContratoPayload {
  dataInicio: string
  dataFim?: string
  renovacaoAutomatica: boolean
  clausulasExtras?: string
}

export const secaoOptions = [
  { label: 'Serviços inclusos', value: 1 },
  { label: 'Honorários', value: 2 },
  { label: 'Serviços separados', value: 3 },
  { label: 'Criação de empresa', value: 4 },
] as const

export const tipoOptions = [
  { label: 'Descrição de serviço', value: 1 },
  { label: 'Item monetário', value: 2 },
  { label: 'Regra comercial', value: 3 },
] as const

export const secaoPorNome: Record<string, number> = {
  ServicosInclusos: 1,
  Honorarios: 2,
  ServicosSeparados: 3,
  CriacaoEmpresa: 4,
}

export const tipoPorNome: Record<string, number> = {
  ServicoDescricao: 1,
  ItemMonetario: 2,
  RegraComercial: 3,
}
