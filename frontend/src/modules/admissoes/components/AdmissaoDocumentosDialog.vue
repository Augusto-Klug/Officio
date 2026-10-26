<script setup lang="ts">
import { computed } from 'vue'
import Dialog from 'primevue/dialog'
import BaseButton from '../../../components/base/BaseButton.vue'
import StatusBadge from '../../../components/shared/StatusBadge.vue'
import type { AdmissaoResumo } from '../types/admissao'
import AdmissaoDocumentosSection from './AdmissaoDocumentosSection.vue'

const visible = defineModel<boolean>('visible', { required: true })
const props = defineProps<{ admissao: AdmissaoResumo | null; loading?: boolean; linkLoading?: boolean }>()
const emit = defineEmits<{ gerarLink: [admissaoId: string, papel: 'funcionario' | 'empresa']; concluirRevisao: [admissaoId: string] }>()

const terminal = computed(() => props.admissao?.status === 'CONCLUIDA' || props.admissao?.status === 'CANCELADA')
const funcionarioConcluiu = computed(() => props.admissao?.preenchimentoFuncionarioConcluido === true)
const empresaConcluiu = computed(() => props.admissao?.preenchimentoEmpresaConcluido === true)
const aguardandoValidacao = computed(() => props.admissao?.status === 'AGUARDANDO_VALIDACAO')
const documentosValidados = computed(() => ['DOCUMENTOS_VALIDADOS', 'AGUARDANDO_EMPREGADOR', 'LIBERADO_PELO_EMPREGADOR', 'LIBERADO_PARA_REVISAO_FINAL', 'CONCLUIDA'].includes(props.admissao?.status ?? ''))
const documentosEditaveis = computed(() => ['RASCUNHO', 'AGUARDANDO_ADMITIDO', 'AGUARDANDO_VALIDACAO'].includes(props.admissao?.status ?? ''))
const severity = computed(() => {
  if (props.admissao?.status === 'CONCLUIDA') return 'success'
  if (props.admissao?.status === 'CANCELADA') return 'danger'
  if (['DOCUMENTOS_VALIDADOS', 'AGUARDANDO_EMPREGADOR', 'LIBERADO_PELO_EMPREGADOR', 'LIBERADO_PARA_REVISAO_FINAL'].includes(props.admissao?.status ?? '')) return 'info'
  return 'warn'
})
</script>

<template>
  <Dialog v-model:visible="visible" modal header="Documentos da admissão" class="workflow-dialog admission-documents-dialog" :style="{ width: 'min(980px, 96vw)' }">
    <div v-if="loading" class="list-state compact-state" aria-busy="true"><i class="pi pi-spinner loading-spinner" /><p>Atualizando admissão...</p></div>
    <div v-else-if="admissao" class="documents-dialog-content">
      <header class="documents-dialog-heading">
        <div><h2>{{ admissao.nomeFuncionario }}</h2><p>{{ admissao.empresaNome }}</p></div>
        <StatusBadge :label="admissao.statusDescricao" :severity="severity" />
      </header>
      <div class="admission-form-progress" aria-label="Etapas de preenchimento">
        <div><span>Admitido</span><StatusBadge :label="funcionarioConcluiu ? 'Concluído' : 'Pendente'" :severity="funcionarioConcluiu ? 'success' : 'warn'" /></div>
        <div><span>Revisão documental</span><StatusBadge :label="documentosValidados ? 'Concluída' : funcionarioConcluiu ? 'Pendente' : 'Aguardando admitido'" :severity="documentosValidados ? 'success' : funcionarioConcluiu ? 'warn' : 'secondary'" /></div>
        <div><span>Empregador</span><StatusBadge :label="empresaConcluiu ? 'Concluído' : 'Pendente'" :severity="empresaConcluiu ? 'success' : 'warn'" /></div>
      </div>
      <AdmissaoDocumentosSection :admissao-id="admissao.id" :editable="!terminal && documentosEditaveis" :can-validate="aguardandoValidacao" />
    </div>
    <div v-else class="list-state compact-state"><p>Não foi possível carregar a admissão.</p></div>
    <template #footer>
      <BaseButton label="Fechar" variant="text" @click="visible = false" />
      <BaseButton v-if="admissao && !terminal && !funcionarioConcluiu" label="Gerar link do admitido" icon="pi pi-link" variant="secondary" :loading="linkLoading" :disabled="linkLoading" @click="emit('gerarLink', admissao.id, 'funcionario')" />
      <BaseButton v-if="admissao && !terminal && !empresaConcluiu" label="Gerar link do empregador" icon="pi pi-link" variant="secondary" :loading="linkLoading" :disabled="linkLoading" @click="emit('gerarLink', admissao.id, 'empresa')" />
      <BaseButton v-if="admissao && aguardandoValidacao" label="Concluir revisão documental" icon="pi pi-check-circle" :loading="linkLoading" :disabled="linkLoading" @click="emit('concluirRevisao', admissao.id)" />
    </template>
  </Dialog>
</template>

<style scoped>
.documents-dialog-content { display: grid; gap: 20px; }
.documents-dialog-heading { display: flex; align-items: flex-start; justify-content: space-between; gap: 16px; }
.documents-dialog-heading h2 { margin: 0; font-size: 18px; overflow-wrap: anywhere; }
.documents-dialog-heading p { margin: 4px 0 0; color: var(--officio-muted); font-size: 13px; }
.admission-form-progress { display: grid; grid-template-columns: repeat(3, minmax(0, 1fr)); gap: 16px; padding: 12px 0; border-top: 1px solid var(--officio-border); border-bottom: 1px solid var(--officio-border); }
.admission-form-progress > div { display: flex; align-items: center; justify-content: space-between; gap: 8px; color: var(--officio-muted); font-size: 13px; }
@media (max-width: 560px) { .admission-form-progress { grid-template-columns: 1fr; } }
</style>
