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
  preenchimentoFuncionarioConcluido: boolean
  preenchimentoEmpresaConcluido: boolean
}

export interface EmpresaAdmissoesResumo {
  empresaId: string
  empresaNome: string
  cnpj: string | null
  quantidadeAdmissoes: number
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

export interface AdmissaoDocumento {
  id: string
  tipoDocumentoId: string
  tipoCodigo: string
  tipoDescricao: string
  rotulo: string
  obrigatorio: boolean
  ordem: number
  documentoId: string | null
  nomeArquivo: string | null
  mimeType: string | null
  tamanhoBytes: number | null
  qualidadeEstimada: number | null
  enviadoEm: string | null
  statusValidacao: string
  confiancaOcr: number | null
  mensagemValidacao: string | null
  camposExtraidos: Record<string, string>
  divergencias: DivergenciaDocumento[]
  tipoDocumentoDetectado?: string | null
  tipoDocumentoIncompativel?: boolean
  observacaoRevisao?: string | null
  revisadoPorNome?: string | null
  revisadoEm?: string | null
  statusProcessamentoOcr?: string | null
  categoriaResultadoOcr?: string | null
  modeloOcr?: string | null
  statusHttpOcr?: number | null
  codigoErroOcr?: string | null
  idRequisicaoOcr?: string | null
  confiancasCamposOcr?: Record<string, number> | null
  diagnosticoTecnicoOcr?: string | null
  alternativas?: AdmissaoDocumento[]
}

export interface DivergenciaDocumento {
  campo: string
  valorDocumento: string
  valorCadastro: string
  similaridade: number
}

export interface UrlDocumentoAdmissao {
  url: string
  expiraEm: string
}

export interface ResultadoValidacaoDocumento {
  valido: boolean
  confiancaOcr: number
  fallbackManual: boolean
  mensagem: string | null
  divergencias: DivergenciaDocumento[]
  diagnosticoTecnicoOcr?: string | null
}

export interface LinkFichaAdmissao {
  token: string
  expiraEm: string
}

export interface FichaFuncionarioPublica {
  id: string
  empresaNome: string
  nomeFuncionario: string
  cpf: string | null
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
  dataAdmissaoPrevista: string | null
  salario: number | null
  comissao: string | null
  funcao: string | null
  contratoExperienciaDias: number | null
  utilizaValeTransporte: boolean | null
  horarioTrabalho: string | null
  preenchimentoConcluido: boolean
  expiraEm: string
  dependentes: AdmissaoDependente[]
  documentos: AdmissaoDocumento[]
}

export interface FichaEmpresaPublica {
  id: string
  empresaNome: string
  nomeFuncionario: string
  cpf: string | null
  rg: string | null
  dataNascimento: string | null
  sexo: string | null
  estadoCivil: string | null
  escolaridade: string | null
  telefone: string | null
  email: string | null
  banco: string | null
  agencia: string | null
  conta: string | null
  tipoConta: string | null
  pis: string | null
  dataAdmissaoPrevista: string | null
  salario: number | null
  comissao: string | null
  funcao: string | null
  contratoExperienciaDias: number | null
  utilizaValeTransporte: boolean | null
  horarioTrabalho: string | null
  preenchimentoConcluido: boolean
  expiraEm: string
}

export interface AtualizarFichaFuncionarioPayload {
  nomeFuncionario: string
  dataNascimento?: string
  sexo?: string
  estadoCivil?: string
  escolaridade?: string
  telefone?: string
  email?: string
  dependentes: DependenteAdmissaoPayload[]
}

export interface AtualizarFichaEmpresaPayload {
  pis?: string
  dataAdmissaoPrevista?: string
  salario?: number
  comissao?: string
  funcao?: string
  contratoExperienciaDias?: number
  utilizaValeTransporte?: boolean
  horarioTrabalho?: string
  banco?: string
  agencia?: string
  conta?: string
  tipoConta?: string
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
  empresaId?: string
  status?: string
  busca?: string
}

export const statusAdmissaoOptions = [
  { label: 'Todos', value: '' },
  { label: 'Rascunho', value: 'RASCUNHO' },
  { label: 'Aguardando admitido', value: 'AGUARDANDO_ADMITIDO' },
  { label: 'Aguardando validação', value: 'AGUARDANDO_VALIDACAO' },
  { label: 'Documentos validados', value: 'DOCUMENTOS_VALIDADOS' },
  { label: 'Aguardando empregador', value: 'AGUARDANDO_EMPREGADOR' },
  { label: 'Liberado para revisão final', value: 'LIBERADO_PARA_REVISAO_FINAL' },
  { label: 'Liberado pelo empregador', value: 'LIBERADO_PELO_EMPREGADOR' },
  { label: 'Concluída', value: 'CONCLUIDA' },
  { label: 'Cancelada', value: 'CANCELADA' },
]
