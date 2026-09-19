export interface AdmissaoResumo {
  id: string
  empresaId: string
  empresaNome: string
  nomeFuncionario: string
  cpf: string | null
  status: string
  statusDescricao: string
  dataAdmissaoPrevista: string | null
  funcao: string | null
  salario: number | null
  criadoEm: string
}

export interface AdmissaoDependente {
  id: string
  nome: string
  cpf: string | null
  dataNascimento: string | null
  parentesco: string | null
  dependenteIr: boolean | null
  salarioFamilia: boolean | null
}

export interface AdmissaoDetalhe extends AdmissaoResumo {
  rg: string | null
  dataNascimento: string | null
  sexo: string | null
  estadoCivil: string | null
  escolaridade: string | null
  telefone: string | null
  email: string | null
  pis: string | null
  banco: string | null
  agencia: string | null
  conta: string | null
  tipoConta: string | null
  comissao: string | null
  contratoExperienciaDias: number | null
  utilizaValeTransporte: boolean | null
  horarioTrabalho: string | null
  observacoes: string | null
  atualizadoEm: string
  dependentes: AdmissaoDependente[]
}

export interface DependenteAdmissaoPayload {
  nome: string
  cpf?: string
  dataNascimento?: string
  parentesco?: string
  dependenteIr?: boolean
  salarioFamilia?: boolean
}

export interface DadosAdmissaoPayload {
  nomeFuncionario: string
  cpf?: string
  rg?: string
  dataNascimento?: string
  sexo?: string
  estadoCivil?: string
  escolaridade?: string
  telefone?: string
  email?: string
  pis?: string
  banco?: string
  agencia?: string
  conta?: string
  tipoConta?: string
  dataAdmissaoPrevista?: string
  salario?: number
  comissao?: string
  funcao?: string
  contratoExperienciaDias?: number
  utilizaValeTransporte?: boolean
  horarioTrabalho?: string
  observacoes?: string
  dependentes: DependenteAdmissaoPayload[]
}

export interface CriarAdmissaoPayload {
  clienteId: string
  dados: DadosAdmissaoPayload
}

export interface AdmissoesFiltro {
  clienteId?: string
  status?: string
  busca?: string
}

export const statusAdmissaoOptions = [
  { label: 'Todos', value: '' },
  { label: 'Rascunho', value: 'RASCUNHO' },
  { label: 'Concluída', value: 'CONCLUIDA' },
  { label: 'Cancelada', value: 'CANCELADA' },
]
