import { field, optionalString, record, requiredString } from '../../../api/contracts'
import { http } from '../../../api/http'
import type { ConvidarUsuarioInput, PerfilGerenciavelUsuario, SituacaoUsuario, UsuarioAcesso, EmpresaVinculadaUsuario } from '../types/usuario'

const situacoes: SituacaoUsuario[] = ['ATIVO', 'INATIVO', 'CONVITE_PENDENTE', 'CONVITE_EXPIRADO']

function normalizeEmpresa(value: unknown): EmpresaVinculadaUsuario {
  const row = record(value)
  return {
    id: requiredString(field(row, 'id')),
    razaoSocial: requiredString(field(row, 'razaoSocial', 'nome')),
    cnpj: optionalString(field(row, 'cnpj')) || '',
    empresaAtiva: field(row, 'empresaAtiva', 'ativa') !== false,
    vinculoAtivo: field(row, 'vinculoAtivo') !== false,
  }
}

export function normalizeUsuario(value: unknown): UsuarioAcesso {
  const row = record(value)
  const perfilRaw = optionalString(field(row, 'perfil')) || 'Desconhecido'
  const perfilConhecido = ['Admin', 'Operacional', 'Consulta', 'Cliente', 'Comercial', 'Financeiro']
    .find((item) => item.toLowerCase() === perfilRaw.toLowerCase())
  const ativo = field(row, 'ativo') !== false
  const situacaoRaw = optionalString(field(row, 'situacao', 'status'))?.toUpperCase()
  const situacao = situacoes.find((item) => item === situacaoRaw) || (ativo ? 'ATIVO' : 'INATIVO')
  const empresasRaw = field(row, 'empresas')
  const empresas = Array.isArray(empresasRaw)
    ? empresasRaw.map(normalizeEmpresa)
    : field(row, 'empresaId') && field(row, 'empresaNome')
      ? [normalizeEmpresa({
          id: field(row, 'empresaId'),
          razaoSocial: field(row, 'empresaNome'),
          cnpj: field(row, 'empresaCnpj'),
          empresaAtiva: field(row, 'empresaAtiva'),
          vinculoAtivo: true,
        })]
      : []

  return {
    id: requiredString(field(row, 'id')),
    nome: requiredString(field(row, 'nome')),
    email: requiredString(field(row, 'email')),
    perfil: perfilConhecido || perfilRaw,
    situacao,
    ativo,
    convidadoEm: optionalString(field(row, 'convidadoEm', 'criadoEm')),
    ultimoAcessoEm: optionalString(field(row, 'ultimoAcessoEm')),
    empresas,
  }
}

export async function listarUsuarios(signal?: AbortSignal): Promise<UsuarioAcesso[]> {
  const [usuariosResponse, convitesResponse] = await Promise.all([
    http.get<unknown>('/v1/usuarios', { signal }),
    http.get<unknown>('/v1/usuarios/convites', { signal }),
  ])
  if (!Array.isArray(usuariosResponse.data) || !Array.isArray(convitesResponse.data)) throw new Error('A API retornou uma listagem de usuarios em formato inesperado.')
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

export async function alterarPerfil(id: string, perfil: PerfilGerenciavelUsuario): Promise<void> {
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
