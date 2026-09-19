export type PerfilUsuario = 'Admin' | 'Comercial' | 'Operacional' | 'Financeiro' | 'Consulta'
export type SituacaoUsuario = 'ATIVO' | 'INATIVO' | 'CONVITE_PENDENTE' | 'CONVITE_EXPIRADO'

export interface UsuarioAcesso {
  id: string
  nome: string
  email: string
  perfil: PerfilUsuario
  situacao: SituacaoUsuario
  ativo: boolean
  convidadoEm: string | null
  ultimoAcessoEm: string | null
}

export interface ConvidarUsuarioInput {
  nome: string
  email: string
  perfil: PerfilUsuario
}
