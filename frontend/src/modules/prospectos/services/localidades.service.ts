import { http } from '../../../api/http'
import { field, optionalString, record, requiredString } from '../../../api/contracts'
import type { EnderecoCepLocalidade, EstadoLocalidade, MunicipioLocalidade } from '../types/localidade'

function lista(value: unknown): unknown[] {
  if (!Array.isArray(value)) throw new Error('A API retornou localidades em formato inesperado.')
  return value
}

export function normalizeEstados(value: unknown): EstadoLocalidade[] {
  return lista(value).map((item) => {
    const estado = record(item)
    return {
      codigoIbge: requiredString(field(estado, 'codigoIbge')),
      sigla: requiredString(field(estado, 'sigla')),
      nome: requiredString(field(estado, 'nome')),
    }
  })
}

export function normalizeMunicipios(value: unknown): MunicipioLocalidade[] {
  return lista(value).map((item) => {
    const municipio = record(item)
    return {
      codigoIbge: requiredString(field(municipio, 'codigoIbge')),
      nome: requiredString(field(municipio, 'nome')),
      uf: requiredString(field(municipio, 'uf')),
    }
  })
}

export function normalizeEnderecoCep(value: unknown): EnderecoCepLocalidade {
  const endereco = record(value)
  return {
    cep: requiredString(field(endereco, 'cep')),
    logradouro: optionalString(field(endereco, 'logradouro')),
    complemento: optionalString(field(endereco, 'complemento')),
    bairro: optionalString(field(endereco, 'bairro')),
    cidade: requiredString(field(endereco, 'cidade')),
    uf: requiredString(field(endereco, 'uf')),
    municipioCodigoIbge: requiredString(field(endereco, 'municipioCodigoIbge')),
  }
}

export async function listarEstados(signal?: AbortSignal): Promise<EstadoLocalidade[]> {
  const { data } = await http.get<unknown>('/v1/localidades/estados', { signal })
  return normalizeEstados(data)
}

export async function listarMunicipios(uf: string, signal?: AbortSignal): Promise<MunicipioLocalidade[]> {
  const { data } = await http.get<unknown>(`/v1/localidades/estados/${encodeURIComponent(uf)}/municipios`, { signal })
  return normalizeMunicipios(data)
}

export async function consultarCep(cep: string, signal?: AbortSignal): Promise<EnderecoCepLocalidade> {
  const { data } = await http.get<unknown>(`/v1/localidades/cep/${encodeURIComponent(cep)}`, { signal })
  return normalizeEnderecoCep(data)
}
