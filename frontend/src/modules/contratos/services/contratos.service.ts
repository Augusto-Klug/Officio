import { http } from '../../../api/http'
import { field, optionalString, record, requiredString } from '../../../api/contracts'
import type { Contrato, EmpresaContratoFiltro, EnviarAssinaturaPayload, EnvioAssinaturaResultado } from '../types/contrato'

function booleanValue(value: unknown): boolean {
  if (typeof value !== 'boolean') throw new Error('A API retornou um indicador inválido.')
  return value
}

function numberValue(value: unknown): number {
  if (typeof value !== 'number' || !Number.isFinite(value)) throw new Error('A API retornou um valor numérico inválido.')
  return value
}

function normalizeContrato(value: unknown): Contrato {
  const item = record(value)
  return {
    id: requiredString(field(item, 'id')),
    numero: requiredString(field(item, 'numero')),
    prospectoId: requiredString(field(item, 'prospectoId')),
    clienteId: optionalString(field(item, 'clienteId')),
    orcamentoId: optionalString(field(item, 'orcamentoId')),
    razaoSocial: requiredString(field(item, 'razaoSocial')),
    status: requiredString(field(item, 'status')),
    valorTotal: numberValue(field(item, 'valorTotal')),
    diaVencimento: numberValue(field(item, 'diaVencimento')),
    dataInicio: requiredString(field(item, 'dataInicio')),
    dataFim: optionalString(field(item, 'dataFim')),
    clausulasExtras: optionalString(field(item, 'clausulasExtras')),
    pdfGerado: booleanValue(field(item, 'pdfGerado')),
    pdfAssinadoDisponivel: booleanValue(field(item, 'pdfAssinadoDisponivel')),
    assinadoEm: optionalString(field(item, 'assinadoEm')),
  }
}

export function normalizeContratos(value: unknown): Contrato[] {
  if (!Array.isArray(value)) throw new Error('A API retornou os contratos em formato inesperado.')
  return value.map(normalizeContrato)
}

export function normalizeEmpresasContrato(value: unknown): EmpresaContratoFiltro[] {
  if (!Array.isArray(value)) throw new Error('A API retornou as empresas em formato inesperado.')
  return value.map((value) => {
    const item = record(value)
    return {
      id: requiredString(field(item, 'id')),
      razaoSocial: requiredString(field(item, 'razaoSocial')),
      cnpj: optionalString(field(item, 'cnpj')),
    }
  })
}

export async function listarContratos(empresaId?: string, signal?: AbortSignal, status?: string): Promise<Contrato[]> {
  const params = {
    ...(empresaId ? { empresaId } : {}),
    ...(status ? { status } : {}),
  }
  const { data } = await http.get<unknown>('/v1/contratos/Listar', {
    params: Object.keys(params).length ? params : undefined,
    signal,
  })
  return normalizeContratos(data)
}

export async function listarEmpresasContrato(signal?: AbortSignal): Promise<EmpresaContratoFiltro[]> {
  const { data } = await http.get<unknown>('/v1/contratos/ListarEmpresas', { signal })
  return normalizeEmpresasContrato(data)
}

export async function gerarPdfContrato(id: string): Promise<{ pdfUrl: string }> {
  const { data } = await http.post<unknown>(`/v1/contratos/GerarPdf/${id}`)
  const body = record(data)
  return { pdfUrl: requiredString(field(body, 'pdfUrl')) }
}

export async function enviarContratoParaAssinatura(id: string, payload: EnviarAssinaturaPayload): Promise<EnvioAssinaturaResultado> {
  const { data } = await http.post<unknown>(`/v1/contratos/EnviarParaAssinatura/${id}`, payload)
  const body = record(data)
  const signatarios = field(body, 'signatarios')
  return {
    metodo: requiredString(field(body, 'metodo')),
    envelopeId: optionalString(field(body, 'envelopeId', 'assinaturaExternaId')),
    signatarios: Array.isArray(signatarios) ? signatarios.map((value) => {
      const signer = record(value)
      return { email: requiredString(field(signer, 'email')), link: requiredString(field(signer, 'link')) }
    }) : null,
    mensagem: optionalString(field(body, 'mensagem', 'message')),
    pdfUrl: optionalString(field(body, 'pdfUrl')),
  }
}

export async function obterPdfAssinado(id: string): Promise<string> {
  const { data } = await http.get<unknown>(`/v1/contratos/ObterPdfAssinado/${id}`)
  return requiredString(field(record(data), 'pdfUrl'))
}
