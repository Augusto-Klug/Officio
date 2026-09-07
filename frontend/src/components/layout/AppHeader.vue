<script setup lang="ts">
import { session, clearSession } from '../../stores/session'
import { useRouter } from 'vue-router'
import BaseButton from '../base/BaseButton.vue'
defineEmits<{ menu: [] }>()
const router = useRouter()
function logout() { clearSession(); void router.replace('/login') }
</script>
<template>
  <header class="app-header">
    <div class="header-leading"><BaseButton class="mobile-menu" icon="pi pi-bars" variant="text" aria-label="Abrir navegação" @click="$emit('menu')" /><span>Comercial <span class="breadcrumb-divider" aria-hidden="true">/</span> <strong>Prospectos</strong></span></div>
    <div class="header-account"><span class="account-name">{{ session?.usuario.nome }}</span><BaseButton label="Sair" icon="pi pi-sign-out" variant="text" @click="logout" /></div>
  </header>
</template>
