import { http } from '../../../api/http'
import { field, optionalString, record, requiredString } from '../../../api/contracts'
import type { Cliente, ClienteContrato, ClienteDetalhe, ClienteDocumento, ClientesFiltro, ClienteSocio } from '../types/cliente'
import type { CriarSocioPayload } from '../../prospectos/types/prospecto'
import { RegimeTributario, regimeTributarioOptions } from '../../prospectos/types/prospecto'
import type { EnderecoCadastro } from '../../../components/shared/endereco'

function booleanValue(value: unknown, fallback = false): boolean {
  return typeof value === 'boolean' ? value : fallback
}

function numberValue(value: unknown): number {
  return typeof value === 'number' && Number.isFinite(value) ? value : 0
}

export function normalizarRegimeTributario(value: string | null): RegimeTributario | null {
  const normalized = value?.trim()
  if (!normalized) return null

  const opcao = regimeTributarioOptions.find(item =>
    item.label.toLocaleLowerCase() === normalized.toLocaleLowerCase() ||
    String(item.value) === normalized,
  )
  if (opcao) return opcao.value

  const enumValue = Object.entries(RegimeTributario).find(([name, enumValue]) =>
    typeof enumValue === 'number' && name.toLocaleLowerCase() === normalized.toLocaleLowerCase(),
  )?.[1]
  return typeof enumValue === 'number' ? enumValue : null
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
  return {
    id: requiredString(field(row, 'id')), nome: requiredString(field(row, 'nome')),
    cpf: optionalString(field(row, 'cpf')), participacao: numberValue(field(row, 'participacao')),
    ativo: booleanValue(field(row, 'ativo'), true), vigenteDesde: requiredString(field(row, 'vigenteDesde')),
    vigenteAte: optionalString(field(row, 'vigenteAte')), profissao: optionalString(field(row, 'profissao')),
    tipoRelacaoId: optionalString(field(row, 'tipoRelacaoId')), tipoRelacao: optionalString(field(row, 'tipoRelacao')),
    cep: optionalString(field(row, 'cep')), logradouro: optionalString(field(row, 'logradouro')),
    numero: optionalString(field(row, 'numero')), complemento: optionalString(field(row, 'complemento')),
    bairro: optionalString(field(row, 'bairro')), cidade: optionalString(field(row, 'cidade')),
    uf: optionalString(field(row, 'uf')), municipioCodigoIbge: optionalString(field(row, 'municipioCodigoIbge')),
  }
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
    inscricaoEstadual: optionalString(field(row, 'inscricaoEstadual')),
    inscricaoMunicipal: optionalString(field(row, 'inscricaoMunicipal')),
    tipoEmpresa: optionalString(field(row, 'tipoEmpresa')),
    complemento: optionalString(field(row, 'complemento')),
    municipioCodigoIbge: optionalString(field(row, 'municipioCodigoIbge')),
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

export interface AtualizarClientePayload {
  razaoSocial: string
  nomeFantasia: string | null
  cnpj: string | null
  cpf: string | null
  regimeTributario: number | null
  email: string | null
  telefone: string | null
  whatsapp: string | null
  inscricaoEstadual: string | null
  inscricaoMunicipal: string | null
  tipoEmpresa: string | null
  observacoes: string | null
  endereco: EnderecoCadastro
  socios: CriarSocioPayload[]
  confirmarCancelamentoContratos?: boolean
  contratosConfirmados?: string[]
}

export async function atualizarCliente(id: string, payload: AtualizarClientePayload): Promise<void> {
  await http.put(`/v1/clientes/Atualizar/${id}`, {
    razaoSocial: payload.razaoSocial,
    nomeFantasia: payload.nomeFantasia,
    cnpj: payload.cnpj,
    cpf: payload.cpf,
    regimeTributario: payload.regimeTributario,
    email: payload.email,
    telefone: payload.telefone,
    whatsapp: payload.whatsapp,
    cep: payload.endereco.cep,
    logradouro: payload.endereco.logradouro,
    numero: payload.endereco.numero,
    complemento: payload.endereco.complemento,
    bairro: payload.endereco.bairro,
    cidade: payload.endereco.cidade,
    uf: payload.endereco.uf,
    municipioCodigoIbge: payload.endereco.municipioCodigoIbge,
    inscricaoEstadual: payload.inscricaoEstadual,
    inscricaoMunicipal: payload.inscricaoMunicipal,
    tipoEmpresa: payload.tipoEmpresa,
    observacoes: payload.observacoes,
    socios: payload.socios,
    confirmarCancelamentoContratos: payload.confirmarCancelamentoContratos ?? false,
    contratosConfirmados: payload.contratosConfirmados ?? null,
  })
}
