import { field, optionalString, record, requiredString } from '../../../api/contracts'
import { http } from '../../../api/http'
import type { Notificacao, QuantidadeNotificacoesLidas } from '../types/notificacao'

function booleano(value: unknown): boolean {
  if (typeof value !== 'boolean') throw new Error('A API retornou uma notificacao invalida.')
  return value
}

function dataObrigatoria(value: unknown): string {
  const data = requiredString(value)
  if (Number.isNaN(Date.parse(data))) throw new Error('A API retornou uma data de notificacao invalida.')
  return data
}

function dataOpcional(value: unknown): string | null {
  const data = optionalString(value)
  if (data && Number.isNaN(Date.parse(data))) throw new Error('A API retornou uma data de notificacao invalida.')
  return data
}

function inteiroNaoNegativo(value: unknown): number {
  if (typeof value !== 'number' || !Number.isInteger(value) || value < 0) {
    throw new Error('A API retornou uma quantidade de notificacoes invalida.')
  }
  return value
}

export function normalizarNotificacoes(value: unknown): Notificacao[] {
  if (!Array.isArray(value)) throw new Error('A API retornou uma listagem de notificacoes invalida.')

  return value.map((item) => {
    const notificacao = record(item)
    return {
      id: requiredString(field(notificacao, 'id')),
      tipo: requiredString(field(notificacao, 'tipo')),
      titulo: requiredString(field(notificacao, 'titulo')),
      mensagem: optionalString(field(notificacao, 'mensagem')),
      referenciaId: optionalString(field(notificacao, 'referenciaId')),
      lida: booleano(field(notificacao, 'lida')),
      lidaEm: dataOpcional(field(notificacao, 'lidaEm')),
      criadoEm: dataObrigatoria(field(notificacao, 'criadoEm')),
    }
  })
}

export async function listarNotificacoes(lida?: boolean, signal?: AbortSignal): Promise<Notificacao[]> {
  const { data } = await http.get<unknown>('/v1/notificacoes/Listar', {
    params: lida === undefined ? undefined : { lida },
    signal,
  })
  return normalizarNotificacoes(data)
}

export async function marcarNotificacaoComoLida(id: string): Promise<void> {
  await http.patch(`/v1/notificacoes/MarcarComoLida/${encodeURIComponent(id)}`)
}

export async function marcarTodasNotificacoesComoLidas(): Promise<QuantidadeNotificacoesLidas> {
  const { data } = await http.patch<unknown>('/v1/notificacoes/MarcarTodasComoLidas')
  const resposta = record(data)
  return { quantidade: inteiroNaoNegativo(field(resposta, 'quantidade')) }
}
