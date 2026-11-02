import { http } from '../../../api/http'
import { field, optionalString, record, requiredString } from '../../../api/contracts'
import { normalizeAdmissao, normalizeAdmissoes } from '../../admissoes/services/admissoes.service'
import { normalizeContratos } from '../../contratos/services/contratos.service'
import type { AdmissaoPortalCriada, CriarAdmissaoPortalPayload, DadosEmpregadorPortal, DetalheAdmissaoPortal, EmpresaPortalCliente, VisaoPortalCliente } from '../types/portal-cliente'

function normalizarEmpresa(value: unknown): EmpresaPortalCliente {
  const item = record(value)
  return {
    id: requiredString(field(item, 'id')),
    razaoSocial: requiredString(field(item, 'razaoSocial')),
    cnpj: optionalString(field(item, 'cnpj')),
  }
}

function normalizarVisao(value: unknown): VisaoPortalCliente {
  const item = record(value)
  const empresas = field(item, 'empresas')
  if (!Array.isArray(empresas)) throw new Error('A API retornou as empresas em formato inesperado.')
  return {
    empresas: empresas.map(normalizarEmpresa),
    admissoes: normalizeAdmissoes(field(item, 'admissoes')),
    contratos: normalizeContratos(field(item, 'contratos')),
  }
}

export async function obterVisaoPortalCliente(empresaId?: string, signal?: AbortSignal): Promise<VisaoPortalCliente> {
  const { data } = await http.get<unknown>('/v1/portal-cliente/ObterVisao', {
    params: empresaId ? { empresaId } : undefined,
    signal,
  })
  return normalizarVisao(data)
}

export async function obterAdmissaoPortal(empresaId: string, admissaoId: string): Promise<DetalheAdmissaoPortal> {
  const { data } = await http.get<unknown>(`/v1/portal-cliente/ObterAdmissao/${empresaId}/${admissaoId}`)
  return normalizeAdmissao(data)
}

export async function obterPdfAssinadoPortal(empresaId: string, contratoId: string): Promise<string> {
  const { data } = await http.get<unknown>(`/v1/portal-cliente/ObterPdfAssinado/${empresaId}/${contratoId}`)
  return requiredString(field(record(data), 'pdfUrl'))
}

export async function criarAdmissaoPortal(payload: CriarAdmissaoPortalPayload): Promise<AdmissaoPortalCriada> {
  const { data } = await http.post<unknown>('/v1/portal-cliente/CriarAdmissao', payload)
  const item = record(data)
  const enviado = field(item, 'linkFuncionarioEnviado')
  return {
    id: requiredString(field(item, 'id')),
    emailFuncionario: requiredString(field(item, 'emailFuncionario')),
    linkFuncionarioEnviado: enviado === true,
  }
}

export async function atualizarDadosEmpregadorPortal(admissaoId: string, payload: DadosEmpregadorPortal): Promise<void> {
  await http.put(`/v1/portal-cliente/AtualizarDadosEmpregador/${admissaoId}`, payload)
}

export async function concluirDadosEmpregadorPortal(admissaoId: string): Promise<void> {
  await http.post(`/v1/portal-cliente/ConcluirDadosEmpregador/${admissaoId}`)
}
