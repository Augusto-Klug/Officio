<script setup lang="ts">
import Tag from 'primevue/tag'
withDefaults(defineProps<{ label: string; severity?: 'info' | 'success' | 'warn' | 'danger' | 'secondary' }>(), { severity: 'secondary' })
</script>
<template><Tag :value="label" :severity="severity" class="base-badge" /></template>
