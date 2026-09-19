export interface Cliente {
  id: string
  razaoSocial: string
  nomeFantasia: string | null
  cnpj: string | null
  cpf: string | null
  regimeTributario: string | null
  sciCodigo: string | null
  ativo: boolean
  email: string | null
  telefone: string | null
  cidade: string | null
  uf: string | null
  quantidadeSocios: number
  quantidadeContratos: number
  quantidadeDocumentos: number
}

export interface ClienteSocio {
  id: string
  nome: string
  cpf: string | null
  participacao: number
}

export interface ClienteContrato {
  id: string
  numero: string
  status: string
  valorTotal: number
  dataInicio: string
  dataFim: string | null
}

export interface ClienteDocumento {
  id: string
  nomeArquivo: string
  tipo: string
  legivel: boolean
  qualidadeEstimada: number
}

export interface ClienteDetalhe extends Omit<Cliente, 'quantidadeSocios' | 'quantidadeContratos' | 'quantidadeDocumentos'> {
  whatsapp: string | null
  cep: string | null
  logradouro: string | null
  numero: string | null
  bairro: string | null
  observacoes: string | null
  prospectOrigemId: string | null
  inicioRelacionamentoEm: string | null
  socios: ClienteSocio[]
  contratos: ClienteContrato[]
  documentos: ClienteDocumento[]
}

export interface ClientesFiltro {
  busca?: string
  ativo?: boolean
}
