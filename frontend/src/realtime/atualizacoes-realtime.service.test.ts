import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'

const signalr = vi.hoisted(() => {
  const handlers = new Map<string, (...args: unknown[]) => void>()
  let reconnected: (() => void) | undefined
  let closed: (() => void) | undefined
  const connection = {
    state: 'Disconnected',
    start: vi.fn(async () => { connection.state = 'Connected' }),
    stop: vi.fn(async () => { connection.state = 'Disconnected' }),
    on: vi.fn((event: string, handler: (...args: unknown[]) => void) => handlers.set(event, handler)),
    onreconnected: vi.fn((handler: () => void) => { reconnected = handler }),
    onclose: vi.fn((handler: () => void) => { closed = handler }),
  }
  return {
    handlers,
    connection,
    reconnect: () => reconnected?.(),
    close: () => { connection.state = 'Disconnected'; closed?.() },
    reset: () => {
      connection.state = 'Disconnected'
      connection.start.mockClear().mockImplementation(async () => { connection.state = 'Connected' })
      connection.stop.mockClear().mockImplementation(async () => { connection.state = 'Disconnected' })
    },
  }
})

vi.mock('@microsoft/signalr', () => ({
  HubConnectionState: { Disconnected: 'Disconnected' },
  LogLevel: { None: 6 },
  HubConnectionBuilder: class {
    withUrl() { return this }
    withAutomaticReconnect() { return this }
    configureLogging() { return this }
    build() { return signalr.connection }
  },
}))

import { observarAtualizacoes } from './atualizacoes-realtime.service'

describe('observarAtualizacoes', () => {
  beforeEach(() => signalr.reset())
  afterEach(() => vi.useRealTimers())

  it('filtra entidades sem diferenciar maiusculas e minusculas e recebe null de evento', async () => {
    const contrato = vi.fn()
    const admissao = vi.fn()
    const pararContrato = observarAtualizacoes(['contrato'], contrato)
    const pararAdmissao = observarAtualizacoes(['admissao'], admissao)
    await Promise.resolve()

    expect(contrato).toHaveBeenCalledOnce()
    expect(admissao).toHaveBeenCalledOnce()
    signalr.handlers.get('Atualizacao')?.({ entidade: 'CLIENTE', id: '1' })
    expect(contrato).toHaveBeenCalledOnce()
    expect(admissao).toHaveBeenCalledOnce()
    signalr.handlers.get('Atualizacao')?.({ entidade: 'AdMiSsAo', id: null })
    expect(admissao).toHaveBeenCalledTimes(2)
    expect(admissao).toHaveBeenLastCalledWith({ entidade: 'AdMiSsAo', id: null })

    pararContrato()
    pararAdmissao()
  })

  it('notifica os assinantes após reconexão e tenta novamente após fechamento', async () => {
    vi.useFakeTimers()
    const listener = vi.fn()
    const parar = observarAtualizacoes(['cliente'], listener)
    await Promise.resolve()
    signalr.reconnect()
    expect(listener).toHaveBeenCalledTimes(2)

    signalr.close()
    await vi.advanceTimersByTimeAsync(5_000)
    expect(signalr.connection.start).toHaveBeenCalledTimes(2)
    expect(listener).toHaveBeenCalledTimes(3)
    parar()
  })

  it('tenta novamente se a conexao inicial falhar', async () => {
    vi.useFakeTimers()
    signalr.connection.start.mockRejectedValueOnce(new Error('offline'))
    const parar = observarAtualizacoes(['prospecto'], vi.fn())
    await Promise.resolve()
    await vi.advanceTimersByTimeAsync(5_000)

    expect(signalr.connection.start).toHaveBeenCalledTimes(2)
    parar()
  })
})
