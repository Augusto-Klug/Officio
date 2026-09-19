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

export enum RegimeTributario {
  SimplesNacional = 0,
  LucroPresumido = 1,
  LucroReal = 2,
  MEI = 3,
}

export interface CriarSocioPayload {
  nome: string
  cpf: string | null
  participacao: number
}

export interface CriarProspectoPayload {
  razaoSocial: string
  cnpj: string | null
  regimeTributario: RegimeTributario | null
  socios: CriarSocioPayload[]
}

export interface ProspectoCriado {
  id: string
  razaoSocial: string
}

export const regimeTributarioOptions = [
  { label: 'Simples Nacional', value: RegimeTributario.SimplesNacional },
  { label: 'Lucro Presumido', value: RegimeTributario.LucroPresumido },
  { label: 'Lucro Real', value: RegimeTributario.LucroReal },
  { label: 'MEI', value: RegimeTributario.MEI },
] as const

export const statusOptions = [
  { label: 'Em negociação', value: 'EmNegociacao', severity: 'info' },
  { label: 'Proposta enviada', value: 'PropostaEnviada', severity: 'warn' },
  { label: 'Convertido', value: 'Convertido', severity: 'success' },
  { label: 'Perdido', value: 'Perdido', severity: 'danger' },
  { label: 'Inativo', value: 'Inativo', severity: 'secondary' },
] as const
