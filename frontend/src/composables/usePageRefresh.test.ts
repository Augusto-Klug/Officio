import { ref } from 'vue'
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { usePageRefresh } from './usePageRefresh'

const lifecycle = vi.hoisted(() => ({
  mount: undefined as (() => void) | undefined,
  unmount: undefined as (() => void) | undefined,
}))

vi.mock('vue', async (importOriginal) => {
  const original = await importOriginal<typeof import('vue')>()
  return {
    ...original,
    onMounted: (callback: () => void) => { lifecycle.mount = callback },
    onBeforeUnmount: (callback: () => void) => { lifecycle.unmount = callback },
  }
})

describe('usePageRefresh', () => {
  const windowListeners = new Map<string, () => void>()
  const documentListeners = new Map<string, () => void>()

  beforeEach(() => {
    vi.useFakeTimers()
    vi.setSystemTime(new Date('2026-09-26T12:00:00Z'))
    windowListeners.clear()
    documentListeners.clear()
    lifecycle.mount = undefined
    lifecycle.unmount = undefined

    vi.stubGlobal('navigator', { onLine: true })
    vi.stubGlobal('window', {
      addEventListener: vi.fn((event: string, callback: () => void) => windowListeners.set(event, callback)),
      removeEventListener: vi.fn((event: string) => windowListeners.delete(event)),
    })
    vi.stubGlobal('document', {
      visibilityState: 'visible',
      addEventListener: vi.fn((event: string, callback: () => void) => documentListeners.set(event, callback)),
      removeEventListener: vi.fn((event: string) => documentListeners.delete(event)),
    })
  })

  afterEach(() => {
    lifecycle.unmount?.()
    vi.useRealTimers()
    vi.unstubAllGlobals()
  })

  it('atualiza manualmente e ao recuperar foco sem duplicar eventos proximos', async () => {
    const refresh = vi.fn().mockResolvedValue(undefined)
    const { refreshNow } = usePageRefresh(refresh)
    lifecycle.mount?.()

    await refreshNow()
    windowListeners.get('focus')?.()
    documentListeners.get('visibilitychange')?.()
    await Promise.resolve()

    expect(refresh).toHaveBeenCalledTimes(2)
    expect(refresh).toHaveBeenNthCalledWith(1, 'manual')
    expect(refresh).toHaveBeenNthCalledWith(2, 'focus')
  })

  it('faz polling somente quando habilitado, visivel, online e desocupado', async () => {
    const refresh = vi.fn().mockResolvedValue(undefined)
    const pollingEnabled = ref(false)
    const busy = ref(false)
    usePageRefresh(refresh, { pollingEnabled, busy, pollingIntervalMs: 15_000 })
    lifecycle.mount?.()

    await vi.advanceTimersByTimeAsync(15_000)
    expect(refresh).not.toHaveBeenCalled()

    pollingEnabled.value = true
    await vi.advanceTimersByTimeAsync(15_000)
    expect(refresh).toHaveBeenCalledWith('polling')

    busy.value = true
    await vi.advanceTimersByTimeAsync(15_000)
    expect(refresh).toHaveBeenCalledTimes(1)
  })
})
