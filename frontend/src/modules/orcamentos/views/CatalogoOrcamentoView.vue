<script setup lang="ts">
import { computed, onBeforeUnmount, ref, watch } from 'vue'
import axios from 'axios'
import Button from 'primevue/button'
import Column from 'primevue/column'
import DataTable from 'primevue/datatable'
import Dialog from 'primevue/dialog'
import InputNumber from 'primevue/inputnumber'
import InputText from 'primevue/inputtext'
import Message from 'primevue/message'
import Select from 'primevue/select'
import BaseButton from '../../../components/base/BaseButton.vue'
import BaseCard from '../../../components/base/BaseCard.vue'
import BaseInput from '../../../components/base/BaseInput.vue'
import PageHeader from '../../../components/layout/PageHeader.vue'
import StatusBadge from '../../../components/shared/StatusBadge.vue'
import { normalizeError } from '../../../api/errors'
import { formatCurrency } from '../../../utils/format'
import {
  alterarAtivoItemCatalogoOrcamento,
  atualizarItemCatalogoOrcamento,
  criarItemCatalogoOrcamento,
  listarCatalogoOrcamento,
} from '../services/orcamentos.service'
import {
  naturezaCobrancaOptions,
  naturezaCobrancaPorNome,
  secaoOptions,
  secaoPorNome,
  tipoOptions,
  tipoPorNome,
  type ItemCatalogoOrcamento,
  type ItemCatalogoOrcamentoPayload,
} from '../types/orcamento'

interface FormularioCatalogo {
  descricao: string
  secao: number
  tipo: number
  naturezaCobranca: number
  valorReferencia: number | null
}

const itens = ref<ItemCatalogoOrcamento[]>([])
const loading = ref(true)
const error = ref('')
const success = ref('')
const search = ref('')
const incluirInativos = ref(false)
const dialogOpen = ref(false)
const confirmOpen = ref(false)
const editingId = ref('')
const selected = ref<ItemCatalogoOrcamento | null>(null)
const formError = ref('')
const saving = ref(false)
const changingStatus = ref(false)
const form = ref<FormularioCatalogo>(novoFormulario())
let controller: AbortController | undefined

const itensFiltrados = computed(() => {
  const termo = search.value.trim().toLocaleLowerCase('pt-BR')
  if (!termo) return itens.value
  return itens.value.filter((item) =>
    `${item.descricao} ${labelSecao(item.secao)} ${labelTipo(item.tipo)}`.toLocaleLowerCase('pt-BR').includes(termo),
  )
})
const totalAtivos = computed(() => itens.value.filter((item) => item.ativo).length)

function novoFormulario(): FormularioCatalogo {
  return { descricao: '', secao: 1, tipo: 1, naturezaCobranca: 1, valorReferencia: null }
}

function labelSecao(value: string) {
  return secaoOptions.find((option) => secaoPorNome[value] === option.value)?.label ?? value
}

function labelTipo(value: string) {
  return tipoOptions.find((option) => tipoPorNome[value] === option.value)?.label ?? value
}

function labelCobranca(value: string) {
  return naturezaCobrancaOptions.find((option) => naturezaCobrancaPorNome[value] === option.value)?.label ?? value
}

function abrirCriacao() {
  editingId.value = ''
  form.value = novoFormulario()
  formError.value = ''
  dialogOpen.value = true
}

function abrirEdicao(item: ItemCatalogoOrcamento) {
  editingId.value = item.id
  form.value = {
    descricao: item.descricao,
    secao: secaoPorNome[item.secao] ?? 1,
    tipo: tipoPorNome[item.tipo] ?? 1,
    naturezaCobranca: naturezaCobrancaPorNome[item.naturezaCobranca] ?? 1,
    valorReferencia: item.valorReferencia,
  }
  formError.value = ''
  dialogOpen.value = true
}

function tipoAlterado(value: number) {
  form.value.tipo = value
  if (value !== tipoPorNome.ItemMonetario) {
    form.value.naturezaCobranca = naturezaCobrancaPorNome.NaoRecorrente
    form.value.valorReferencia = null
  } else if (form.value.valorReferencia === null) {
    form.value.valorReferencia = 0
  }
}

function payloadFormulario(): ItemCatalogoOrcamentoPayload {
  return {
    descricao: form.value.descricao.trim(),
    secao: form.value.secao,
    tipo: form.value.tipo,
    naturezaCobranca: form.value.tipo === tipoPorNome.ItemMonetario
      ? form.value.naturezaCobranca
      : naturezaCobrancaPorNome.NaoRecorrente,
    valorReferencia: form.value.tipo === tipoPorNome.ItemMonetario ? form.value.valorReferencia : null,
  }
}

