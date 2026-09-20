import { http } from '../../../api/http'
import { field, optionalString, record, requiredString } from '../../../api/contracts'
import type { AdmissaoDependente, AdmissaoDetalhe, AdmissaoDocumento, AdmissaoResumo, AdmissoesFiltro, CriarAdmissaoPayload, DadosAdmissaoPayload, ResultadoValidacaoDocumento, UrlDocumentoAdmissao } from '../types/admissao'

function optionalNumber(value: unknown): number | null {
  if (value == null) return null
  if (typeof value !== 'number' || !Number.isFinite(value)) throw new Error('A API retornou um valor numérico inválido.')
  return value
}

function optionalBoolean(value: unknown): boolean | null {
  if (value == null) return null
  if (typeof value !== 'boolean') throw new Error('A API retornou um valor lógico inválido.')
  return value
}

function normalizeDependente(value: unknown): AdmissaoDependente {
  const item = record(value)
  return {
    id: requiredString(field(item, 'id')),
    nome: requiredString(field(item, 'nome')),
    cpf: optionalString(field(item, 'cpf')),
    dataNascimento: optionalString(field(item, 'dataNascimento')),
    parentesco: optionalString(field(item, 'parentesco')),
    dependenteIr: optionalBoolean(field(item, 'dependenteIr')),
    salarioFamilia: optionalBoolean(field(item, 'salarioFamilia')),
  }
}

function normalizeResumo(value: unknown): AdmissaoResumo {
  const item = record(value)
  return {
    id: requiredString(field(item, 'id')),
    empresaId: requiredString(field(item, 'empresaId')),
    empresaNome: requiredString(field(item, 'empresaNome')),
    nomeFuncionario: requiredString(field(item, 'nomeFuncionario')),
    cpf: optionalString(field(item, 'cpf')),
    status: requiredString(field(item, 'status')),
    statusDescricao: requiredString(field(item, 'statusDescricao')),
    dataAdmissaoPrevista: optionalString(field(item, 'dataAdmissaoPrevista')),
    funcao: optionalString(field(item, 'funcao')),
    salario: optionalNumber(field(item, 'salario')),
    criadoEm: requiredString(field(item, 'criadoEm')),
  }
}

export function normalizeAdmissoes(value: unknown): AdmissaoResumo[] {
  if (!Array.isArray(value)) throw new Error('A API retornou as admissões em formato inesperado.')
  return value.map(normalizeResumo)
}

export function normalizeAdmissao(value: unknown): AdmissaoDetalhe {
  const item = record(value)
  const dependentes = field(item, 'dependentes')
  if (!Array.isArray(dependentes)) throw new Error('A API retornou dependentes em formato inesperado.')
  return {
    ...normalizeResumo(item),
    rg: optionalString(field(item, 'rg')),
    dataNascimento: optionalString(field(item, 'dataNascimento')),
    sexo: optionalString(field(item, 'sexo')),
    estadoCivil: optionalString(field(item, 'estadoCivil')),
    escolaridade: optionalString(field(item, 'escolaridade')),
    telefone: optionalString(field(item, 'telefone')),
    email: optionalString(field(item, 'email')),
    pis: optionalString(field(item, 'pis')),
    banco: optionalString(field(item, 'banco')),
    agencia: optionalString(field(item, 'agencia')),
    conta: optionalString(field(item, 'conta')),
    tipoConta: optionalString(field(item, 'tipoConta')),
    comissao: optionalString(field(item, 'comissao')),
    contratoExperienciaDias: optionalNumber(field(item, 'contratoExperienciaDias')),
    utilizaValeTransporte: optionalBoolean(field(item, 'utilizaValeTransporte')),
    horarioTrabalho: optionalString(field(item, 'horarioTrabalho')),
    observacoes: optionalString(field(item, 'observacoes')),
    atualizadoEm: requiredString(field(item, 'atualizadoEm')),
    dependentes: dependentes.map(normalizeDependente),
  }
}

