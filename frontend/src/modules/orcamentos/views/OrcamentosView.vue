<script setup lang="ts">
import { computed, onBeforeUnmount, ref, watch } from 'vue'
import { RouterLink, useRoute, useRouter } from 'vue-router'
import axios from 'axios'
import Button from 'primevue/button'
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
import { formatCurrency, formatDate } from '../../../utils/format'
import { aceitarOrcamento, atualizarOrcamento, converterOrcamento, criarOrcamento, excluirOrcamento, listarCatalogoOrcamento, listarContratosAtivosParaConversao, listarOrcamentos, obterOrcamento } from '../services/orcamentos.service'
import { naturezaCobrancaOptions, naturezaCobrancaPorNome, secaoOptions, secaoPorNome, tipoOptions, tipoPorNome, type ContratoAtivoResumo, type CriarOrcamentoPayload, type ItemCatalogoOrcamento, type NovoItemOrcamento, type Orcamento } from '../types/orcamento'

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
const confirmationOpen = ref(false)
const deleteOpen = ref(false)
const selected = ref<Orcamento | null>(null)
const activeContracts = ref<ContratoAtivoResumo[]>([])
const deleteTarget = ref<Orcamento | null>(null)
const editingId = ref('')
const saving = ref(false)
const formError = ref('')
const form = ref(novoFormulario())
const catalogo = ref<ItemCatalogoOrcamento[]>([])
const catalogLoading = ref(false)
const catalogError = ref('')
const catalogoAtivoCount = computed(() => catalogo.value.filter((item) => item.ativo).length)
const conversion = ref({ dataInicio: hojeLocal(), dataFim: '', clausulasExtras: '' })
let controller: AbortController | undefined
let catalogController: AbortController | undefined

function novoFormulario(): CriarOrcamentoPayload {
  return {
    prospectoId: prospectoId.value,
    titulo: '',
    diaVencimento: 10,
    observacoes: '',
    itens: [{ descricao: '', secao: 2, tipo: 2, naturezaCobranca: 2, quantidade: 1, valorUnitario: 0, ordem: 1 }],
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
  void carregarCatalogo(false)
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
        itemOrcamentoId: itemOrcamento.id,
        itemCatalogoOrcamentoId: itemOrcamento.itemCatalogoOrcamentoId ?? undefined,
        descricao: itemOrcamento.descricao,
        secao: secaoPorNome[itemOrcamento.secao] ?? 2,
        tipo: tipoPorNome[itemOrcamento.tipo] ?? 2,
        naturezaCobranca: naturezaCobrancaPorNome[itemOrcamento.naturezaCobranca] ?? 1,
        quantidade: itemOrcamento.quantidade,
        valorUnitario: itemOrcamento.valorUnitario,
        ordem: index + 1,
      })),
  }
  formError.value = ''
  detailOpen.value = false
  createOpen.value = true
  void carregarCatalogo(true)
}

function adicionarItem() {
  form.value.itens.push({ descricao: '', secao: 2, tipo: 2, naturezaCobranca: 2, quantidade: 1, valorUnitario: 0, ordem: form.value.itens.length + 1 })
}

async function carregarCatalogo(incluirInativos: boolean) {
  catalogController?.abort()
  const currentController = new AbortController()
  catalogController = currentController
  catalogLoading.value = true
  catalogError.value = ''
  try {
    catalogo.value = await listarCatalogoOrcamento(incluirInativos, currentController.signal)
  } catch (cause) {
    if (!axios.isCancel(cause)) catalogError.value = normalizeError(cause).message
  } finally {
    if (catalogController === currentController) catalogLoading.value = false
  }
}

function opcoesCatalogo(item: NovoItemOrcamento) {
  return catalogo.value
    .filter((catalogItem) => catalogItem.ativo || catalogItem.id === item.itemCatalogoOrcamentoId)
    .map((catalogItem) => ({ ...catalogItem, desativado: !catalogItem.ativo }))
}