async function salvar() {
  formError.value = ''
  if (!form.value.descricao.trim()) { formError.value = 'Informe a descrição do item.'; return }
  if (form.value.descricao.trim().length > 300) { formError.value = 'A descrição pode ter no máximo 300 caracteres.'; return }
  if (form.value.tipo === tipoPorNome.ItemMonetario && (form.value.valorReferencia === null || form.value.valorReferencia < 0)) {
    formError.value = 'Informe um preço de referência igual ou maior que zero.'
    return
  }

  saving.value = true
  try {
    const payload = payloadFormulario()
    if (editingId.value) await atualizarItemCatalogoOrcamento(editingId.value, payload)
    else await criarItemCatalogoOrcamento(payload)
    dialogOpen.value = false
    success.value = editingId.value ? 'Item do catálogo atualizado.' : 'Item adicionado ao catálogo.'
    await load()
  } catch (cause) {
    formError.value = normalizeError(cause).message
  } finally {
    saving.value = false
  }
}

function pedirAlteracaoStatus(item: ItemCatalogoOrcamento) {
  selected.value = item
  confirmOpen.value = true
}

async function confirmarAlteracaoStatus() {
  if (!selected.value || changingStatus.value) return
  const item = selected.value
  changingStatus.value = true
  error.value = ''
  try {
    await alterarAtivoItemCatalogoOrcamento(item.id, !item.ativo)
    success.value = item.ativo
      ? `${item.descricao} foi desativado. Orçamentos existentes permanecem inalterados.`
      : `${item.descricao} foi reativado.`
    confirmOpen.value = false
    await load()
  } catch (cause) {
    error.value = normalizeError(cause).message
  } finally {
    changingStatus.value = false
  }
}

async function load() {
  controller?.abort()
  const currentController = new AbortController()
  controller = currentController
  loading.value = true
  error.value = ''
  try {
    itens.value = await listarCatalogoOrcamento(incluirInativos.value, currentController.signal)
  } catch (cause) {
    if (!axios.isCancel(cause)) error.value = normalizeError(cause).message
  } finally {
    if (controller === currentController) loading.value = false
  }
}

watch(incluirInativos, () => { void load() })
void load()
onBeforeUnmount(() => controller?.abort())
</script>

