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
  clausulasExtras: string | null
  pdfGerado: boolean
  pdfAssinadoDisponivel: boolean
  assinadoEm: string | null
}

export interface EmpresaContratoFiltro {
  id: string
  razaoSocial: string
  cnpj: string | null
}

export interface SignatarioPayload {
  nome: string
  email: string
  cpf?: string
  tipo: string
}

export interface EnviarAssinaturaPayload {
  signatarios: SignatarioPayload[]
  mensagemPersonalizada?: string
}

export interface EnvioAssinaturaResultado {
  metodo: string
  envelopeId: string | null
  signatarios: Array<{ email: string; link: string }> | null
  mensagem: string | null
  pdfUrl: string | null
}
