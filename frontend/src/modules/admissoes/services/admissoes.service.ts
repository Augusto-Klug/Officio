import { http } from '../../../api/http'
import { field, optionalString, record, requiredString } from '../../../api/contracts'
import type { AdmissaoDependente, AdmissaoDetalhe, AdmissaoDocumento, AdmissaoResumo, AdmissoesFiltro, AtualizarFichaEmpresaPayload, AtualizarFichaFuncionarioPayload, CriarAdmissaoPayload, DadosAdmissaoPayload, DivergenciaDocumento, EmpresaAdmissoesResumo, FichaEmpresaPublica, FichaFuncionarioPublica, LinkFichaAdmissao, ResultadoValidacaoDocumento, UrlDocumentoAdmissao } from '../types/admissao'

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

function normalizeCamposExtraidos(value: unknown): Record<string, string> {
  if (value == null) return {}
  const item = record(value)
  return Object.fromEntries(Object.entries(item).map(([chave, conteudo]) => [chave, requiredString(conteudo)]))
}

function normalizeDivergencias(value: unknown): DivergenciaDocumento[] {
  if (value == null) return []
  if (!Array.isArray(value)) throw new Error('A API retornou divergências em formato inesperado.')
  return value.map((conteudo) => {
    const item = record(conteudo)
    return {
      campo: requiredString(field(item, 'campo')),
      valorDocumento: requiredString(field(item, 'valorDocumento')),
      valorCadastro: requiredString(field(item, 'valorCadastro')),
      similaridade: optionalNumber(field(item, 'similaridade')) ?? 0,
    }
  })
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
    preenchimentoFuncionarioConcluido: optionalBoolean(field(item, 'preenchimentoFuncionarioConcluido')) ?? false,
    preenchimentoEmpresaConcluido: optionalBoolean(field(item, 'preenchimentoEmpresaConcluido')) ?? false,
  }
}

export function normalizeAdmissoes(value: unknown): AdmissaoResumo[] {
  if (!Array.isArray(value)) throw new Error('A API retornou as admissões em formato inesperado.')
  return value.map(normalizeResumo)
}

function normalizeEmpresaAdmissoes(value: unknown): EmpresaAdmissoesResumo {
  const item = record(value)
  return {
    empresaId: requiredString(field(item, 'empresaId')),
    empresaNome: requiredString(field(item, 'empresaNome')),
    cnpj: optionalString(field(item, 'cnpj')),
    quantidadeAdmissoes: optionalNumber(field(item, 'quantidadeAdmissoes')) ?? 0,
  }
}