function selecionarItemCatalogo(item: NovoItemOrcamento, id: string) {
  if (id === item.itemCatalogoOrcamentoId) return
  const catalogItem = catalogo.value.find((entry) => entry.id === id && entry.ativo)
  if (!catalogItem) return
  item.itemOrcamentoId = undefined
  item.itemCatalogoOrcamentoId = catalogItem.id
  item.descricao = catalogItem.descricao
  item.secao = secaoPorNome[catalogItem.secao] ?? 1
  item.tipo = tipoPorNome[catalogItem.tipo] ?? 1
  item.naturezaCobranca = naturezaCobrancaPorNome[catalogItem.naturezaCobranca] ?? 1
  item.valorUnitario = catalogItem.valorReferencia ?? 0
}

function precoReferencia(item: NovoItemOrcamento): number | null {
  return catalogo.value.find((catalogItem) => catalogItem.id === item.itemCatalogoOrcamentoId)?.valorReferencia ?? null
}

function labelOpcao(options: readonly { label: string; value: number }[], value: number) {
  return options.find((option) => option.value === value)?.label ?? ''
}

function removerItem(index: number) {
  if (form.value.itens.length === 1) return
  form.value.itens.splice(index, 1)
  form.value.itens.forEach((item, ordem) => { item.ordem = ordem + 1 })
}

