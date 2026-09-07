import { http } from '../../../api/http'
import { field, optionalString, record, requiredString } from '../../../api/contracts'
import type { ProspectosFiltro, ProspectosPagina } from '../types/prospecto'

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
