import { HubConnectionBuilder, HubConnectionState, LogLevel } from '@microsoft/signalr'
import { accessToken } from '../stores/session'

export interface AtualizacaoTempoReal {
  entidade: string
  id: string | null
}

export type EntidadeTempoReal =
  | 'admissao'
  | 'prospecto'
  | 'orcamento'
  | 'catalogo-orcamento'
  | 'cliente'
  | 'notificacao'
  | 'faturamento'
  | 'contrato'

export const ENTIDADES_DASHBOARD: readonly EntidadeTempoReal[] = [
  'admissao', 'prospecto', 'orcamento', 'cliente', 'notificacao', 'faturamento', 'contrato',
]

export type ListenerAtualizacaoTempoReal = (atualizacao: AtualizacaoTempoReal | null) => void

const apiBase = (import.meta.env.VITE_API_BASE_URL || '/api').replace(/\/$/, '')
const listeners = new Map<ListenerAtualizacaoTempoReal, Set<string>>()
let retryTimer: ReturnType<typeof setTimeout> | undefined
let startPromise: Promise<void> | undefined

const connection = new HubConnectionBuilder()
  .withUrl(`${apiBase}/v1/tempo-real`, {
    accessTokenFactory: () => accessToken() || '',
  })
  .withAutomaticReconnect([0, 2_000, 5_000, 10_000, 30_000])
  .configureLogging(LogLevel.None)
  .build()

function notificar(atualizacao: AtualizacaoTempoReal | null) {
  for (const [listener, entidades] of listeners) {
    if (atualizacao === null || entidades.has(atualizacao.entidade.toLowerCase())) listener(atualizacao)
  }
}

function agendarReconexao() {
  if (!listeners.size || retryTimer) return
  retryTimer = setTimeout(() => {
    retryTimer = undefined
    void iniciar()
  }, 5_000)
}

async function iniciar() {
  if (!listeners.size || connection.state !== HubConnectionState.Disconnected || startPromise) return

  startPromise = connection.start()
    .then(() => notificar(null))
    .catch(agendarReconexao)
    .finally(() => {
      startPromise = undefined
      if (!listeners.size) void connection.stop()
    })

  await startPromise
}

connection.on('Atualizacao', (atualizacao: AtualizacaoTempoReal) => {
  if (typeof atualizacao?.entidade === 'string') notificar(atualizacao)
})
connection.onreconnected(() => notificar(null))
connection.onclose(agendarReconexao)

export function observarAtualizacoes(
  entidades: EntidadeTempoReal | readonly EntidadeTempoReal[],
  listener: ListenerAtualizacaoTempoReal,
): () => void {
  listeners.set(listener, new Set((Array.isArray(entidades) ? entidades : [entidades]).map((entidade) => entidade.toLowerCase())))
  if (retryTimer) {
    clearTimeout(retryTimer)
    retryTimer = undefined
  }
  void iniciar()

  return () => {
    listeners.delete(listener)
    if (!listeners.size) {
      clearTimeout(retryTimer)
      retryTimer = undefined
      void connection.stop()
    }
  }
}
