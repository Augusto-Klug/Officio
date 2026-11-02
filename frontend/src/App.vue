<script setup lang="ts">
import { onBeforeUnmount, onMounted } from 'vue'

let observer: MutationObserver | undefined
let frame = 0
const errorSelector = '.p-message-error, .representatives-state[role="alert"]'

function revealError(element: HTMLElement) {
  cancelAnimationFrame(frame)
  frame = requestAnimationFrame(() => {
    if (!element.isConnected || !element.getClientRects().length) return
    const dialogContent = element.closest<HTMLElement>('.p-dialog-content')
    if (dialogContent) dialogContent.scrollTo({ top: 0, behavior: 'smooth' })
    else window.scrollTo({ top: 0, behavior: 'smooth' })
  })
}

onMounted(() => {
  observer = new MutationObserver(records => {
    for (const record of [...records].reverse()) {
      const target = record.target instanceof Element ? record.target : record.target.parentElement
      const alert = target?.closest<HTMLElement>(errorSelector)
      if (alert) { revealError(alert); return }
      for (const node of [...record.addedNodes].reverse()) {
        if (!(node instanceof HTMLElement)) continue
        const addedAlert = node.matches(errorSelector)
          ? node
          : node.querySelector<HTMLElement>(errorSelector)
        if (addedAlert) { revealError(addedAlert); return }
      }
    }
  })
  observer.observe(document.body, { childList: true, characterData: true, subtree: true })
})

onBeforeUnmount(() => {
  observer?.disconnect()
  cancelAnimationFrame(frame)
})
</script>

<template><RouterView /></template>
