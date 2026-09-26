export interface ParceiroPlataforma {
  id: string
  razaoSocial: string
  nomeFantasia: string | null
  cnpj: string
  ativo: boolean
  criadaEm: string | null
  quantidadeUsuarios: number
  administradorEmail: string | null
}

export interface CriarParceiroInput {
  razaoSocial: string
  nomeFantasia: string | null
  cnpj: string
  administrador: { nome: string; email: string }
}
