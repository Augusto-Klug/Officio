<script setup lang="ts">
import axios from 'axios'
import Message from 'primevue/message'
import SelectButton from 'primevue/selectbutton'
import Skeleton from 'primevue/skeleton'
import { computed, onBeforeUnmount, ref, useId, watch } from 'vue'
import { normalizeError } from '../../../api/errors'
import BaseBadge from '../../../components/base/BaseBadge.vue'
import BaseButton from '../../../components/base/BaseButton.vue'
import {
  listarNotificacoes,
  marcarNotificacaoComoLida,
  marcarTodasNotificacoesComoLidas,
} from '../services/notificacoes.service'
import type { FiltroNotificacoes, Notificacao } from '../types/notificacao'

const props = withDefaults(defineProps<{
  titulo?: string
  somenteNaoLidas?: boolean
}>(), {
  titulo: 'Notificações',
  somenteNaoLidas: false,
})

const emit = defineEmits<{
  selecionar: [notificacao: Notificacao]
  atualizarNaoLidas: [quantidade: number]
}>()

const tituloId = `notificacoes-${useId()}`
const opcoesFiltro: Array<{ label: string; value: FiltroNotificacoes }> = [
  { label: 'Todas', value: 'todas' },
  { label: 'Não lidas', value: 'nao-lidas' },
]
const filtro = ref<FiltroNotificacoes>(props.somenteNaoLidas ? 'nao-lidas' : 'todas')
const notificacoes = ref<Notificacao[]>([])
const carregando = ref(true)
const erro = ref('')
const erroAcao = ref('')
const marcando = ref(new Set<string>())
const marcandoTodas = ref(false)
let controller: AbortController | undefined
let geracao = 0

const naoLidas = computed(() => notificacoes.value.filter((item) => !item.lida).length)
const possuiNaoLidas = computed(() => naoLidas.value > 0)

function informarNaoLidas() {
  emit('atualizarNaoLidas', naoLidas.value)
}

async function carregar() {
  const requisicao = ++geracao
  controller?.abort()
  controller = new AbortController()
  carregando.value = true
  erro.value = ''
  erroAcao.value = ''

  try {
    const resultado = await listarNotificacoes(
      filtro.value === 'nao-lidas' ? false : undefined,
      controller.signal,
    )
    if (requisicao !== geracao) return
    notificacoes.value = resultado
    informarNaoLidas()
  } catch (causa) {
    if (requisicao === geracao && !axios.isCancel(causa)) erro.value = normalizeError(causa).message
  } finally {
    if (requisicao === geracao) carregando.value = false
  }
}

async function marcarComoLida(notificacao: Notificacao) {
  if (notificacao.lida || marcando.value.has(notificacao.id)) return
  erroAcao.value = ''
  marcando.value = new Set(marcando.value).add(notificacao.id)

  try {
    await marcarNotificacaoComoLida(notificacao.id)
    if (filtro.value === 'nao-lidas') {
      notificacoes.value = notificacoes.value.filter((item) => item.id !== notificacao.id)
    } else {
      notificacoes.value = notificacoes.value.map((item) => item.id === notificacao.id
        ? { ...item, lida: true, lidaEm: new Date().toISOString() }
        : item)
    }
    informarNaoLidas()
  } catch (causa) {
    erroAcao.value = normalizeError(causa).message
  } finally {
    const pendentes = new Set(marcando.value)
    pendentes.delete(notificacao.id)
    marcando.value = pendentes
  }
}

async function marcarTodas() {
  if (!possuiNaoLidas.value || marcandoTodas.value) return
  erroAcao.value = ''
  marcandoTodas.value = true

  try {
    await marcarTodasNotificacoesComoLidas()
    if (filtro.value === 'nao-lidas') notificacoes.value = []
    else {
      const agora = new Date().toISOString()
      notificacoes.value = notificacoes.value.map((item) => item.lida ? item : { ...item, lida: true, lidaEm: agora })
    }
    informarNaoLidas()
  } catch (causa) {
    erroAcao.value = normalizeError(causa).message
  } finally {
    marcandoTodas.value = false
  }
}

