import { field, optionalString, record, requiredString } from '../../../api/contracts'
import { http } from '../../../api/http'
import type { CriarParceiroInput, ParceiroPlataforma } from '../types/parceiro'

function numberValue(value: unknown): number {
  return typeof value === 'number' && Number.isFinite(value) ? value : 0
}

export function normalizeParceiro(value: unknown): ParceiroPlataforma {
  const row = record(value)
  return {
    id: requiredString(field(row, 'id')),
    razaoSocial: requiredString(field(row, 'razaoSocial')),
    nomeFantasia: optionalString(field(row, 'nomeFantasia')),
    cnpj: requiredString(field(row, 'cnpj')),
    ativo: field(row, 'ativo', 'ativa') !== false,
    criadaEm: optionalString(field(row, 'criadaEm')),
    quantidadeUsuarios: numberValue(field(row, 'quantidadeUsuarios')),
    administradorEmail: optionalString(field(row, 'administradorEmail', 'emailAdministrador')),
  }
}

export async function listarParceiros(signal?: AbortSignal): Promise<ParceiroPlataforma[]> {
  const { data } = await http.get<unknown>('/v1/admin/parceiros', { signal })
  const body = Array.isArray(data) ? data : field(record(data), 'itens', 'parceiros')
  if (!Array.isArray(body)) throw new Error('A API retornou uma listagem de parceiros em formato inesperado.')
  return body.map(normalizeParceiro)
}

export async function criarParceiro(input: CriarParceiroInput): Promise<void> {
  await http.post('/v1/admin/parceiros', {
    razaoSocial: input.razaoSocial.trim(),
    nomeFantasia: input.nomeFantasia?.trim() || null,
    cnpj: input.cnpj.replace(/\D/g, ''),
    emailParceiro: null,
    nomeAdministrador: input.administrador.nome.trim(),
    emailAdministrador: input.administrador.email.trim(),
  })
}

export async function alterarStatusParceiro(id: string, ativo: boolean): Promise<void> {
  await http.patch(`/v1/admin/parceiros/${id}/status`, { ativo })
}

export async function reenviarConviteAdministrador(id: string): Promise<void> {
  await http.post(`/v1/admin/parceiros/${id}/administrador/reenviar-convite`)
}

export async function convidarAdministradorParceiro(
  id: string,
  administrador: { nome: string; email: string },
): Promise<void> {
  await http.post(`/v1/admin/parceiros/${id}/administrador/convites`, {
    nome: administrador.nome.trim(),
    email: administrador.email.trim(),
  })
}