function normalizeDocumento(value: unknown): AdmissaoDocumento {
  const item = record(value)
  return {
    id: requiredString(field(item, 'id')),
    tipoDocumentoId: requiredString(field(item, 'tipoDocumentoId')),
    tipoCodigo: requiredString(field(item, 'tipoCodigo')),
    tipoDescricao: requiredString(field(item, 'tipoDescricao')),
    rotulo: requiredString(field(item, 'rotulo')),
    obrigatorio: optionalBoolean(field(item, 'obrigatorio')) ?? false,
    ordem: optionalNumber(field(item, 'ordem')) ?? 0,
    documentoId: optionalString(field(item, 'documentoId')),
    nomeArquivo: optionalString(field(item, 'nomeArquivo')),
    mimeType: optionalString(field(item, 'mimeType')),
    tamanhoBytes: optionalNumber(field(item, 'tamanhoBytes')),
    qualidadeEstimada: optionalNumber(field(item, 'qualidadeEstimada')),
    enviadoEm: optionalString(field(item, 'enviadoEm')),
    statusValidacao: requiredString(field(item, 'statusValidacao')),
    confiancaOcr: optionalNumber(field(item, 'confiancaOcr')),
    mensagemValidacao: optionalString(field(item, 'mensagemValidacao')),
  }
}

export function normalizeDocumentosAdmissao(value: unknown): AdmissaoDocumento[] {
  if (!Array.isArray(value)) throw new Error('A API retornou os documentos em formato inesperado.')
  return value.map(normalizeDocumento)
}

export async function listarAdmissoes(filtro: AdmissoesFiltro = {}, signal?: AbortSignal): Promise<AdmissaoResumo[]> {
  const { data } = await http.get<unknown>('/v1/admissoes/Listar', { params: filtro, signal })
  return normalizeAdmissoes(data)
}

export async function obterAdmissao(id: string): Promise<AdmissaoDetalhe> {
  const { data } = await http.get<unknown>(`/v1/admissoes/ObterPorId/${id}`)
  return normalizeAdmissao(data)
}

export async function criarAdmissao(payload: CriarAdmissaoPayload): Promise<string> {
  const { data } = await http.post<unknown>('/v1/admissoes/CriarManual', payload)
  return requiredString(data)
}

export async function atualizarAdmissao(id: string, dados: DadosAdmissaoPayload): Promise<string> {
  const { data } = await http.put<unknown>(`/v1/admissoes/Atualizar/${id}`, { dados })
  return requiredString(data)
}

export async function concluirAdmissao(id: string): Promise<void> {
  await http.post(`/v1/admissoes/Concluir/${id}`)
}

export async function cancelarAdmissao(id: string): Promise<void> {
  await http.post(`/v1/admissoes/Cancelar/${id}`)
}

export async function listarDocumentosAdmissao(id: string): Promise<AdmissaoDocumento[]> {
  const { data } = await http.get<unknown>(`/v1/admissoes/ListarDocumentos/${id}`)
  return normalizeDocumentosAdmissao(data)
}

export async function enviarDocumentoAdmissao(id: string, itemId: string, arquivo: File): Promise<void> {
  const form = new FormData()
  form.append('arquivo', arquivo)
  await http.post(`/v1/admissoes/EnviarDocumento/${id}/itens/${itemId}`, form)
}

export async function obterUrlDocumentoAdmissao(id: string, itemId: string): Promise<UrlDocumentoAdmissao> {
  const { data } = await http.get<unknown>(`/v1/admissoes/ObterUrlDocumento/${id}/itens/${itemId}`)
  const item = record(data)
  return { url: requiredString(field(item, 'url')), expiraEm: requiredString(field(item, 'expiraEm')) }
}

export async function validarDocumentoAdmissao(id: string, itemId: string): Promise<ResultadoValidacaoDocumento> {
  const { data } = await http.post<unknown>(`/v1/admissoes/ValidarDocumento/${id}/itens/${itemId}`)
  const item = record(data)
  return {
    valido: optionalBoolean(field(item, 'valido')) ?? false,
    confiancaOcr: optionalNumber(field(item, 'confiancaOcr')) ?? 0,
    fallbackManual: optionalBoolean(field(item, 'fallbackManual')) ?? false,
    mensagem: optionalString(field(item, 'mensagem')),
  }
}

export async function aprovarDocumentoAdmissao(id: string, itemId: string, observacao?: string): Promise<void> {
  await http.post(`/v1/admissoes/AprovarDocumento/${id}/itens/${itemId}`, { observacao })
}

export async function removerDocumentoAdmissao(id: string, itemId: string): Promise<void> {
  await http.delete(`/v1/admissoes/RemoverDocumento/${id}/itens/${itemId}`)
}
