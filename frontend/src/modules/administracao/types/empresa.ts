export interface EmpresaPlataforma {
  id: string
  razaoSocial: string
  nomeFantasia: string | null
  cnpj: string
  ativa: boolean
  criadaEm: string | null
  quantidadeUsuarios: number
  administradorEmail: string | null
}

export interface CriarEmpresaInput {
  razaoSocial: string
  nomeFantasia: string | null
  cnpj: string
  administrador: { nome: string; email: string }
}
