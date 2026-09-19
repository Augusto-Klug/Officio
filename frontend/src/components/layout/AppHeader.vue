<script setup lang="ts">
import { computed, ref } from 'vue'
import Popover from 'primevue/popover'
import { useRoute, useRouter } from 'vue-router'
import { session, clearSession, hasRole } from '../../stores/session'
import type { Notificacao } from '../../modules/notificacoes/types/notificacao'
import NotificacoesPainel from '../../modules/notificacoes/components/NotificacoesPainel.vue'
import BaseButton from '../base/BaseButton.vue'

defineEmits<{ menu: [] }>()
const route = useRoute()
const router = useRouter()
const popover = ref<InstanceType<typeof Popover> | null>(null)
const notificacoes = ref<InstanceType<typeof NotificacoesPainel> | null>(null)
const naoLidas = ref(0)
const pageTitle = computed(() => String(route.meta.title || 'Comercial'))
const areaTitle = computed(() => route.path.startsWith('/admin/') ? 'Plataforma' : route.path === '/acessos' ? 'Configurações' : 'Comercial')
const platformAdmin = hasRole('PlatformAdmin')

function logout() {
  clearSession()
  void router.replace('/login')
}

function toggleNotifications(event: Event) {
  popover.value?.toggle(event)
  void notificacoes.value?.recarregar()
}

function selectNotification(notification: Notificacao) {
  popover.value?.hide()
  if (notification.tipo === 'CONTRATO_ASSINADO') void router.push('/contratos')
  else if (notification.tipo === 'PROSPECTO_CONVERTIDO') void router.push('/clientes')
  else if (notification.tipo === 'ORCAMENTO_ACEITO') void router.push('/prospectos')
  else if (notification.tipo === 'DOCUMENTO_REPROVADO' || notification.tipo === 'DOCUMENTO_REVISAO_MANUAL') void router.push('/clientes')
}
</script>

<template>
  <header class="app-header">
    <div class="header-leading">
      <BaseButton class="mobile-menu" icon="pi pi-bars" variant="text" aria-label="Abrir navegação" @click="$emit('menu')" />
      <span>{{ areaTitle }} <span class="breadcrumb-divider" aria-hidden="true">/</span> <strong>{{ pageTitle }}</strong></span>
    </div>
    <div class="header-account">
      <div v-if="!platformAdmin" class="notification-trigger">
        <BaseButton icon="pi pi-bell" variant="text" aria-label="Abrir notificações" aria-haspopup="dialog" @click="toggleNotifications" />
        <span v-if="naoLidas" class="notification-count" :aria-label="`${naoLidas} notificações não lidas`">{{ naoLidas > 9 ? '9+' : naoLidas }}</span>
      </div>
      <span class="account-name">{{ session?.usuario.nome }}</span>
      <BaseButton label="Sair" icon="pi pi-sign-out" variant="text" @click="logout" />
    </div>
    <Popover v-if="!platformAdmin" ref="popover" class="notifications-popover">
      <NotificacoesPainel ref="notificacoes" titulo="Notificações" @atualizar-nao-lidas="naoLidas = $event" @selecionar="selectNotification" />
    </Popover>
  </header>
</template>
