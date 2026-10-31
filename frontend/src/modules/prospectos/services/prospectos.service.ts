import { http } from '../../../api/http'
export async function atualizarSociosProspecto(id: string, payload: CriarProspectoPayload): Promise<void> {
  await http.put(`/v1/prospectos/AtualizarSocios/${id}`, payload)
}
import { field, optionalString, record, requiredString } from '../../../api/contracts'
import type { CriarProspectoPayload, ProspectoCriado, ProspectoDetalhe, ProspectosFiltro, ProspectosPagina } from '../types/prospecto'

function integer(value: unknown, minimum: number): number {
  if (typeof value !== 'number' || !Number.isInteger(value) || value < minimum) throw new Error('A API retornou uma paginação inválida.')
  return value
}

export function normalizeProspectos(value: unknown): ProspectosPagina {
  const body = record(value)
  const rows = field(body, 'data', 'dados')
  if (!Array.isArray(rows)) throw new Error('A API retornou uma listagem em formato inesperado.')
  return {
    dados: rows.map((value) => {
      const row = record(value)
      return {
        id: requiredString(field(row, 'id')),
        razaoSocial: requiredString(field(row, 'razaoSocial')),
        cnpj: optionalString(field(row, 'cnpj')),
        status: optionalString(field(row, 'status')),
        responsavel: optionalString(field(row, 'responsavel', 'responsavelNome')),
        criadoEm: optionalString(field(row, 'criadoEm')),
        podeEditar: field(row, 'podeEditar') === true,
        podeExcluir: field(row, 'podeExcluir') === true,
        motivoBloqueio: optionalString(field(row, 'motivoBloqueio')),
      }
    }),
    totalRegistros: integer(field(body, 'totalCount', 'totalRegistros'), 0),
    pagina: integer(field(body, 'page', 'pagina'), 1),
    tamanhoPagina: integer(field(body, 'pageSize', 'tamanhoPagina'), 1),
    totalPaginas: integer(field(body, 'totalPages', 'totalPaginas'), 0),
  }
}

export async function listarProspectos(filtro: ProspectosFiltro, signal?: AbortSignal): Promise<ProspectosPagina> {
  const { data } = await http.get<unknown>('/v1/prospectos/Listar', { params: filtro, signal })
  return normalizeProspectos(data)
}

export function normalizeProspectoCriado(value: unknown): ProspectoCriado {
  const body = record(value)
  return {
    id: requiredString(field(body, 'id')),
    razaoSocial: requiredString(field(body, 'razaoSocial')),
  }
}

export async function criarProspecto(payload: CriarProspectoPayload): Promise<ProspectoCriado> {
  const { data } = await http.post<unknown>('/v1/prospectos/Criar', payload)
  return normalizeProspectoCriado(data)
}

export async function obterProspecto(id: string, signal?: AbortSignal, incluirSociosCliente = false): Promise<ProspectoDetalhe> {
  const { data } = await http.get<ProspectoDetalhe>(`/v1/prospectos/ObterPorId/${id}`, { signal,
    ...(incluirSociosCliente ? { params: { incluirSociosCliente: true } } : {}) })
  return data
}

export async function atualizarProspecto(id: string, payload: CriarProspectoPayload): Promise<void> {
  await http.put(`/v1/prospectos/Atualizar/${id}`, payload)
}

export async function excluirProspecto(id: string): Promise<void> {
  await http.delete(`/v1/prospectos/Excluir/${id}`)
}

export async function listarTiposRelacaoSocietaria(signal?: AbortSignal): Promise<{ id: string; codigo: string; descricao: string }[]> {
  return (await http.get('/v1/prospectos/TiposRelacaoSocietaria', { signal })).data
}