async function salvar() {
  formError.value = ''
  if (form.value.itens.some((item) => !item.itemCatalogoOrcamentoId && !item.itemOrcamentoId)) {
    formError.value = 'Selecione um serviço do catálogo para cada novo item.'
    return
  }
  const catalogIds = form.value.itens.map((item) => item.itemCatalogoOrcamentoId).filter((id): id is string => !!id)
  if (new Set(catalogIds).size !== catalogIds.length) {
    formError.value = 'Cada serviço pode ser adicionado uma vez. Ajuste a quantidade para incluir mais unidades.'
    return
  }
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

function solicitarExclusao(item: Orcamento) {
  if (item.status !== 'Rascunho' || actionId.value || detailLoadingId.value) return
  deleteTarget.value = item
  deleteOpen.value = true
}

async function confirmarExclusao() {
  if (!deleteTarget.value || actionId.value) return
  const item = deleteTarget.value
  actionId.value = item.id
  error.value = ''
  try {
    await excluirOrcamento(item.id)
    deleteOpen.value = false
    deleteTarget.value = null
    success.value = 'Orçamento em rascunho excluído.'
    await load()
  } catch (cause) {
    error.value = normalizeError(cause).message
  } finally {
    actionId.value = ''
  }
}

function abrirConversao(item: Orcamento) {
  selected.value = item
  formError.value = ''
  conversion.value = { dataInicio: hojeLocal(), dataFim: '', clausulasExtras: '' }
  activeContracts.value = []
  detailOpen.value = false
  convertOpen.value = true
}

function podeEditar(status: string) {
  return status === 'Rascunho' || status === 'EmRevisao'
}

function podeAceitar(status: string) {
  return !['Aceito', 'Recusado', 'Expirado'].includes(status)
}

async function converter(confirmarSubstituicaoContratoAtivo = false) {
  const orcamentoSelecionado = selected.value
  if (!orcamentoSelecionado) return
  if (!conversion.value.dataInicio) { formError.value = 'Informe a data de início.'; return }
  if (conversion.value.dataFim && conversion.value.dataFim < conversion.value.dataInicio) { formError.value = 'A data final não pode ser anterior à data de início.'; return }
  saving.value = true
  formError.value = ''
  try {
    if (!confirmarSubstituicaoContratoAtivo) {
      activeContracts.value = await listarContratosAtivosParaConversao(orcamentoSelecionado.id)
      if (activeContracts.value.length > 0) {
        convertOpen.value = false
        confirmationOpen.value = true
        return
      }
    }

    await converterOrcamento(orcamentoSelecionado.id, {
      dataInicio: conversion.value.dataInicio,
      dataFim: conversion.value.dataFim || undefined,
      confirmarSubstituicaoContratoAtivo,
      clausulasExtras: conversion.value.clausulasExtras.trim() || undefined,
    })
    confirmationOpen.value = false
    convertOpen.value = false
    await router.push({ name: 'contratos' })
  } catch (cause) {
    if (!confirmarSubstituicaoContratoAtivo && axios.isAxiosError(cause) && cause.response?.status === 409) {
      try {
        activeContracts.value = await listarContratosAtivosParaConversao(orcamentoSelecionado.id)
        if (activeContracts.value.length > 0) {
          convertOpen.value = false
          confirmationOpen.value = true
          return
        }
      } catch {
        // Preserve the original conversion error if the follow-up lookup also fails.
      }
    }
    formError.value = normalizeError(cause).message
  }
  finally { saving.value = false }
}

function cancelarConfirmacaoSubstituicao() {
  confirmationOpen.value = false
  convertOpen.value = true
}

function severity(status: string) {
  if (status === 'Aceito') return 'success'
  if (status === 'Recusado' || status === 'Expirado') return 'danger'
  if (status === 'Enviado' || status === 'ProntoParaEnvio') return 'info'
  return 'secondary'
}

watch(prospectoId, () => void load(), { immediate: true })
onBeforeUnmount(() => { controller?.abort(); catalogController?.abort() })
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
      <Column header="Ações"><template #body="{ data }"><div class="budget-row-actions"><BaseButton label="Visualizar" icon="pi pi-eye" variant="text" :loading="detailLoadingId === data.id" :disabled="!!actionId || (!!detailLoadingId && detailLoadingId !== data.id)" @click="abrirDetalhe(data)" /><BaseButton v-if="podeEditar(data.status)" label="Editar" icon="pi pi-pencil" variant="text" :disabled="!!actionId || !!detailLoadingId" @click="abrirEdicao(data)" /><BaseButton v-if="data.status === 'Rascunho'" label="Excluir" icon="pi pi-trash" variant="text" :loading="actionId === data.id" :disabled="!!detailLoadingId || (!!actionId && actionId !== data.id)" @click="solicitarExclusao(data)" /><BaseButton v-if="podeAceitar(data.status)" label="Aceitar" icon="pi pi-check" variant="text" :loading="actionId === data.id" :disabled="!!detailLoadingId || (!!actionId && actionId !== data.id)" @click="aceitar(data)" /><BaseButton v-if="data.status === 'Aceito'" label="Gerar contrato" icon="pi pi-file" variant="text" :disabled="!!actionId || !!detailLoadingId" @click="abrirConversao(data)" /></div></template></Column>
      <template #empty><div class="empty-state"><i class="pi pi-file-edit" /><h3>Nenhum orçamento criado</h3><p>Crie a primeira proposta comercial para este prospecto.</p><BaseButton label="Novo orçamento" icon="pi pi-plus" @click="abrirCriacao" /></div></template>
    </DataTable>
  </BaseCard>

  <OrcamentoDetalheDialog v-model:visible="detailOpen" :orcamento="selected" :busy="!!actionId" @editar="abrirEdicao" @aceitar="aceitar" @converter="abrirConversao" />

  <Dialog v-model:visible="deleteOpen" modal header="Excluir orçamento em rascunho" class="workflow-dialog" :style="{ width: 'min(480px, 94vw)' }">
    <p class="confirmation-copy">O orçamento <strong>{{ deleteTarget?.titulo || 'sem título' }}</strong> e seus itens serão excluídos permanentemente. Apenas orçamentos em rascunho podem ser removidos.</p>
    <template #footer><BaseButton label="Manter orçamento" variant="text" :disabled="!!actionId" @click="deleteOpen = false" /><Button label="Excluir rascunho" icon="pi pi-trash" severity="danger" :loading="actionId === deleteTarget?.id" @click="confirmarExclusao" /></template>
  </Dialog>

  <Dialog v-model:visible="createOpen" modal :header="editingId ? 'Editar orçamento' : 'Novo orçamento'" class="workflow-dialog" :style="{ width: 'min(760px, 94vw)' }">
    <form id="budget-form" class="dialog-form" @submit.prevent="salvar">
      <Message v-if="formError" severity="error" :closable="false">{{ formError }}</Message>
      <div class="form-grid two-columns"><div class="field"><label for="budget-title">Título</label><InputText id="budget-title" v-model="form.titulo" /></div><div class="field"><label for="budget-due">Dia de vencimento</label><InputNumber input-id="budget-due" v-model="form.diaVencimento" :min="1" :max="31" /></div></div>
      <div class="field"><label for="budget-notes">Observações</label><Textarea id="budget-notes" v-model="form.observacoes" rows="2" auto-resize /></div>
      <div class="form-section-heading"><div><h3>Itens</h3><p>Serviços e valores que compõem a proposta.</p></div><Button label="Adicionar item" icon="pi pi-plus" text type="button" :disabled="catalogLoading || catalogoAtivoCount === 0" @click="adicionarItem" /></div>
      <Message v-if="catalogError" severity="error" :closable="false">{{ catalogError }}</Message>
      <Message v-else-if="!catalogLoading && catalogoAtivoCount === 0" severity="info" :closable="false">
        O catálogo não tem serviços ativos. <RouterLink :to="{ name: 'catalogo-orcamento' }">Cadastrar serviços</RouterLink>
      </Message>
      <div v-for="(item, index) in form.itens" :key="item.itemOrcamentoId ?? item.itemCatalogoOrcamentoId ?? index" class="budget-item-row catalog-budget-row">
        <div v-if="item.itemOrcamentoId && !item.itemCatalogoOrcamentoId" class="legacy-budget-item">
          <span>Item histórico</span><strong>{{ item.descricao }}</strong>
          <small>{{ labelOpcao(secaoOptions, item.secao) }} | {{ labelOpcao(tipoOptions, item.tipo) }} | sem vínculo ao catálogo</small>
        </div>
        <div v-else class="field item-picker">
          <label :for="`item-catalog-${index}`">Serviço</label>
          <Select :input-id="`item-catalog-${index}`" :model-value="item.itemCatalogoOrcamentoId" :options="opcoesCatalogo(item)" option-label="descricao" option-value="id" option-disabled="desativado" :loading="catalogLoading" placeholder="Selecione um serviço" @update:model-value="selecionarItemCatalogo(item, $event)">
            <template #option="{ option }"><span>{{ option.descricao }} <small v-if="!option.ativo">(inativo)</small></span></template>
          </Select>
          <small v-if="item.itemCatalogoOrcamentoId" class="catalog-item-summary">{{ labelOpcao(secaoOptions, item.secao) }} | {{ labelOpcao(tipoOptions, item.tipo) }} | {{ labelOpcao(naturezaCobrancaOptions, item.naturezaCobranca) }}</small>
        </div>
        <div class="field quantity-field"><label :for="`item-quantity-catalog-${index}`">Qtd.</label><InputNumber :input-id="`item-quantity-catalog-${index}`" v-model="item.quantidade" :min="1" /></div>
        <div v-if="item.tipo === tipoPorNome.ItemMonetario" class="field catalog-price">
          <label :for="`item-price-catalog-${index}`">Preço unitário</label>
          <InputNumber :input-id="`item-price-catalog-${index}`" v-model="item.valorUnitario" mode="currency" currency="BRL" locale="pt-BR" :min="0" :max-fraction-digits="2" />
          <small v-if="precoReferencia(item) !== null" class="catalog-price-reference">Referência: {{ formatCurrency(precoReferencia(item) ?? 0) }}</small>
        </div>
        <div v-else class="catalog-price"><span>Preço unitário</span><strong>Não monetário</strong></div>
        <Button icon="pi pi-trash" text severity="danger" type="button" :disabled="form.itens.length === 1" aria-label="Remover item" @click="removerItem(index)" />
      </div>
    </form>
    <template #footer><BaseButton label="Cancelar" variant="text" @click="createOpen = false" /><BaseButton :label="editingId ? 'Salvar alterações' : 'Criar orçamento'" type="submit" form="budget-form" :loading="saving" /></template>
  </Dialog>

  <Dialog v-model:visible="convertOpen" modal header="Gerar contrato" class="workflow-dialog" :style="{ width: 'min(620px, 94vw)' }">
    <form id="contract-form" class="dialog-form" @submit.prevent="converter()">
      <Message v-if="formError" severity="error" :closable="false">{{ formError }}</Message>
      <div class="form-grid two-columns"><div class="field"><label for="contract-start">Início</label><InputText id="contract-start" v-model="conversion.dataInicio" type="date" /></div><div class="field"><label for="contract-end">Fim (opcional)</label><InputText id="contract-end" v-model="conversion.dataFim" type="date" /></div></div>
      <div class="field"><label for="extra-clauses">Cláusulas extras</label><Textarea id="extra-clauses" v-model="conversion.clausulasExtras" rows="4" auto-resize /></div>
    </form>
    <template #footer><BaseButton label="Cancelar" variant="text" @click="convertOpen = false" /><BaseButton label="Gerar contrato" type="submit" form="contract-form" :loading="saving" /></template>
  </Dialog>

  <Dialog v-model:visible="confirmationOpen" modal header="Contrato ativo encontrado" class="workflow-dialog" :style="{ width: 'min(540px, 94vw)' }" :closable="false" :close-on-escape="false">
    <p class="confirmation-copy">Esta empresa já possui contrato ativo. O contrato atual continuará ativo enquanto o novo estiver sendo gerado e assinado. Após a conclusão da nova assinatura, os contratos atuais serão encerrados.</p>
    <ul class="active-contract-list">
      <li v-for="contract in activeContracts" :key="contract.id">
        <strong>{{ contract.numero }}</strong>
        <span>Vigência: {{ formatDate(contract.dataInicio) }} até {{ contract.dataFim ? formatDate(contract.dataFim) : 'prazo indeterminado' }}</span>
      </li>
    </ul>
    <Message v-if="formError" severity="error" :closable="false">{{ formError }}</Message>
    <template #footer>
      <BaseButton label="Voltar" variant="text" :disabled="saving" @click="cancelarConfirmacaoSubstituicao" />
      <BaseButton label="Gerar contrato" icon="pi pi-file" :loading="saving" @click="converter(true)" />
    </template>
  </Dialog>
</template>

<style scoped>
.catalog-budget-row {
  grid-template-columns: minmax(0, 1fr) minmax(84px, 110px) minmax(145px, 170px) 36px;
  align-items: start;
}

.budget-row-actions { display: flex; flex-direction: column; align-items: flex-start; gap: 2px; }
.budget-row-actions :deep(.base-button.p-button) { min-height: 32px; padding: 4px 8px; font-size: 13px; font-weight: 600; }

.item-picker { min-width: 0; }
.catalog-item-summary,
.legacy-budget-item small,
.legacy-budget-item > span,
.catalog-price > span { color: var(--officio-muted); font-size: 12px; }
.catalog-item-summary { display: block; margin-top: 6px; }
.legacy-budget-item,
.catalog-price { display: grid; gap: 6px; min-width: 0; }
.legacy-budget-item strong { overflow-wrap: anywhere; }
.catalog-price-reference { font-size: 11px; }
.active-contract-list { display: grid; gap: 10px; margin: 0; padding-left: 20px; }
.active-contract-list li { display: grid; gap: 3px; }
.active-contract-list span { color: var(--officio-muted); font-size: 13px; }
.catalog-price > .p-inputnumber,
.catalog-price :deep(.p-inputnumber-input) { width: 100%; min-width: 0; }

@media (max-width: 700px) {
  .catalog-budget-row { grid-template-columns: minmax(0, 1fr); }
  .catalog-budget-row > button { justify-self: end; }
}
</style>
