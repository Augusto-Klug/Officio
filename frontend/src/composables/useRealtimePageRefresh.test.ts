import { afterEach, describe, expect, it, vi } from 'vitest'

vi.mock('../realtime/atualizacoes-realtime.service', () => ({
  observarAtualizacoes: vi.fn(() => vi.fn()),
}))

import { criarCoordenadorRefreshTempoReal } from './useRealtimePageRefresh'

describe('criarCoordenadorRefreshTempoReal', () => {
  afterEach(() => vi.useRealTimers())

  it('coalesce eventos durante refresh e executa uma unica atualizacao pendente', async () => {
    let finalizarRefresh!: () => void
    let ocupado = false
    const refresh = vi.fn(() => new Promise<void>((resolve) => { finalizarRefresh = resolve }))
    const coordenador = criarCoordenadorRefreshTempoReal(refresh, () => ocupado)

    coordenador.notificar()
    coordenador.notificar()
    expect(refresh).toHaveBeenCalledOnce()

    coordenador.notificar()
    coordenador.notificar()
    expect(refresh).toHaveBeenCalledOnce()

    finalizarRefresh()
    await Promise.resolve()
    await Promise.resolve()
    expect(refresh).toHaveBeenCalledTimes(2)
    coordenador.descartar()
  })

  it('guarda notificacao enquanto a view esta ocupada e descarta apos teardown', async () => {
    let ocupado = true
    const refresh = vi.fn().mockResolvedValue(undefined)
    const coordenador = criarCoordenadorRefreshTempoReal(refresh, () => ocupado)

    coordenador.notificar()
    expect(refresh).not.toHaveBeenCalled()
    ocupado = false
    coordenador.processarPendente()
    await Promise.resolve()
    expect(refresh).toHaveBeenCalledOnce()

    coordenador.descartar()
    coordenador.notificar()
    expect(refresh).toHaveBeenCalledOnce()
  })
})