export function normalizeEmpresasComAdmissoes(value: unknown): EmpresaAdmissoesResumo[] {
  if (!Array.isArray(value)) throw new Error('A API retornou as empresas com admissões em formato inesperado.')
  return value.map(normalizeEmpresaAdmissoes)
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
    camposExtraidos: normalizeCamposExtraidos(field(item, 'camposExtraidos')),
    divergencias: normalizeDivergencias(field(item, 'divergencias')),
    tipoDocumentoDetectado: optionalString(field(item, 'tipoDocumentoDetectado')),
    tipoDocumentoIncompativel: optionalBoolean(field(item, 'tipoDocumentoIncompativel')) ?? false,
    observacaoRevisao: optionalString(field(item, 'observacaoRevisao')),
    revisadoPorNome: optionalString(field(item, 'revisadoPorNome')),
    revisadoEm: optionalString(field(item, 'revisadoEm')),
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

export async function listarEmpresasComAdmissoes(busca?: string, signal?: AbortSignal): Promise<EmpresaAdmissoesResumo[]> {
  const { data } = await http.get<unknown>('/v1/admissoes/ListarEmpresasComAdmissoes', {
    params: busca ? { busca } : {},
    signal,
  })
  return normalizeEmpresasComAdmissoes(data)
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

export async function concluirRevisaoDocumentalAdmissao(id: string): Promise<void> {
  await http.post(`/v1/admissoes/ConcluirRevisaoDocumental/${id}`)
}

export async function enviarLinkFichaAdmissaoPorEmail(
  id: string,
  papel: 'funcionario' | 'empresa',
  email: string,
  token: string,
): Promise<void> {
  await http.post(`/v1/admissoes/EnviarLinkPorEmail/${id}`, { papel, email, token })
}

export async function cancelarAdmissao(id: string): Promise<void> {
  await http.post(`/v1/admissoes/Cancelar/${id}`)
}

export async function listarDocumentosAdmissao(id: string): Promise<AdmissaoDocumento[]> {
  const { data } = await http.get<unknown>(`/v1/admissoes/ListarDocumentos/${id}`)
  return normalizeDocumentosAdmissao(data)
}

export async function enviarDocumentoAdmissao(id: string, itemId: string, arquivos: File[]): Promise<void> {
  const form = new FormData()
  arquivos.forEach((arquivo) => form.append('arquivos', arquivo))
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
    divergencias: normalizeDivergencias(field(item, 'divergencias')),
  }
}

export async function aprovarDocumentoAdmissao(id: string, itemId: string, observacao?: string): Promise<void> {
  await http.post(`/v1/admissoes/AprovarDocumento/${id}/itens/${itemId}`, { observacao })
}

export async function removerDocumentoAdmissao(id: string, itemId: string): Promise<void> {
  await http.delete(`/v1/admissoes/RemoverDocumento/${id}/itens/${itemId}`)
}

export async function gerarLinkFichaAdmissao(id: string): Promise<LinkFichaAdmissao> {
  const { data } = await http.post<unknown>(`/v1/admissoes/GerarLinkFicha/${id}`)
  const item = record(data)
  return { token: requiredString(field(item, 'token')), expiraEm: requiredString(field(item, 'expiraEm')) }
}

export async function revogarLinkFichaAdmissao(id: string): Promise<void> {
  await http.post(`/v1/admissoes/RevogarLinkFicha/${id}`)
}

export function normalizeFichaFuncionario(value: unknown): FichaFuncionarioPublica {
  const item = record(value)
  const dependentes = field(item, 'dependentes')
  if (!Array.isArray(dependentes)) throw new Error('A API retornou dependentes em formato inesperado.')
  return {
    id: requiredString(field(item, 'id')),
    empresaNome: requiredString(field(item, 'empresaNome')),
    nomeFuncionario: requiredString(field(item, 'nomeFuncionario')),
    cpf: optionalString(field(item, 'cpf')),
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
    dataAdmissaoPrevista: optionalString(field(item, 'dataAdmissaoPrevista')),
    salario: optionalNumber(field(item, 'salario')),
    comissao: optionalString(field(item, 'comissao')),
    funcao: optionalString(field(item, 'funcao')),
    contratoExperienciaDias: optionalNumber(field(item, 'contratoExperienciaDias')),
    utilizaValeTransporte: optionalBoolean(field(item, 'utilizaValeTransporte')),
    horarioTrabalho: optionalString(field(item, 'horarioTrabalho')),
    preenchimentoConcluido: optionalBoolean(field(item, 'preenchimentoConcluido')) ?? false,
    expiraEm: requiredString(field(item, 'expiraEm')),
    dependentes: dependentes.map(normalizeDependente),
    documentos: normalizeDocumentosAdmissao(field(item, 'documentos')),
  }
}

export function normalizeFichaEmpresa(value: unknown): FichaEmpresaPublica {
  const item = record(value)
  return {
    id: requiredString(field(item, 'id')),
    empresaNome: requiredString(field(item, 'empresaNome')),
    nomeFuncionario: requiredString(field(item, 'nomeFuncionario')),
    cpf: optionalString(field(item, 'cpf')),
    rg: optionalString(field(item, 'rg')),
    dataNascimento: optionalString(field(item, 'dataNascimento')),
    sexo: optionalString(field(item, 'sexo')),
    estadoCivil: optionalString(field(item, 'estadoCivil')),
    escolaridade: optionalString(field(item, 'escolaridade')),
    telefone: optionalString(field(item, 'telefone')),
    email: optionalString(field(item, 'email')),
    banco: optionalString(field(item, 'banco')),
    agencia: optionalString(field(item, 'agencia')),
    conta: optionalString(field(item, 'conta')),
    tipoConta: optionalString(field(item, 'tipoConta')),
    pis: optionalString(field(item, 'pis')),
    dataAdmissaoPrevista: optionalString(field(item, 'dataAdmissaoPrevista')),
    salario: optionalNumber(field(item, 'salario')),
    comissao: optionalString(field(item, 'comissao')),
    funcao: optionalString(field(item, 'funcao')),
    contratoExperienciaDias: optionalNumber(field(item, 'contratoExperienciaDias')),
    utilizaValeTransporte: optionalBoolean(field(item, 'utilizaValeTransporte')),
    horarioTrabalho: optionalString(field(item, 'horarioTrabalho')),
    preenchimentoConcluido: optionalBoolean(field(item, 'preenchimentoConcluido')) ?? false,
    expiraEm: requiredString(field(item, 'expiraEm')),
  }
}

export async function obterFichaAdmissao(token: string): Promise<FichaFuncionarioPublica> {
  const { data } = await http.get<unknown>(`/v1/admissoes-publicas/Obter/${encodeURIComponent(token)}`, { skipAuth: true })
  return normalizeFichaFuncionario(data)
}

export async function atualizarFichaAdmissao(token: string, payload: AtualizarFichaFuncionarioPayload): Promise<void> {
  await http.put(`/v1/admissoes-publicas/Atualizar/${encodeURIComponent(token)}`, payload, { skipAuth: true })
}

export async function concluirFichaAdmissao(token: string): Promise<void> {
  await http.post(`/v1/admissoes-publicas/Concluir/${encodeURIComponent(token)}`, undefined, { skipAuth: true })
}

export async function enviarDocumentoFichaAdmissao(token: string, itemId: string, arquivos: File[]): Promise<void> {
  const form = new FormData()
  arquivos.forEach((arquivo) => form.append('arquivos', arquivo))
  await http.post(`/v1/admissoes-publicas/EnviarDocumento/${encodeURIComponent(token)}/itens/${itemId}`, form, { skipAuth: true })
}

export async function obterUrlDocumentoFichaAdmissao(token: string, itemId: string): Promise<UrlDocumentoAdmissao> {
  const { data } = await http.get<unknown>(`/v1/admissoes-publicas/ObterUrlDocumento/${encodeURIComponent(token)}/itens/${itemId}`, { skipAuth: true })
  const item = record(data)
  return { url: requiredString(field(item, 'url')), expiraEm: requiredString(field(item, 'expiraEm')) }
}

export async function gerarLinkFichaEmpresaAdmissao(id: string): Promise<LinkFichaAdmissao> {
  const { data } = await http.post<unknown>(`/v1/admissoes/GerarLinkFichaEmpresa/${id}`)
  const item = record(data)
  return { token: requiredString(field(item, 'token')), expiraEm: requiredString(field(item, 'expiraEm')) }
}

export async function revogarLinkFichaEmpresaAdmissao(id: string): Promise<void> {
  await http.post(`/v1/admissoes/RevogarLinkFichaEmpresa/${id}`)
}

export async function obterFichaEmpresaAdmissao(token: string): Promise<FichaEmpresaPublica> {
  const { data } = await http.get<unknown>(`/v1/admissoes-publicas/ObterEmpresa/${encodeURIComponent(token)}`, { skipAuth: true })
  return normalizeFichaEmpresa(data)
}

export async function atualizarFichaEmpresaAdmissao(token: string, payload: AtualizarFichaEmpresaPayload): Promise<void> {
  await http.put(`/v1/admissoes-publicas/AtualizarEmpresa/${encodeURIComponent(token)}`, payload, { skipAuth: true })
}

export async function concluirFichaEmpresaAdmissao(token: string): Promise<void> {
  await http.post(`/v1/admissoes-publicas/ConcluirEmpresa/${encodeURIComponent(token)}`, undefined, { skipAuth: true })
}
