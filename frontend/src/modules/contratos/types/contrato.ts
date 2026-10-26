export interface Contrato {
  id: string
  numero: string
  prospectoId: string
  clienteId: string | null
  orcamentoId: string | null
  razaoSocial: string
  status: string
  valorTotal: number
  diaVencimento: number
  dataInicio: string
  dataFim: string | null
  duracaoAnos: number | null
  competenciaInicialHonorarios: string | null
  valorHonorarioMensal: number | null
  valorUnitarioPorFolha: number | null
  clausulasExtras: string | null
  pdfGerado: boolean
  pdfAssinadoDisponivel: boolean
  assinadoEm: string | null
  signatariosAssinatura: SignatarioAssinaturaStatus[]
}

export interface SignatarioAssinaturaStatus {
  tipo: string
  nome: string
  assinado: boolean
  assinadoEm: string | null
}

export interface EmpresaContratoFiltro {
  id: string
  razaoSocial: string
  cnpj: string | null
}

export interface SignatarioContratoInfo {
  tipo: string
  nome: string
  cpf: string
  email: string | null
}

export interface EnviarAssinaturaPayload {
  emailContratante: string
  emailContratada: string
  mensagemPersonalizada?: string
}

export interface EnvioAssinaturaResultado {
  metodo: string
  envelopeId: string | null
  signatarios: Array<{ email: string; link: string; nome: string | null; tipo: string | null }> | null
  mensagem: string | null
  pdfUrl: string | null
}
