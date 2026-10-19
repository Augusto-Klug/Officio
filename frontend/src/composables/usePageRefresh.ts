import { onBeforeUnmount, onMounted, ref, toValue, type MaybeRefOrGetter } from 'vue'

export type PageRefreshReason = 'manual' | 'focus' | 'online' | 'polling' | 'realtime'

interface PageRefreshOptions {
  busy?: MaybeRefOrGetter<boolean>
  pollingEnabled?: MaybeRefOrGetter<boolean>
  pollingIntervalMs?: number
}

export function usePageRefresh(
  refresh: (reason: PageRefreshReason) => Promise<void>,
  options: PageRefreshOptions = {},
) {
  const refreshing = ref(false)
  let pollingTimer: ReturnType<typeof setInterval> | undefined
  let lastAutomaticRefreshAt: number | undefined

  function canRefresh(reason: PageRefreshReason) {
    return !refreshing.value
      && !toValue(options.busy ?? false)
      && (reason === 'manual' || (navigator.onLine && document.visibilityState === 'visible'))
  }

  async function run(reason: PageRefreshReason) {
    if (!canRefresh(reason)) return

    const automatic = reason !== 'manual'
    const now = Date.now()
    if (automatic && lastAutomaticRefreshAt !== undefined && now - lastAutomaticRefreshAt < 1_000) return

    if (automatic) lastAutomaticRefreshAt = now
    refreshing.value = true
    try {
      await refresh(reason)
    } finally {
      refreshing.value = false
    }
  }

  function handleFocus() {
    void run('focus')
  }

  function handleVisibilityChange() {
    if (document.visibilityState === 'visible') void run('focus')
  }

  function handleOnline() {
    void run('online')
  }

  onMounted(() => {
    window.addEventListener('focus', handleFocus)
    window.addEventListener('online', handleOnline)
    document.addEventListener('visibilitychange', handleVisibilityChange)

    if (options.pollingEnabled) {
      pollingTimer = setInterval(() => {
        if (toValue(options.pollingEnabled ?? false)) void run('polling')
      }, options.pollingIntervalMs ?? 15_000)
    }
  })

  onBeforeUnmount(() => {
    window.removeEventListener('focus', handleFocus)
    window.removeEventListener('online', handleOnline)
    document.removeEventListener('visibilitychange', handleVisibilityChange)
    clearInterval(pollingTimer)
  })

  return {
    refreshing,
    refreshNow: () => run('manual'),
    refreshAutomatically: () => run('realtime'),
  }
}
