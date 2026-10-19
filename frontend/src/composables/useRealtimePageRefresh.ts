import { onBeforeUnmount, onMounted, toValue, watch, type MaybeRefOrGetter } from 'vue'
import { observarAtualizacoes, type EntidadeTempoReal } from '../realtime/atualizacoes-realtime.service'

export function criarCoordenadorRefreshTempoReal(refresh: () => void | Promise<void>, ocupado: () => boolean) {
  let pendente = false
  let atualizando = false
  let descartado = false

  function processarPendente() {
    if (descartado || !pendente || atualizando || ocupado()) return
    pendente = false
    atualizando = true
    let resultado: void | Promise<void>
    try {
      resultado = refresh()
    } catch {
      resultado = undefined
    }
    void Promise.resolve(resultado)
      .catch(() => undefined)
      .finally(() => {
        atualizando = false
        processarPendente()
      })
  }

  return {
    notificar() {
      if (descartado) return
      pendente = true
      processarPendente()
    },
    processarPendente,
    descartar() {
      descartado = true
      pendente = false
    },
  }
}

export function useRealtimePageRefresh(
  entidades: EntidadeTempoReal | readonly EntidadeTempoReal[],
  refresh: () => void | Promise<void>,
  busy: MaybeRefOrGetter<boolean>,
) {
  const coordenador = criarCoordenadorRefreshTempoReal(refresh, () => toValue(busy))
  let pararObservacao: (() => void) | undefined

  onMounted(() => {
    pararObservacao = observarAtualizacoes(entidades, () => coordenador.notificar())
  })

  const pararWatch = watch(() => toValue(busy), (ocupado) => {
    if (!ocupado) coordenador.processarPendente()
  })

  onBeforeUnmount(() => {
    pararObservacao?.()
    pararWatch()
    coordenador.descartar()
  })

  return coordenador
}
