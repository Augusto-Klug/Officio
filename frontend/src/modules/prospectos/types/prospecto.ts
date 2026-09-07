export interface Prospecto {
  id: string
  razaoSocial: string
  cnpj: string | null
  status: string | null
  responsavel: string | null
  criadoEm: string | null
}

export interface ProspectosFiltro {
  pagina: number
  tamanhoPagina: number
  status?: string
  busca?: string
  responsavelId?: string
}

export interface ProspectosPagina {
  dados: Prospecto[]
  totalRegistros: number
  pagina: number
  tamanhoPagina: number
  totalPaginas: number
}

export const statusOptions = [
  { label: 'Em negociação', value: 'EmNegociacao', severity: 'info' },
  { label: 'Proposta enviada', value: 'PropostaEnviada', severity: 'warn' },
  { label: 'Convertido', value: 'Convertido', severity: 'success' },
  { label: 'Perdido', value: 'Perdido', severity: 'danger' },
  { label: 'Inativo', value: 'Inativo', severity: 'secondary' },
] as const
