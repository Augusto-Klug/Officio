import { http } from '../../../api/http'
import { field, optionalString, record, requiredString } from '../../../api/contracts'
import type { Cliente, ClienteContrato, ClienteDetalhe, ClienteDocumento, ClientesFiltro, ClienteSocio } from '../types/cliente'

function booleanValue(value: unknown, fallback = false): boolean {
  return typeof value === 'boolean' ? value : fallback
}

function numberValue(value: unknown): number {
  return typeof value === 'number' && Number.isFinite(value) ? value : 0
}

function normalizeClienteBase(value: unknown) {
  const row = record(value)
  return {
    id: requiredString(field(row, 'id')),
    empresaId: requiredString(field(row, 'empresaId')),
    razaoSocial: requiredString(field(row, 'razaoSocial')),
    nomeFantasia: optionalString(field(row, 'nomeFantasia')),
    cnpj: optionalString(field(row, 'cnpj')),
    cpf: optionalString(field(row, 'cpf')),
    regimeTributario: optionalString(field(row, 'regimeTributario')),
    sciCodigo: optionalString(field(row, 'sciCodigo')),
    ativo: booleanValue(field(row, 'ativo'), true),
    email: optionalString(field(row, 'email')),
    telefone: optionalString(field(row, 'telefone')),
    cidade: optionalString(field(row, 'cidade')),
    uf: optionalString(field(row, 'uf')),
  }
}

export function normalizeClientes(value: unknown): Cliente[] {
  if (!Array.isArray(value)) throw new Error('A API retornou uma listagem de clientes em formato inesperado.')
  return value.map((item) => {
    const row = record(item)
    return {
      ...normalizeClienteBase(row),
      quantidadeSocios: numberValue(field(row, 'quantidadeSocios')),
      quantidadeContratos: numberValue(field(row, 'quantidadeContratos')),
      quantidadeDocumentos: numberValue(field(row, 'quantidadeDocumentos')),
    }
  })
}

function normalizeSocio(value: unknown): ClienteSocio {
  const row = record(value)
  return { id: requiredString(field(row, 'id')), nome: requiredString(field(row, 'nome')), cpf: optionalString(field(row, 'cpf')), participacao: numberValue(field(row, 'participacao')) }
}

function normalizeContrato(value: unknown): ClienteContrato {
  const row = record(value)
  return {
    id: requiredString(field(row, 'id')), numero: requiredString(field(row, 'numero')),
    status: requiredString(field(row, 'status')), valorTotal: numberValue(field(row, 'valorTotal')),
    dataInicio: requiredString(field(row, 'dataInicio')), dataFim: optionalString(field(row, 'dataFim')),
  }
}

function normalizeDocumento(value: unknown): ClienteDocumento {
  const row = record(value)
  return {
    id: requiredString(field(row, 'id')), nomeArquivo: requiredString(field(row, 'nomeArquivo')),
    tipo: requiredString(field(row, 'tipo')), legivel: booleanValue(field(row, 'legivel')),
    qualidadeEstimada: numberValue(field(row, 'qualidadeEstimada')),
  }
}

export function normalizeClienteDetalhe(value: unknown): ClienteDetalhe {
  const row = record(value)
  const array = (name: string) => {
    const result = field(row, name)
    return Array.isArray(result) ? result : []
  }
  return {
    ...normalizeClienteBase(row),
    whatsapp: optionalString(field(row, 'whatsapp')), cep: optionalString(field(row, 'cep')),
    logradouro: optionalString(field(row, 'logradouro')), numero: optionalString(field(row, 'numero')),
    bairro: optionalString(field(row, 'bairro')), observacoes: optionalString(field(row, 'observacoes')),
    prospectOrigemId: optionalString(field(row, 'prospectOrigemId')),
    inicioRelacionamentoEm: optionalString(field(row, 'inicioRelacionamentoEm')),
    socios: array('socios').map(normalizeSocio), contratos: array('contratos').map(normalizeContrato),
    documentos: array('documentos').map(normalizeDocumento),
  }
}

export async function listarClientes(filtro: ClientesFiltro, signal?: AbortSignal): Promise<Cliente[]> {
  const { data } = await http.get<unknown>('/v1/clientes/Listar', { params: filtro, signal })
  return normalizeClientes(data)
}

export async function obterCliente(id: string): Promise<ClienteDetalhe> {
  const { data } = await http.get<unknown>(`/v1/clientes/ObterPorId/${id}`)
  return normalizeClienteDetalhe(data)
}

export async function vincularCodigoSci(id: string, sciCodigo: string): Promise<void> {
  await http.patch(`/v1/clientes/VincularCodigoSci/${id}`, { sciCodigo })
}
