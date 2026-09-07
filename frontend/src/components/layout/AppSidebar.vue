<script setup lang="ts">
defineEmits<{ navigate: [] }>()
</script>
<template>
  <div class="sidebar-content">
    <RouterLink to="/prospectos" class="brand" aria-label="Officio — Prospectos" @click="$emit('navigate')">
      <img src="/favicon.svg" width="36" height="36" alt="" /><span translate="no">officio<span class="brand-dot">.</span></span>
    </RouterLink>
    <div class="workspace-label">Gestão do escritório</div>
    <nav aria-label="Navegação principal">
      <p class="nav-section">COMERCIAL</p>
      <RouterLink to="/prospectos" class="nav-item" @click="$emit('navigate')"><i class="pi pi-users" aria-hidden="true" />Prospectos</RouterLink>
    </nav>
    <div class="sidebar-footer"><span class="sidebar-mark" aria-hidden="true">O</span><div>Officio<small>Gestão contábil</small></div></div>
  </div>
</template>
