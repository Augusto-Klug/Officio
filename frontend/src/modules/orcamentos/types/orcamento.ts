export interface ItemOrcamento {
  id: string
  itemCatalogoOrcamentoId: string | null
  descricao: string
  secao: string
  tipo: string
  naturezaCobranca: string
  quantidade: number
  valorUnitario: number
  valorTotal: number
  ordem: number
}

export interface ItemCatalogoOrcamento {
  id: string
  descricao: string
  secao: string
  tipo: string
  naturezaCobranca: string
  valorReferencia: number | null
  ativo: boolean
}

export interface ItemCatalogoOrcamentoPayload {
  descricao: string
  secao: number
  tipo: number
  naturezaCobranca: number
  valorReferencia: number | null
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
  itemOrcamentoId?: string
  itemCatalogoOrcamentoId?: string
  descricao: string
  secao: number
  tipo: number
  naturezaCobranca: number
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
  confirmarSubstituicaoContratoAtivo: boolean
  clausulasExtras?: string
}

export interface ContratoAtivoResumo {
  id: string
  numero: string
  dataInicio: string
  dataFim: string | null
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

export const naturezaCobrancaOptions = [
  { label: 'Não recorrente', value: 1 },
  { label: 'Honorário mensal', value: 2 },
  { label: 'Por folha', value: 3 },
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

export const naturezaCobrancaPorNome: Record<string, number> = {
  NaoRecorrente: 1,
  HonorarioMensal: 2,
  PorFolha: 3,
}
