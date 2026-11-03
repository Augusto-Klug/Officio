export type PerfilUsuario = string
export type PerfilGerenciavelUsuario = 'Admin' | 'Operacional' | 'Consulta'
export type SituacaoUsuario = 'ATIVO' | 'INATIVO' | 'CONVITE_PENDENTE' | 'CONVITE_EXPIRADO'

export interface EmpresaVinculadaUsuario {
  id: string
  razaoSocial: string
  cnpj: string
  empresaAtiva: boolean
  vinculoAtivo: boolean
}

export interface UsuarioAcesso {
  id: string
  nome: string
  email: string
  perfil: PerfilUsuario
  situacao: SituacaoUsuario
  ativo: boolean
  convidadoEm: string | null
  ultimoAcessoEm: string | null
  empresas: EmpresaVinculadaUsuario[]
}

export interface ConvidarUsuarioInput {
  nome: string
  email: string
  perfil: PerfilGerenciavelUsuario
}
