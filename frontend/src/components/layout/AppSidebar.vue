<script setup lang="ts">
import { hasRole } from '../../stores/session'
defineEmits<{ navigate: [] }>()
const platformAdmin = hasRole('PlatformAdmin')
</script>
<template>
  <div class="sidebar-content">
    <RouterLink :to="platformAdmin ? '/admin/parceiros' : '/dashboard'" class="brand" aria-label="Officio" @click="$emit('navigate')">
      <img src="/favicon.svg" width="36" height="36" alt="" /><span translate="no">officio<span class="brand-dot">.</span></span>
    </RouterLink>
    <div class="workspace-label">{{ platformAdmin ? 'Administração da plataforma' : 'Gestão do escritório' }}</div>
    <nav aria-label="Navegação principal">
      <template v-if="!platformAdmin">
        <p class="nav-section">ESCRITORIO</p>
        <RouterLink to="/dashboard" class="nav-item" @click="$emit('navigate')"><i class="pi pi-chart-bar" aria-hidden="true" />Visao geral</RouterLink>
        <p class="nav-section nav-section-spaced">COMERCIAL</p>
        <RouterLink to="/prospectos" class="nav-item" @click="$emit('navigate')"><i class="pi pi-users" aria-hidden="true" />Prospectos</RouterLink>
        <RouterLink to="/clientes" class="nav-item" @click="$emit('navigate')"><i class="pi pi-building" aria-hidden="true" />Clientes</RouterLink>
        <RouterLink to="/contratos" class="nav-item" @click="$emit('navigate')"><i class="pi pi-file" aria-hidden="true" />Contratos</RouterLink>
        <RouterLink to="/faturamentos" class="nav-item" @click="$emit('navigate')"><i class="pi pi-receipt" aria-hidden="true" />Faturamento</RouterLink>
        <p class="nav-section nav-section-spaced">OPERACIONAL</p>
        <RouterLink to="/admissoes" class="nav-item" @click="$emit('navigate')"><i class="pi pi-id-card" aria-hidden="true" />Admissões</RouterLink>
      </template>
      <template v-if="hasRole('Admin') && !platformAdmin">
        <p class="nav-section nav-section-spaced">CONFIGURAÇÕES</p>
        <RouterLink to="/acessos" class="nav-item" @click="$emit('navigate')"><i class="pi pi-user-edit" aria-hidden="true" />Usuários e acessos</RouterLink>
      </template>
      <template v-if="platformAdmin">
        <p class="nav-section">PLATAFORMA</p>
        <RouterLink to="/admin/parceiros" class="nav-item" @click="$emit('navigate')"><i class="pi pi-sitemap" aria-hidden="true" />Parceiros</RouterLink>
      </template>
    </nav>
    <div class="sidebar-footer"><span class="sidebar-mark" aria-hidden="true">O</span><div>Officio<small>Gestão contábil</small></div></div>
  </div>
</template>
