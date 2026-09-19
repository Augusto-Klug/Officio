<script setup lang="ts">
import { computed, onBeforeUnmount, ref, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import axios from 'axios'
import Button from 'primevue/button'
import Checkbox from 'primevue/checkbox'
import Column from 'primevue/column'
import DataTable from 'primevue/datatable'
import Dialog from 'primevue/dialog'
import InputNumber from 'primevue/inputnumber'
import InputText from 'primevue/inputtext'
import Message from 'primevue/message'
import Select from 'primevue/select'
import Textarea from 'primevue/textarea'
import BaseButton from '../../../components/base/BaseButton.vue'
import BaseCard from '../../../components/base/BaseCard.vue'
import PageHeader from '../../../components/layout/PageHeader.vue'
import StatusBadge from '../../../components/shared/StatusBadge.vue'
import OrcamentoDetalheDialog from '../components/OrcamentoDetalheDialog.vue'
import { normalizeError } from '../../../api/errors'
import { formatCurrency } from '../../../utils/format'
import { aceitarOrcamento, atualizarOrcamento, converterOrcamento, criarOrcamento, listarOrcamentos, obterOrcamento } from '../services/orcamentos.service'
import { secaoOptions, secaoPorNome, tipoOptions, tipoPorNome, type CriarOrcamentoPayload, type Orcamento } from '../types/orcamento'

const route = useRoute()
const router = useRouter()
const prospectoId = computed(() => String(route.params.prospectoId || ''))
const prospectoNome = computed(() => typeof route.query.nome === 'string' ? route.query.nome : 'Prospecto selecionado')
const orcamentos = ref<Orcamento[]>([])
const loading = ref(true)
const error = ref('')
const success = ref('')
const actionId = ref('')
const detailLoadingId = ref('')
const createOpen = ref(false)
const detailOpen = ref(false)
const convertOpen = ref(false)
const selected = ref<Orcamento | null>(null)
const editingId = ref('')
const saving = ref(false)
const formError = ref('')
const form = ref(novoFormulario())
const conversion = ref({ dataInicio: hojeLocal(), dataFim: '', renovacaoAutomatica: true, clausulasExtras: '' })
let controller: AbortController | undefined

function novoFormulario(): CriarOrcamentoPayload {
  return {
    prospectoId: prospectoId.value,
    titulo: '',
    diaVencimento: 10,
    observacoes: '',
    itens: [{ descricao: '', secao: 2, tipo: 2, quantidade: 1, valorUnitario: 0, ordem: 1 }],
  }
}

function hojeLocal(): string {
  const agora = new Date()
  const local = new Date(agora.getTime() - agora.getTimezoneOffset() * 60_000)
  return local.toISOString().slice(0, 10)
}

async function load() {
  controller?.abort()
  controller = new AbortController()
  loading.value = true
  error.value = ''
  try { orcamentos.value = await listarOrcamentos(prospectoId.value, controller.signal) }
  catch (cause) { if (!axios.isCancel(cause)) error.value = normalizeError(cause).message }
  finally { loading.value = false }
}

function abrirCriacao() {
  editingId.value = ''
  form.value = novoFormulario()
  formError.value = ''
  createOpen.value = true
}

async function abrirDetalhe(item: Orcamento) {
  if (actionId.value || detailLoadingId.value) return
  detailLoadingId.value = item.id
  error.value = ''
  try {
    selected.value = await obterOrcamento(item.id)
    detailOpen.value = true
  } catch (cause) { error.value = normalizeError(cause).message }
  finally { detailLoadingId.value = '' }
}

function abrirEdicao(item: Orcamento) {
  selected.value = item
  editingId.value = item.id
  form.value = {
    prospectoId: item.prospectoId,
    titulo: item.titulo ?? '',
    diaVencimento: item.diaVencimento,
    observacoes: item.observacoes ?? '',
    itens: [...item.itens]
      .sort((a, b) => a.ordem - b.ordem)
      .map((itemOrcamento, index) => ({
        itemCatalogoOrcamentoId: itemOrcamento.itemCatalogoOrcamentoId ?? undefined,
        descricao: itemOrcamento.descricao,
        secao: secaoPorNome[itemOrcamento.secao] ?? 2,
        tipo: tipoPorNome[itemOrcamento.tipo] ?? 2,
        quantidade: itemOrcamento.quantidade,
        valorUnitario: itemOrcamento.valorUnitario,
        ordem: index + 1,
      })),
  }
  formError.value = ''
  detailOpen.value = false
  createOpen.value = true
}

function adicionarItem() {
  form.value.itens.push({ descricao: '', secao: 2, tipo: 2, quantidade: 1, valorUnitario: 0, ordem: form.value.itens.length + 1 })
}

function removerItem(index: number) {
  if (form.value.itens.length === 1) return
  form.value.itens.splice(index, 1)
  form.value.itens.forEach((item, ordem) => { item.ordem = ordem + 1 })
}

async function salvar() {
  formError.value = ''
  if (!form.value.titulo.trim()) { formError.value = 'Informe o título do orçamento.'; return }
  if (form.value.diaVencimento < 1 || form.value.diaVencimento > 31) { formError.value = 'O dia de vencimento deve estar entre 1 e 31.'; return }
  if (form.value.itens.some((item) => !item.descricao.trim() || item.quantidade < 1 || item.valorUnitario < 0)) {
    formError.value = 'Preencha a descrição, quantidade e valor de todos os itens.'
    return
  }
  saving.value = true
  try {
    const payload = {
      titulo: form.value.titulo.trim(),
      diaVencimento: form.value.diaVencimento,
      observacoes: form.value.observacoes?.trim() || undefined,
      itens: form.value.itens,
    }
    if (editingId.value) await atualizarOrcamento(editingId.value, payload)
    else await criarOrcamento({ ...payload, prospectoId: prospectoId.value })
    createOpen.value = false
    success.value = editingId.value ? 'Orçamento atualizado com sucesso.' : 'Orçamento criado com sucesso.'
    editingId.value = ''
    await load()
  } catch (cause) { formError.value = normalizeError(cause).message }
  finally { saving.value = false }
}

async function aceitar(item: Orcamento) {
  if (actionId.value) return
  actionId.value = item.id
  error.value = ''
  try { await aceitarOrcamento(item.id); detailOpen.value = false; success.value = 'Orçamento aceito.'; await load() }
  catch (cause) { error.value = normalizeError(cause).message }
  finally { actionId.value = '' }
}

function abrirConversao(item: Orcamento) {
  selected.value = item
  formError.value = ''
  conversion.value = { dataInicio: hojeLocal(), dataFim: '', renovacaoAutomatica: true, clausulasExtras: '' }
  detailOpen.value = false
  convertOpen.value = true
}

function podeEditar(status: string) {
  return status === 'Rascunho' || status === 'EmRevisao'
}

function podeAceitar(status: string) {
  return !['Aceito', 'Recusado', 'Expirado'].includes(status)
}

async function converter() {
  if (!selected.value) return
  if (!conversion.value.dataInicio) { formError.value = 'Informe a data de início.'; return }
  if (conversion.value.dataFim && conversion.value.dataFim < conversion.value.dataInicio) { formError.value = 'A data final não pode ser anterior à data de início.'; return }
  saving.value = true
  formError.value = ''
  try {
    await converterOrcamento(selected.value.id, {
      dataInicio: conversion.value.dataInicio,
      dataFim: conversion.value.dataFim || undefined,
      renovacaoAutomatica: conversion.value.renovacaoAutomatica,
      clausulasExtras: conversion.value.clausulasExtras.trim() || undefined,
    })
    convertOpen.value = false
    await router.push({ name: 'contratos' })
  } catch (cause) { formError.value = normalizeError(cause).message }
  finally { saving.value = false }
}

function severity(status: string) {
  if (status === 'Aceito') return 'success'
  if (status === 'Recusado' || status === 'Expirado') return 'danger'
  if (status === 'Enviado' || status === 'ProntoParaEnvio') return 'info'
  return 'secondary'
}

watch(prospectoId, () => void load(), { immediate: true })
onBeforeUnmount(() => controller?.abort())
</script>

<template>
  <PageHeader title="Orçamentos" :description="`Propostas comerciais de ${prospectoNome}.`">
    <div class="page-actions"><BaseButton label="Voltar" icon="pi pi-arrow-left" variant="text" @click="router.push('/prospectos')" /><BaseButton label="Novo orçamento" icon="pi pi-plus" @click="abrirCriacao" /></div>
  </PageHeader>
  <Message v-if="success" severity="success" :closable="true" class="page-message" @close="success = ''">{{ success }}</Message>
  <Message v-if="error" severity="error" :closable="false" class="page-message">{{ error }}</Message>
  <BaseCard class="data-card">
    <div class="list-heading"><h2>Propostas</h2><span>{{ loading ? 'Carregando…' : `${orcamentos.length} ${orcamentos.length === 1 ? 'orçamento' : 'orçamentos'}` }}</span></div>
    <div v-if="loading" class="list-state" aria-busy="true"><i class="pi pi-spinner loading-spinner" /><p>Carregando orçamentos…</p></div>
    <div v-else-if="error" class="list-state"><BaseButton label="Tentar novamente" icon="pi pi-refresh" variant="secondary" @click="load" /></div>
    <DataTable v-else :value="orcamentos" data-key="id" class="work-table" :table-props="{ 'aria-label': 'Orçamentos do prospecto' }">
      <Column field="titulo" header="Proposta"><template #body="{ data }"><div class="company-cell"><strong>{{ data.titulo || 'Sem título' }}</strong><span>{{ data.itens.length }} {{ data.itens.length === 1 ? 'item' : 'itens' }}</span></div></template></Column>
      <Column field="status" header="Status"><template #body="{ data }"><StatusBadge :label="data.status" :severity="severity(data.status)" /></template></Column>
      <Column field="valorTotal" header="Valor"><template #body="{ data }"><span class="numeric">{{ formatCurrency(data.valorTotal) }}</span></template></Column>
      <Column field="diaVencimento" header="Vencimento"><template #body="{ data }">Dia {{ data.diaVencimento }}</template></Column>
      <Column header="Ações"><template #body="{ data }"><div class="row-actions"><BaseButton label="Visualizar" icon="pi pi-eye" variant="text" :loading="detailLoadingId === data.id" :disabled="!!actionId || (!!detailLoadingId && detailLoadingId !== data.id)" @click="abrirDetalhe(data)" /><BaseButton v-if="podeEditar(data.status)" label="Editar" icon="pi pi-pencil" variant="text" :disabled="!!actionId || !!detailLoadingId" @click="abrirEdicao(data)" /><BaseButton v-if="podeAceitar(data.status)" label="Aceitar" icon="pi pi-check" variant="text" :loading="actionId === data.id" :disabled="!!detailLoadingId || (!!actionId && actionId !== data.id)" @click="aceitar(data)" /><BaseButton v-if="data.status === 'Aceito'" label="Gerar contrato" icon="pi pi-file" variant="secondary" :disabled="!!actionId || !!detailLoadingId" @click="abrirConversao(data)" /></div></template></Column>
      <template #empty><div class="empty-state"><i class="pi pi-file-edit" /><h3>Nenhum orçamento criado</h3><p>Crie a primeira proposta comercial para este prospecto.</p><BaseButton label="Novo orçamento" icon="pi pi-plus" @click="abrirCriacao" /></div></template>
    </DataTable>
  </BaseCard>

  <OrcamentoDetalheDialog v-model:visible="detailOpen" :orcamento="selected" :busy="!!actionId" @editar="abrirEdicao" @aceitar="aceitar" @converter="abrirConversao" />

  <Dialog v-model:visible="createOpen" modal :header="editingId ? 'Editar orçamento' : 'Novo orçamento'" class="workflow-dialog" :style="{ width: 'min(760px, 94vw)' }">
    <form id="budget-form" class="dialog-form" @submit.prevent="salvar">
      <Message v-if="formError" severity="error" :closable="false">{{ formError }}</Message>
      <div class="form-grid two-columns"><div class="field"><label for="budget-title">Título</label><InputText id="budget-title" v-model="form.titulo" /></div><div class="field"><label for="budget-due">Dia de vencimento</label><InputNumber input-id="budget-due" v-model="form.diaVencimento" :min="1" :max="31" /></div></div>
      <div class="field"><label for="budget-notes">Observações</label><Textarea id="budget-notes" v-model="form.observacoes" rows="2" auto-resize /></div>
      <div class="form-section-heading"><div><h3>Itens</h3><p>Serviços e valores que compõem a proposta.</p></div><Button label="Adicionar item" icon="pi pi-plus" text type="button" @click="adicionarItem" /></div>
      <div v-for="(item, index) in form.itens" :key="index" class="budget-item-row">
        <div class="field item-description"><label :for="`item-description-${index}`">Descrição</label><InputText :id="`item-description-${index}`" v-model="item.descricao" /></div>
        <div class="field"><label :for="`item-section-${index}`">Seção</label><Select :input-id="`item-section-${index}`" v-model="item.secao" :options="[...secaoOptions]" option-label="label" option-value="value" /></div>
        <div class="field"><label :for="`item-type-${index}`">Tipo</label><Select :input-id="`item-type-${index}`" v-model="item.tipo" :options="[...tipoOptions]" option-label="label" option-value="value" /></div>
        <div class="field quantity-field"><label :for="`item-quantity-${index}`">Qtd.</label><InputNumber :input-id="`item-quantity-${index}`" v-model="item.quantidade" :min="1" /></div>
        <div class="field value-field"><label :for="`item-value-${index}`">Valor</label><InputNumber :input-id="`item-value-${index}`" v-model="item.valorUnitario" mode="currency" currency="BRL" locale="pt-BR" :min="0" /></div>
        <Button icon="pi pi-trash" text severity="danger" type="button" :disabled="form.itens.length === 1" aria-label="Remover item" @click="removerItem(index)" />
      </div>
    </form>
    <template #footer><BaseButton label="Cancelar" variant="text" @click="createOpen = false" /><BaseButton :label="editingId ? 'Salvar alterações' : 'Criar orçamento'" type="submit" form="budget-form" :loading="saving" /></template>
  </Dialog>

  <Dialog v-model:visible="convertOpen" modal header="Gerar contrato" class="workflow-dialog" :style="{ width: 'min(620px, 94vw)' }">
    <form id="contract-form" class="dialog-form" @submit.prevent="converter">
      <Message v-if="formError" severity="error" :closable="false">{{ formError }}</Message>
      <div class="form-grid two-columns"><div class="field"><label for="contract-start">Início</label><InputText id="contract-start" v-model="conversion.dataInicio" type="date" /></div><div class="field"><label for="contract-end">Fim (opcional)</label><InputText id="contract-end" v-model="conversion.dataFim" type="date" /></div></div>
      <label class="check-field"><Checkbox v-model="conversion.renovacaoAutomatica" binary input-id="auto-renew" /><span>Renovação automática</span></label>
      <div class="field"><label for="extra-clauses">Cláusulas extras</label><Textarea id="extra-clauses" v-model="conversion.clausulasExtras" rows="4" auto-resize /></div>
    </form>
    <template #footer><BaseButton label="Cancelar" variant="text" @click="convertOpen = false" /><BaseButton label="Gerar contrato" type="submit" form="contract-form" :loading="saving" /></template>
  </Dialog>
</template>