function selecionar(notificacao: Notificacao) {
  if (!notificacao.lida) void marcarComoLida(notificacao)
  emit('selecionar', notificacao)
}

function icone(tipo: string): string {
  const icones: Record<string, string> = {
    CONTRATO_ASSINADO: 'pi pi-file-check',
    ORCAMENTO_ACEITO: 'pi pi-check-circle',
    PROSPECTO_CONVERTIDO: 'pi pi-user-plus',
    DOCUMENTO_REPROVADO: 'pi pi-times-circle',
    DOCUMENTO_REVISAO_MANUAL: 'pi pi-exclamation-triangle',
  }
  return icones[tipo] ?? 'pi pi-bell'
}

function categoria(tipo: string): string {
  const categorias: Record<string, string> = {
    CONTRATO_ASSINADO: 'Contrato',
    ORCAMENTO_ACEITO: 'Orçamento',
    PROSPECTO_CONVERTIDO: 'Prospecto',
    DOCUMENTO_REPROVADO: 'Documento',
    DOCUMENTO_REVISAO_MANUAL: 'Documento',
  }
  return categorias[tipo] ?? 'Atualização'
}

function formatarData(data: string): string {
  return new Intl.DateTimeFormat('pt-BR', {
    dateStyle: 'short',
    timeStyle: 'short',
  }).format(new Date(data))
}

watch(filtro, () => { void carregar() }, { immediate: true })
onBeforeUnmount(() => { ++geracao; controller?.abort() })
defineExpose({ recarregar: carregar })
</script>

<template>
  <section class="notifications-panel" :aria-labelledby="tituloId">
    <header class="panel-header">
      <div class="heading">
        <h2 :id="tituloId">{{ titulo }}</h2>
        <BaseBadge v-if="possuiNaoLidas" :label="`${naoLidas} não ${naoLidas === 1 ? 'lida' : 'lidas'}`" severity="info" />
      </div>
      <BaseButton
        label="Marcar todas como lidas"
        icon="pi pi-check-circle"
        variant="text"
        :loading="marcandoTodas"
        :disabled="carregando || !possuiNaoLidas"
        @click="marcarTodas"
      />
    </header>

    <div class="panel-filters">
      <SelectButton
        v-model="filtro"
        :options="opcoesFiltro"
        option-label="label"
        option-value="value"
        :allow-empty="false"
        aria-label="Filtrar notificações"
      />
    </div>

    <Message v-if="erroAcao" severity="error" :closable="false" role="alert">{{ erroAcao }}</Message>

    <div v-if="erro" class="panel-state">
      <Message severity="error" :closable="false" role="alert">{{ erro }}</Message>
      <BaseButton label="Tentar novamente" icon="pi pi-refresh" variant="secondary" @click="carregar" />
    </div>

    <div v-else-if="carregando" class="notification-list" aria-busy="true" aria-label="Carregando notificações">
      <div v-for="item in 3" :key="item" class="notification-skeleton">
        <Skeleton shape="circle" size="2.5rem" />
        <div><Skeleton width="9rem" height="1rem" /><Skeleton width="min(24rem, 70vw)" height="0.75rem" /></div>
      </div>
    </div>

    <div v-else-if="notificacoes.length === 0" class="panel-state empty-state">
      <i class="pi pi-bell" aria-hidden="true" />
      <h3>{{ filtro === 'nao-lidas' ? 'Tudo em dia' : 'Nenhuma notificação' }}</h3>
      <p>{{ filtro === 'nao-lidas' ? 'Você não possui notificações pendentes.' : 'As atualizações importantes aparecerão aqui.' }}</p>
    </div>

    <ul v-else class="notification-list" aria-label="Lista de notificações">
      <li v-for="notificacao in notificacoes" :key="notificacao.id" :class="['notification-item', { unread: !notificacao.lida }]">
        <button class="notification-content" type="button" :disabled="!notificacao.referenciaId" @click="selecionar(notificacao)">
          <span class="notification-icon" aria-hidden="true"><i :class="icone(notificacao.tipo)" /></span>
          <span class="notification-copy">
            <span class="notification-meta"><BaseBadge :label="categoria(notificacao.tipo)" /> <time :datetime="notificacao.criadoEm">{{ formatarData(notificacao.criadoEm) }}</time></span>
            <strong>{{ notificacao.titulo }}</strong>
            <span v-if="notificacao.mensagem">{{ notificacao.mensagem }}</span>
          </span>
        </button>
        <BaseButton
          v-if="!notificacao.lida"
          icon="pi pi-check"
          variant="text"
          :loading="marcando.has(notificacao.id)"
          :disabled="marcandoTodas"
          :aria-label="`Marcar ${notificacao.titulo} como lida`"
          title="Marcar como lida"
          @click="marcarComoLida(notificacao)"
        />
      </li>
    </ul>
  </section>
