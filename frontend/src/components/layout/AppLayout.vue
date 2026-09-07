<script setup lang="ts">
import { ref } from 'vue'
import Drawer from 'primevue/drawer'
import AppSidebar from './AppSidebar.vue'
import AppHeader from './AppHeader.vue'
import PageContainer from './PageContainer.vue'
const menuOpen = ref(false)
</script>
<template>
  <a class="skip-link" href="#main-content">Ir para o conteúdo</a>
  <div class="app-layout">
    <aside class="desktop-sidebar"><AppSidebar /></aside>
    <Drawer v-model:visible="menuOpen" header="Navegação" class="mobile-sidebar"><AppSidebar @navigate="menuOpen = false" /></Drawer>
    <div class="app-body"><AppHeader @menu="menuOpen = true" /><PageContainer><RouterView /></PageContainer></div>
  </div>
</template>
