import { nextTick, ref } from 'vue'
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'

const hooks = vi.hoisted(() => ({
  mounted: undefined as (() => void) | undefined,
  unmounted: undefined as (() => void) | undefined,
  listener: undefined as (() => void) | undefined,
  observe: vi.fn((_entities: unknown, listener: () => void) => {
    hooks.listener = listener
    return vi.fn()
  }),
}))

vi.mock('vue', async (importOriginal) => {
  const original = await importOriginal<typeof import('vue')>()
  return {
    ...original,
    onMounted: (callback: () => void) => { hooks.mounted = callback },
    onBeforeUnmount: (callback: () => void) => { hooks.unmounted = callback },
  }
})

vi.mock('../realtime/atualizacoes-realtime.service', () => ({
  observarAtualizacoes: hooks.observe,
}))

import { useRealtimePageRefresh } from './useRealtimePageRefresh'

describe('useRealtimePageRefresh', () => {
  beforeEach(() => {
    hooks.mounted = undefined
    hooks.unmounted = undefined
    hooks.listener = undefined
    hooks.observe.mockClear()
  })

  afterEach(() => {
    hooks.unmounted?.()
  })

  it('assina entidades, conserva refresh pendente durante busy e limpa assinatura ao desmontar', async () => {
    const busy = ref(true)
    const refresh = vi.fn().mockResolvedValue(undefined)
    const unsubscribe = vi.fn()
    hooks.observe.mockImplementation((_entities, listener) => {
      hooks.listener = listener
      return unsubscribe
    })

    const coordinator = useRealtimePageRefresh(['admissao', 'cliente'], refresh, busy)
    hooks.mounted?.()

    expect(hooks.observe).toHaveBeenCalledWith(['admissao', 'cliente'], expect.any(Function))
    hooks.listener?.()
    expect(refresh).not.toHaveBeenCalled()

    busy.value = false
    await nextTick()
    await Promise.resolve()
    expect(refresh).toHaveBeenCalledOnce()

    hooks.unmounted?.()
    hooks.unmounted = undefined
    expect(unsubscribe).toHaveBeenCalledOnce()
    coordinator.notificar()
    expect(refresh).toHaveBeenCalledOnce()
  })
})