</template>

<style scoped>
.notifications-panel { display: grid; gap: 1rem; width: 100%; }
.panel-header, .heading, .panel-filters, .notification-item, .notification-content, .notification-meta { display: flex; align-items: center; }
.panel-header { justify-content: space-between; gap: 1rem; }
.heading { gap: .65rem; min-width: 0; }
.heading h2 { margin: 0; font-size: 1.125rem; letter-spacing: 0; }
.panel-filters { justify-content: flex-start; }
.panel-state { display: grid; justify-items: center; gap: .75rem; padding: 2rem 1rem; text-align: center; }
.panel-state :deep(.p-message) { width: 100%; }
.empty-state i { color: var(--p-text-muted-color); font-size: 1.75rem; }
.empty-state h3, .empty-state p { margin: 0; }
.empty-state p { color: var(--p-text-muted-color); }
.notification-list { display: grid; gap: 0; margin: 0; padding: 0; list-style: none; border-block: 1px solid var(--p-content-border-color); }
.notification-item { min-height: 5.25rem; gap: .5rem; border-bottom: 1px solid var(--p-content-border-color); }
.notification-item:last-child { border-bottom: 0; }
.notification-item.unread { background: var(--p-primary-50); box-shadow: inset 3px 0 0 var(--p-primary-500); }
.notification-content { flex: 1; gap: .85rem; min-width: 0; padding: .9rem 1rem; border: 0; background: transparent; color: inherit; text-align: left; font: inherit; }
.notification-content:not(:disabled) { cursor: pointer; }
.notification-content:not(:disabled):hover { background: color-mix(in srgb, var(--p-content-hover-background) 75%, transparent); }
.notification-content:focus-visible { outline: 2px solid var(--p-primary-color); outline-offset: -2px; }
.notification-icon { display: grid; flex: 0 0 2.5rem; width: 2.5rem; height: 2.5rem; place-items: center; border-radius: 50%; background: var(--p-content-hover-background); color: var(--p-primary-color); }
.notification-copy { display: grid; gap: .25rem; min-width: 0; line-height: 1.35; }
.notification-copy strong, .notification-copy > span:last-child { overflow-wrap: anywhere; }
.notification-copy > span:last-child, .notification-meta time { color: var(--p-text-muted-color); }
.notification-meta { flex-wrap: wrap; gap: .5rem; font-size: .78rem; }
.notification-item > :deep(.base-button) { flex: 0 0 auto; margin-right: .5rem; }
.notification-skeleton { display: grid; grid-template-columns: 2.5rem 1fr; gap: .85rem; align-items: center; min-height: 5.25rem; padding: .9rem 1rem; border-bottom: 1px solid var(--p-content-border-color); }
.notification-skeleton > div { display: grid; gap: .6rem; }
@media (max-width: 40rem) {
  .panel-header { align-items: flex-start; flex-direction: column; }
  .notification-content { padding-inline: .75rem; }
}
</style>
