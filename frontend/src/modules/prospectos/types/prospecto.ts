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
  id?: string
  nome: string
  cpf: string | null
  participacao: number | null
  pessoaId?: string
  profissao?: string | null
  tipoRelacaoId?: string | null
  endereco?: Partial<import("../../../components/shared/endereco").EnderecoCadastro> | null
}

export interface CriarProspectoPayload {
  razaoSocial: string
  nomeFantasia?: string | null
  cnpj: string | null
  cpf?: string | null
  responsavelId?: string | null
  inscricaoEstadual?: string | null
  inscricaoMunicipal?: string | null
  tipoEmpresa?: string | null
  regimeTributario: RegimeTributario | null
  porteEmpresa?: string | null
  cnaePrincipal?: string | null
  dataAbertura?: string | null
  email?: string | null
  telefone?: string | null
  whatsapp?: string | null
  nomeContato?: string | null
  cep?: string | null
  logradouro?: string | null
  numero?: string | null
  complemento?: string | null
  bairro?: string | null
  cidade?: string | null
  uf?: string | null
  municipioCodigoIbge?: string | null
  origem?: string | null
  socios: CriarSocioPayload[]
}

export interface ProspectoCriado {
  id: string
  razaoSocial: string
}

export interface ProspectoDetalhe {
  id: string
  status: string
  dados: CriarProspectoPayload
}

export function prospectoNaEtapaInicial(prospecto: Pick<Prospecto, 'status'>): boolean {
  return prospecto.status === 'EmNegociacao'
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