<template>
  <PageHeader title="Itens do orçamento" description="Cadastre os serviços e valores padrão usados nas propostas.">
    <BaseButton label="Cadastrar item" icon="pi pi-plus" @click="abrirCriacao" />
  </PageHeader>

  <Message v-if="success" severity="success" closable class="page-message" @close="success = ''">{{ success }}</Message>
  <Message v-if="error" severity="error" :closable="false" class="page-message">{{ error }}</Message>

  <BaseCard class="data-card">
    <div class="list-heading">
      <div><h2>Catálogo de serviços</h2><span>{{ totalAtivos }} ativos<span v-if="incluirInativos"> · {{ itens.length - totalAtivos }} inativos</span></span></div>
      <label class="catalog-toggle"><input v-model="incluirInativos" type="checkbox" /> Mostrar inativos</label>
    </div>
    <div class="catalog-toolbar" role="search" aria-label="Filtrar itens do catálogo">
      <div class="catalog-search-field">
        <BaseInput id="catalog-search" v-model="search" label="Buscar item" placeholder="Descrição, seção ou tipo" autocomplete="off" />
      </div>
      <Button icon="pi pi-refresh" text rounded aria-label="Atualizar catálogo" title="Atualizar catálogo" :disabled="loading" @click="load" />
    </div>

    <div v-if="loading" class="list-state" aria-busy="true"><i class="pi pi-spinner loading-spinner" /><p>Carregando catálogo...</p></div>
    <div v-else-if="error" class="list-state"><BaseButton label="Tentar novamente" icon="pi pi-refresh" variant="secondary" @click="load" /></div>
    <DataTable v-else :value="itensFiltrados" data-key="id" class="work-table catalog-table" :table-props="{ 'aria-label': 'Itens do catálogo de orçamento' }">
      <Column field="descricao" header="Serviço">
        <template #body="{ data }"><div class="catalog-service"><strong>{{ data.descricao }}</strong><span>{{ labelSecao(data.secao) }}</span></div></template>
      </Column>
      <Column field="tipo" header="Tipo"><template #body="{ data }">{{ labelTipo(data.tipo) }}</template></Column>
      <Column field="naturezaCobranca" header="Cobrança"><template #body="{ data }">{{ labelCobranca(data.naturezaCobranca) }}</template></Column>
      <Column field="valorReferencia" header="Preço padrão">
        <template #body="{ data }"><span class="numeric">{{ data.valorReferencia === null ? 'Sem preço' : formatCurrency(data.valorReferencia) }}</span></template>
      </Column>
      <Column field="ativo" header="Situação"><template #body="{ data }"><StatusBadge :label="data.ativo ? 'Ativo' : 'Inativo'" :severity="data.ativo ? 'success' : 'secondary'" /></template></Column>
      <Column header="Ações"><template #body="{ data }"><div class="catalog-actions">
        <BaseButton label="Editar" icon="pi pi-pencil" variant="text" :disabled="changingStatus" @click="abrirEdicao(data)" />
        <BaseButton :label="data.ativo ? 'Desativar' : 'Reativar'" :icon="data.ativo ? 'pi pi-ban' : 'pi pi-check'" variant="text" :disabled="changingStatus" @click="pedirAlteracaoStatus(data)" />
      </div></template></Column>
      <template #empty><div class="empty-state"><i class="pi pi-list" /><h3>{{ search ? 'Nenhum item encontrado' : 'Catálogo vazio' }}</h3><p>{{ search ? 'Ajuste a busca ou limpe o filtro.' : 'Cadastre os serviços usados nas propostas.' }}</p><BaseButton v-if="!search" label="Cadastrar primeiro item" icon="pi pi-plus" @click="abrirCriacao" /></div></template>
    </DataTable>
  </BaseCard>

  <Dialog v-model:visible="dialogOpen" modal :header="editingId ? 'Editar item do catálogo' : 'Cadastrar item do catálogo'" class="workflow-dialog" :style="{ width: 'min(560px, 94vw)' }">
    <form id="catalog-item-form" class="dialog-form" @submit.prevent="salvar">
      <Message v-if="formError" severity="error" :closable="false">{{ formError }}</Message>
      <div class="field"><label for="catalog-description">Descrição</label><InputText id="catalog-description" v-model="form.descricao" maxlength="300" required autocomplete="off" /></div>
      <div class="form-grid two-columns">
        <div class="field"><label for="catalog-section">Seção</label><Select input-id="catalog-section" v-model="form.secao" :options="[...secaoOptions]" option-label="label" option-value="value" /></div>
        <div class="field"><label for="catalog-type">Tipo</label><Select input-id="catalog-type" :model-value="form.tipo" :options="[...tipoOptions]" option-label="label" option-value="value" @update:model-value="tipoAlterado" /></div>
      </div>
      <div v-if="form.tipo === tipoPorNome.ItemMonetario" class="form-grid two-columns">
        <div class="field"><label for="catalog-billing">Cobrança</label><Select input-id="catalog-billing" v-model="form.naturezaCobranca" :options="[...naturezaCobrancaOptions]" option-label="label" option-value="value" /></div>
        <div class="field"><label for="catalog-price">Preço de referência</label><InputNumber input-id="catalog-price" v-model="form.valorReferencia" mode="currency" currency="BRL" locale="pt-BR" :min="0" /></div>
      </div>
      <p v-else class="form-note">Itens não monetários não recebem cobrança recorrente nem preço padrão.</p>
    </form>
    <template #footer><BaseButton label="Cancelar" variant="text" :disabled="saving" @click="dialogOpen = false" /><BaseButton :label="editingId ? 'Salvar alterações' : 'Cadastrar item'" icon="pi pi-check" type="submit" form="catalog-item-form" :loading="saving" /></template>
  </Dialog>

  <Dialog v-model:visible="confirmOpen" modal :header="selected?.ativo ? 'Desativar item' : 'Reativar item'" class="workflow-dialog" :style="{ width: 'min(460px, 94vw)' }">
    <p v-if="selected" class="confirmation-copy">{{ selected.ativo ? `${selected.descricao} deixará de aparecer em novos orçamentos. Os orçamentos já criados não serão alterados.` : `${selected.descricao} voltará a aparecer para seleção em novos orçamentos.` }}</p>
    <template #footer><BaseButton label="Cancelar" variant="text" :disabled="changingStatus" @click="confirmOpen = false" /><BaseButton :label="selected?.ativo ? 'Desativar item' : 'Reativar item'" :icon="selected?.ativo ? 'pi pi-ban' : 'pi pi-check'" :loading="changingStatus" @click="confirmarAlteracaoStatus" /></template>
  </Dialog>
</template>

<style scoped>
.catalog-toolbar { display: flex; flex-wrap: wrap; align-items: flex-end; gap: 8px; padding: 0 24px 16px; }
.catalog-search-field { flex: 0 1 320px; width: min(320px, 100%); }
.catalog-toggle { display: inline-flex; align-items: center; gap: 8px; color: var(--officio-muted); font-size: 12px; white-space: nowrap; }
.catalog-toggle input { width: 16px; height: 16px; accent-color: var(--officio-green); }
.catalog-service { display: grid; gap: 4px; }
.catalog-service span { color: var(--officio-muted); font-size: 12px; }
.catalog-actions { display: flex; flex-wrap: wrap; gap: 4px; }
.form-note { margin: 0; color: var(--officio-muted); font-size: 13px; }

@media (max-width: 700px) {
  .list-heading { align-items: flex-start; flex-direction: column; }
  .catalog-toolbar { padding-inline: 16px; }
  .catalog-toolbar > :deep(.p-button) { margin-left: auto; }
  .catalog-actions { align-items: flex-start; flex-direction: column; }
}
</style>
