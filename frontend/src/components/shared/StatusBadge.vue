<script setup lang="ts">
import BaseBadge from '../base/BaseBadge.vue'
withDefaults(defineProps<{ label: string; severity?: 'info' | 'success' | 'warn' | 'danger' | 'secondary' }>(), { severity: 'secondary' })
</script>
<template><BaseBadge :label="label" :severity="severity" class="status-badge" /></template>
