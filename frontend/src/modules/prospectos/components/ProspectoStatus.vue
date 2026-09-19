<script setup lang="ts">
import { computed } from 'vue'
import StatusBadge from '../../../components/shared/StatusBadge.vue'
import { statusOptions } from '../types/prospecto'
const props = defineProps<{ status: string | null }>()
const statusInfo = computed(() => statusOptions.find((item) => item.value.toLowerCase() === props.status?.toLowerCase()))
</script>
<template><StatusBadge :label="statusInfo?.label || status || 'Não informado'" :severity="statusInfo?.severity" /></template>
