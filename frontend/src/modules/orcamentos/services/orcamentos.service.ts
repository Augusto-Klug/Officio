import { http } from '../../../api/http'
import { field, optionalString, record, requiredString } from '../../../api/contracts'
import type { AtualizarOrcamentoPayload, ConverterContratoPayload, CriarOrcamentoPayload, ItemOrcamento, Orcamento } from '../types/orcamento'

function numberValue(value: unknown): number {
  if (typeof value !== 'number' || !Number.isFinite(value)) throw new Error('A API retornou um valor numérico inválido.')
  return value
}

function normalizeItem(value: unknown): ItemOrcamento {
  const item = record(value)
  return {
    id: requiredString(field(item, 'id')),
    itemCatalogoOrcamentoId: optionalString(field(item, 'itemCatalogoOrcamentoId')),
    descricao: requiredString(field(item, 'descricao')),
    secao: requiredString(field(item, 'secao')),
    tipo: requiredString(field(item, 'tipo')),
    naturezaCobranca: requiredString(field(item, 'naturezaCobranca')),
    quantidade: numberValue(field(item, 'quantidade')),
    valorUnitario: numberValue(field(item, 'valorUnitario')),
    valorTotal: numberValue(field(item, 'valorTotal')),
    ordem: numberValue(field(item, 'ordem')),
  }
}

export function normalizeOrcamento(value: unknown): Orcamento {
  const item = record(value)
  const itens = field(item, 'itens')
  if (!Array.isArray(itens)) throw new Error('A API retornou itens de orçamento inválidos.')
  return {
    id: requiredString(field(item, 'id')),
    prospectoId: requiredString(field(item, 'prospectoId')),
    titulo: optionalString(field(item, 'titulo')),
    status: requiredString(field(item, 'status')),
    valorTotal: numberValue(field(item, 'valorTotal')),
    diaVencimento: numberValue(field(item, 'diaVencimento')),
    observacoes: optionalString(field(item, 'observacoes')),
    motivoRecusa: optionalString(field(item, 'motivoRecusa')),
    itens: itens.map(normalizeItem),
  }
}

export function normalizeOrcamentos(value: unknown): Orcamento[] {
  if (!Array.isArray(value)) throw new Error('A API retornou os orçamentos em formato inesperado.')
  return value.map(normalizeOrcamento)
}

export async function listarOrcamentos(prospectoId: string, signal?: AbortSignal): Promise<Orcamento[]> {
  const { data } = await http.get<unknown>(`/v1/orcamentos/PorProspecto/${prospectoId}`, { signal })
  return normalizeOrcamentos(data)
}

export async function obterOrcamento(id: string): Promise<Orcamento> {
  const { data } = await http.get<unknown>(`/v1/orcamentos/ObterPorId/${id}`)
  return normalizeOrcamento(data)
}

export async function criarOrcamento(payload: CriarOrcamentoPayload): Promise<string> {
  const { data } = await http.post<unknown>('/v1/orcamentos/Criar', payload)
  return requiredString(data)
}

export async function atualizarOrcamento(id: string, payload: AtualizarOrcamentoPayload): Promise<string> {
  const { data } = await http.put<unknown>(`/v1/orcamentos/Atualizar/${id}`, payload)
  return requiredString(data)
}

export async function aceitarOrcamento(id: string): Promise<void> {
  await http.post(`/v1/orcamentos/Aceitar/${id}`)
}

export async function converterOrcamento(id: string, payload: ConverterContratoPayload): Promise<string> {
  const { data } = await http.post<unknown>(`/v1/orcamentos/ConverterParaContrato/${id}`, payload)
  return requiredString(data)
}
