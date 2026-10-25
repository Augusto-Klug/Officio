<script setup lang="ts">
import RadioButton from 'primevue/radiobutton'
import BaseButton from '../../../components/base/BaseButton.vue'
import { formatarCpf } from '../../../utils/inputMasks'
import type { RepresentanteDisponivel } from './representante'

defineProps<{ socios: RepresentanteDisponivel[]; loading: boolean; error: string; disabled?: boolean }>()
const selecionado = defineModel<string | null>({ required: true })
defineEmits<{ editar: []; atualizar: [] }>()
</script>

<template>
  <fieldset class="representatives">
    <legend>Representante do cliente</legend>
    <div class="representatives-heading">
      <p>Escolha um sócio com cadastro completo.</p>
      <BaseButton label="Adicionar sócio" icon="pi pi-plus" variant="secondary" :disabled="disabled || loading || !!error" @click="$emit('editar')" />
    </div>

    <p v-if="loading" class="representatives-state" role="status">Carregando sócios...</p>
    <div v-else-if="error" class="representatives-state" role="alert">
      <p>{{ error }}</p>
      <BaseButton label="Tentar novamente" variant="secondary" :disabled="disabled" @click="$emit('atualizar')" />
    </div>
    <div v-else-if="!socios.length" class="representatives-empty">
      <i class="pi pi-users" aria-hidden="true" />
      <div>
        <strong>Nenhum sócio cadastrado</strong>
        <p>Cadastre um representante antes de gerar o contrato.</p>
      </div>
      <BaseButton label="Cadastrar sócio" variant="secondary" :disabled="disabled" @click="$emit('editar')" />
    </div>
    <div v-else class="representative-list">
      <article
        v-for="socio in socios"
        :key="socio.id"
        class="representative"
        :class="{ 'is-selected': selecionado === socio.id }"
      >
        <div class="representative-heading">
          <div class="representative-choice">
            <RadioButton
              v-model="selecionado"
              :input-id="`representante-${socio.id}`"
              name="representante"
              :value="socio.id"
              :disabled="disabled || !socio.cadastroCompleto"
            />
            <label :for="`representante-${socio.id}`">{{ socio.nome || 'Nome não informado' }}</label>
          </div>
          <span class="representative-status" :class="socio.cadastroCompleto ? 'is-complete' : 'is-incomplete'">
            <i :class="socio.cadastroCompleto ? 'pi pi-check-circle' : 'pi pi-exclamation-circle'" aria-hidden="true" />
            {{ socio.cadastroCompleto ? 'Cadastro completo' : 'Cadastro incompleto' }}
          </span>
        </div>

        <dl class="representative-data">
          <div><dt>Tipo</dt><dd>{{ socio.tipoRelacao || 'Não informado' }}</dd></div>
          <div><dt>CPF</dt><dd>{{ formatarCpf(socio.cpf) || 'Não informado' }}</dd></div>
        </dl>

        <div v-if="!socio.cadastroCompleto" class="representative-pending">
          <p>Faltam informações obrigatórias para o contrato.</p>
          <details v-if="socio.informacoesFaltantes.length" class="pending-details">
            <summary>Ver pendências</summary>
            <ul><li v-for="pendencia in socio.informacoesFaltantes" :key="pendencia">{{ pendencia }}</li></ul>
          </details>
          <BaseButton label="Completar cadastro" variant="secondary" :disabled="disabled" @click="$emit('editar')" />
        </div>
      </article>
    </div>
  </fieldset>
</template>

<style scoped>
.representatives { display: grid; gap: 14px; min-width: 0; margin: 0; padding: 0; border: 0; }
.representatives-heading, .representative-heading { display: flex; align-items: center; justify-content: space-between; gap: 12px; }
.representatives legend { padding: 0; font-size: 15px; font-weight: 650; }
.representatives-heading p, .representatives-empty p, .representatives-state p, .representative-pending p { margin: 0; color: var(--officio-muted); font-size: 13px; line-height: 1.45; }
.representative-list { display: grid; gap: 10px; }
.representative { min-width: 0; padding: 14px 16px; border: 1px solid var(--officio-border); border-radius: 8px; background: var(--officio-surface); }
.representative.is-selected { border-color: #8ab49b; background: #f7fbf8; }
.representative-choice { display: flex; align-items: center; gap: 10px; min-width: 0; }
.representative-choice label { min-width: 0; font-size: 14px; font-weight: 600; overflow-wrap: anywhere; cursor: pointer; }
.representative-status { display: inline-flex; flex: 0 0 auto; align-items: center; gap: 6px; font-size: 11px; font-weight: 600; }
.representative-status.is-complete { color: #205740; }
.representative-status.is-incomplete { color: #8a5112; }
.representative-data { display: flex; gap: 28px; margin: 12px 0 0 30px; }
.representative-data div { min-width: 0; }
.representative-data dt { margin-bottom: 3px; color: var(--officio-muted); font-size: 11px; }
.representative-data dd { margin: 0; font-size: 13px; overflow-wrap: anywhere; }
.representative-pending { display: grid; justify-items: start; gap: 10px; margin: 12px 0 0 30px; padding-top: 11px; border-top: 1px solid var(--officio-border); }
.representative-pending p { margin: 0; }
.pending-details { color: var(--officio-muted); font-size: 12px; }
.pending-details summary { width: fit-content; cursor: pointer; text-decoration: underline; text-underline-offset: 2px; }
.pending-details ul { margin: 8px 0 0; padding-left: 18px; line-height: 1.5; }
.representatives-empty { display: flex; align-items: center; gap: 14px; padding: 20px; border: 1px dashed #b8c9bf; border-radius: 8px; background: #f8faf9; }
.representatives-empty > i { color: #426c58; font-size: 20px; }
.representatives-empty > div { flex: 1; min-width: 0; }
.representatives-empty strong { font-size: 14px; }
.representatives-state { margin: 0; color: var(--officio-muted); font-size: 13px; }
@media (max-width: 560px) {
  .representatives-heading { align-items: flex-start; flex-direction: column; }
  .representative-heading { align-items: flex-start; flex-direction: column; }
  .representative-data { gap: 18px; margin-left: 0; }
  .representative-pending { margin-left: 0; }
  .representatives-empty { align-items: flex-start; flex-wrap: wrap; }
  .representatives-empty > div { flex-basis: calc(100% - 36px); }
  .representatives-empty .base-button { margin-left: 34px; }
}
</style>
