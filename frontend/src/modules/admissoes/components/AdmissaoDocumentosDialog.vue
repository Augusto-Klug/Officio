<script setup lang="ts">
import { computed } from 'vue'
import Dialog from 'primevue/dialog'
import BaseButton from '../../../components/base/BaseButton.vue'
import StatusBadge from '../../../components/shared/StatusBadge.vue'
import type { AdmissaoResumo } from '../types/admissao'
import AdmissaoDocumentosSection from './AdmissaoDocumentosSection.vue'

const visible = defineModel<boolean>('visible', { required: true })
const props = defineProps<{ admissao: AdmissaoResumo | null; linkLoading?: boolean }>()
const emit = defineEmits<{ atualizado: []; gerarLink: [admissaoId: string] }>()

const terminal = computed(() => props.admissao?.status === 'CONCLUIDA' || props.admissao?.status === 'CANCELADA')
const severity = computed(() => {
  if (props.admissao?.status === 'CONCLUIDA') return 'success'
  if (props.admissao?.status === 'CANCELADA') return 'danger'
  return 'warn'
})
</script>

<template>
  <Dialog v-model:visible="visible" modal header="Documentos da admissão" class="workflow-dialog admission-documents-dialog" :style="{ width: 'min(980px, 96vw)' }">
    <div v-if="admissao" class="documents-dialog-content">
      <header class="documents-dialog-heading">
        <div><h2>{{ admissao.nomeFuncionario }}</h2><p>{{ admissao.empresaNome }}</p></div>
        <StatusBadge :label="admissao.statusDescricao" :severity="severity" />
      </header>
      <AdmissaoDocumentosSection :admissao-id="admissao.id" :editable="!terminal" @atualizado="emit('atualizado')" />
    </div>
    <div v-else class="list-state compact-state" aria-busy="true"><i class="pi pi-spinner loading-spinner" /><p>Carregando admissão...</p></div>
    <template #footer>
      <BaseButton label="Fechar" variant="text" @click="visible = false" />
      <BaseButton v-if="admissao && !terminal" label="Gerar ficha digital" icon="pi pi-link" variant="secondary" :loading="linkLoading" :disabled="linkLoading" @click="emit('gerarLink', admissao.id)" />
    </template>
  </Dialog>
</template>

<style scoped>
.documents-dialog-content { display: grid; gap: 20px; }
.documents-dialog-heading { display: flex; align-items: flex-start; justify-content: space-between; gap: 16px; }
.documents-dialog-heading h2 { margin: 0; font-size: 18px; overflow-wrap: anywhere; }
.documents-dialog-heading p { margin: 4px 0 0; color: var(--officio-muted); font-size: 13px; }
</style>
