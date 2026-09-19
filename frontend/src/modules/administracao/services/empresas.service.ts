import { field, optionalString, record, requiredString } from '../../../api/contracts'
import { http } from '../../../api/http'
import type { CriarEmpresaInput, EmpresaPlataforma } from '../types/empresa'

function numberValue(value: unknown): number {
  return typeof value === 'number' && Number.isFinite(value) ? value : 0
}

export function normalizeEmpresa(value: unknown): EmpresaPlataforma {
  const row = record(value)
  return {
    id: requiredString(field(row, 'id')),
    razaoSocial: requiredString(field(row, 'razaoSocial')),
    nomeFantasia: optionalString(field(row, 'nomeFantasia')),
    cnpj: requiredString(field(row, 'cnpj')),
    ativa: field(row, 'ativa', 'ativo') !== false,
    criadaEm: optionalString(field(row, 'criadaEm')),
    quantidadeUsuarios: numberValue(field(row, 'quantidadeUsuarios')),
    administradorEmail: optionalString(field(row, 'administradorEmail', 'emailAdministrador')),
  }
}

export async function listarEmpresas(signal?: AbortSignal): Promise<EmpresaPlataforma[]> {
  const { data } = await http.get<unknown>('/v1/admin/empresas', { signal })
  const body = Array.isArray(data) ? data : field(record(data), 'itens', 'empresas')
  if (!Array.isArray(body)) throw new Error('A API retornou uma listagem de empresas em formato inesperado.')
  return body.map(normalizeEmpresa)
}

export async function criarEmpresa(input: CriarEmpresaInput): Promise<void> {
  await http.post('/v1/admin/empresas', {
    razaoSocial: input.razaoSocial.trim(),
    nomeFantasia: input.nomeFantasia?.trim() || null,
    cnpj: input.cnpj.replace(/\D/g, ''),
    emailEmpresa: null,
    nomeAdministrador: input.administrador.nome.trim(),
    emailAdministrador: input.administrador.email.trim(),
  })
}

export async function alterarStatusEmpresa(id: string, ativa: boolean): Promise<void> {
  await http.patch(`/v1/admin/empresas/${id}/status`, { ativa })
}

export async function reenviarConviteAdministrador(id: string): Promise<void> {
  await http.post(`/v1/admin/empresas/${id}/administrador/reenviar-convite`)
}
