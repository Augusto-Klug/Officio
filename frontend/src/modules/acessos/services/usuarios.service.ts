import { field, optionalString, record, requiredString } from '../../../api/contracts'
import { http } from '../../../api/http'
import type { ConvidarUsuarioInput, PerfilUsuario, SituacaoUsuario, UsuarioAcesso } from '../types/usuario'

const perfis: PerfilUsuario[] = ['Admin', 'Comercial', 'Operacional', 'Financeiro', 'Consulta']
const situacoes: SituacaoUsuario[] = ['ATIVO', 'INATIVO', 'CONVITE_PENDENTE', 'CONVITE_EXPIRADO']

export function normalizeUsuario(value: unknown): UsuarioAcesso {
  const row = record(value)
  const perfilRaw = requiredString(field(row, 'perfil'))
  const perfil = perfis.find((item) => item.toLowerCase() === perfilRaw.toLowerCase())
  if (!perfil) throw new Error('A API retornou um perfil de usuário desconhecido.')
  const ativo = field(row, 'ativo') !== false
  const situacaoRaw = optionalString(field(row, 'situacao', 'status'))?.toUpperCase()
  const situacao = situacoes.find((item) => item === situacaoRaw) || (ativo ? 'ATIVO' : 'INATIVO')
  return {
    id: requiredString(field(row, 'id')),
    nome: requiredString(field(row, 'nome')),
    email: requiredString(field(row, 'email')),
    perfil,
    situacao,
    ativo,
    convidadoEm: optionalString(field(row, 'convidadoEm', 'criadoEm')),
    ultimoAcessoEm: optionalString(field(row, 'ultimoAcessoEm')),
  }
}

export async function listarUsuarios(signal?: AbortSignal): Promise<UsuarioAcesso[]> {
  const [usuariosResponse, convitesResponse] = await Promise.all([
    http.get<unknown>('/v1/usuarios', { signal }),
    http.get<unknown>('/v1/usuarios/convites', { signal }),
  ])
  if (!Array.isArray(usuariosResponse.data) || !Array.isArray(convitesResponse.data)) throw new Error('A API retornou uma listagem de usuários em formato inesperado.')
  const usuarios = usuariosResponse.data.map(normalizeUsuario)
  const convites = convitesResponse.data
    .map((value) => {
      const row = record(value)
      const status = requiredString(field(row, 'status')).toUpperCase()
      const mapped = status === 'PENDENTE' ? 'CONVITE_PENDENTE' : status === 'EXPIRADO' ? 'CONVITE_EXPIRADO' : status
      return normalizeUsuario({ ...row, situacao: mapped, ativo: false, convidadoEm: field(row, 'criadoEm') })
    })
    .filter((item) => item.situacao === 'CONVITE_PENDENTE' || item.situacao === 'CONVITE_EXPIRADO')
  return [...usuarios, ...convites]
}

export async function convidarUsuario(input: ConvidarUsuarioInput): Promise<void> {
  await http.post('/v1/usuarios/convites', { ...input, nome: input.nome.trim(), email: input.email.trim() })
}

export async function alterarPerfil(id: string, perfil: PerfilUsuario): Promise<void> {
  await http.patch(`/v1/usuarios/${id}/perfil`, { perfil })
}

export async function alterarStatus(id: string, ativo: boolean): Promise<void> {
  await http.patch(`/v1/usuarios/${id}/status`, { ativo })
}

export async function reenviarConvite(id: string): Promise<void> {
  await http.post(`/v1/usuarios/convites/${id}/reenviar`)
}

export async function cancelarConvite(id: string): Promise<void> {
  await http.post(`/v1/usuarios/convites/${id}/cancelar`)
}
