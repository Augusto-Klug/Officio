<script setup lang="ts">
import { computed, onBeforeUnmount, ref, watch } from 'vue'
import { useRouter } from 'vue-router'
import axios from 'axios'
import Column from 'primevue/column'
import DataTable from 'primevue/datatable'
import Dialog from 'primevue/dialog'
import InputText from 'primevue/inputtext'
import Message from 'primevue/message'
import Select from 'primevue/select'
import BaseButton from '../../../components/base/BaseButton.vue'
import BaseCard from '../../../components/base/BaseCard.vue'
import BaseInput from '../../../components/base/BaseInput.vue'
import PageHeader from '../../../components/layout/PageHeader.vue'
import StatusBadge from '../../../components/shared/StatusBadge.vue'
import { normalizeError } from '../../../api/errors'
import { formatCnpj, formatCurrency, formatDate } from '../../../utils/format'
import { listarClientes, obterCliente, vincularCodigoSci } from '../services/clientes.service'
import type { Cliente, ClienteDetalhe } from '../types/cliente'
import { usePageRefresh, type PageRefreshReason } from '../../../composables/usePageRefresh'

const clientes = ref<Cliente[]>([])
const router = useRouter()
const loading = ref(true)
const error = ref('')
const success = ref('')
const busca = ref('')
const situacao = ref<boolean | null>(true)
const detalheOpen = ref(false)
const detalheLoading = ref(false)
const detalhe = ref<ClienteDetalhe | null>(null)
const sciOpen = ref(false)
const sciCodigo = ref('')
const saving = ref(false)
const dialogError = ref('')
let controller: AbortController | undefined
let generation = 0
let searchTimer: ReturnType<typeof setTimeout> | undefined

const situacoes = [
  { label: 'Todos', value: null },
  { label: 'Ativos', value: true },
  { label: 'Inativos', value: false },
]
const localizacao = computed(() => detalhe.value && [detalhe.value.cidade, detalhe.value.uf].filter(Boolean).join(' / '))

async function load(options: { background?: boolean; reportErrors?: boolean } = {}) {
  if (searchTimer) clearTimeout(searchTimer)
  const { background = false, reportErrors = true } = options
  const request = ++generation
  controller?.abort()
  controller = new AbortController()
  if (!background) loading.value = true
  if (reportErrors) error.value = ''
  try {
    const resultado = await listarClientes({ busca: busca.value.trim() || undefined, ativo: situacao.value ?? undefined }, controller.signal)
    if (request === generation) {
      clientes.value = resultado
      error.value = ''
    }
  } catch (cause) {
    if (request === generation && reportErrors && !axios.isCancel(cause)) error.value = normalizeError(cause).message
  } finally { if (request === generation && !background) loading.value = false }
}

function agendarBusca() {
  if (searchTimer) clearTimeout(searchTimer)
  searchTimer = setTimeout(() => void load(), 350)
}

async function refreshData(reason: PageRefreshReason) {
  await load({ background: true, reportErrors: reason === 'manual' })
}

const { refreshing, refreshNow } = usePageRefresh(refreshData, { busy: loading })

async function abrirDetalhe(item: Cliente) {
  detalheOpen.value = true
  detalheLoading.value = true
  dialogError.value = ''
  detalhe.value = null
  try { detalhe.value = await obterCliente(item.id) }
  catch (cause) { dialogError.value = normalizeError(cause).message }
  finally { detalheLoading.value = false }
}

function abrirSci(item: Cliente | ClienteDetalhe) {
  if (!detalhe.value || detalhe.value.id !== item.id) {
    detalhe.value = { ...item, whatsapp: null, cep: null, logradouro: null, numero: null, bairro: null, observacoes: null, prospectOrigemId: null, inicioRelacionamentoEm: null, socios: [], contratos: [], documentos: [] }
  }
  sciCodigo.value = item.sciCodigo || ''
  dialogError.value = ''
  sciOpen.value = true
}

async function salvarSci() {
  if (!detalhe.value) return
  const codigo = sciCodigo.value.trim()
  if (!codigo) { dialogError.value = 'Informe o código SCI.'; return }
  saving.value = true
  dialogError.value = ''
  try {
    await vincularCodigoSci(detalhe.value.id, codigo)
    sciOpen.value = false
    success.value = 'Código SCI vinculado ao cliente.'
    await load()
    if (detalheOpen.value) detalhe.value = await obterCliente(detalhe.value.id)
  } catch (cause) { dialogError.value = normalizeError(cause).message }
  finally { saving.value = false }
}

function abrirAdmissoes(clienteId: string) {
  void router.push({ name: 'admissoes', query: { clienteId } })
}

watch(busca, agendarBusca)
watch(situacao, () => void load())
void load()
onBeforeUnmount(() => {
  if (searchTimer) clearTimeout(searchTimer)
  ++generation
  controller?.abort()
})
</script>

<template>
  <PageHeader title="Clientes" description="Consulte empresas convertidas, seus contratos e documentos.">
    <BaseButton label="Atualizar" icon="pi pi-refresh" variant="secondary" :loading="refreshing" @click="refreshNow" />
  </PageHeader>
  <Message v-if="success" severity="success" :closable="true" class="page-message" @close="success = ''">{{ success }}</Message>
  <Message v-if="error" severity="error" :closable="false" class="page-message">{{ error }}</Message>
  <BaseCard class="data-card">
    <form class="filters" role="search" aria-label="Filtrar clientes" @submit.prevent="() => load()">
      <BaseInput id="client-search" v-model="busca" label="Buscar cliente" type="search" placeholder="Razão social ou CNPJ" autocomplete="off" />
      <div class="field"><label for="client-status">Situação</label><Select input-id="client-status" v-model="situacao" :options="situacoes" option-label="label" option-value="value" /></div>
    </form>
    <div class="list-heading"><h2>Carteira de clientes</h2><span>{{ loading ? 'Carregando…' : `${clientes.length} ${clientes.length === 1 ? 'cliente' : 'clientes'}` }}</span></div>
    <div v-if="loading" class="list-state" aria-busy="true"><i class="pi pi-spinner loading-spinner" /><p>Carregando clientes…</p></div>
    <div v-else-if="error" class="list-state"><BaseButton label="Tentar novamente" icon="pi pi-refresh" variant="secondary" @click="() => load()" /></div>
    <DataTable v-else :value="clientes" data-key="id" class="work-table clients-table" :table-props="{ 'aria-label': 'Lista de clientes' }">
      <Column field="razaoSocial" header="Cliente"><template #body="{ data }"><div class="company-cell"><strong>{{ data.razaoSocial }}</strong><span>{{ formatCnpj(data.cnpj || data.cpf) }}</span></div></template></Column>
      <Column field="ativo" header="Situação"><template #body="{ data }"><StatusBadge :label="data.ativo ? 'Ativo' : 'Inativo'" :severity="data.ativo ? 'success' : 'secondary'" /></template></Column>
      <Column field="sciCodigo" header="SCI"><template #body="{ data }"><span class="numeric">{{ data.sciCodigo || 'Não vinculado' }}</span></template></Column>
      <Column header="Relacionamentos"><template #body="{ data }"><span class="relation-counts">{{ data.quantidadeContratos }} contratos · {{ data.quantidadeDocumentos }} documentos</span></template></Column>
      <Column header="Ações"><template #body="{ data }"><div class="row-actions"><BaseButton label="Detalhes" icon="pi pi-eye" variant="text" @click="abrirDetalhe(data)" /><BaseButton label="Admissões" icon="pi pi-id-card" variant="secondary" @click="abrirAdmissoes(data.id)" /><BaseButton label="Vincular SCI" icon="pi pi-link" variant="secondary" @click="abrirSci(data)" /></div></template></Column>
      <template #empty><div class="empty-state"><i class="pi pi-building" /><h3>Nenhum cliente encontrado</h3><p>Clientes são criados quando o fluxo comercial é concluído.</p></div></template>
    </DataTable>
  </BaseCard>

  <Dialog v-model:visible="detalheOpen" modal header="Detalhes do cliente" class="workflow-dialog client-detail-dialog" :style="{ width: 'min(860px, 95vw)' }">
    <div v-if="detalheLoading" class="list-state compact-state" aria-busy="true"><i class="pi pi-spinner loading-spinner" /><p>Carregando detalhes…</p></div>
    <Message v-else-if="dialogError" severity="error" :closable="false">{{ dialogError }}</Message>
    <div v-else-if="detalhe" class="client-detail">
      <div class="detail-heading"><div><h2>{{ detalhe.razaoSocial }}</h2><p>{{ detalhe.nomeFantasia || formatCnpj(detalhe.cnpj || detalhe.cpf) }}</p></div><StatusBadge :label="detalhe.ativo ? 'Ativo' : 'Inativo'" :severity="detalhe.ativo ? 'success' : 'secondary'" /></div>
      <dl class="detail-grid">
        <div><dt>Regime tributário</dt><dd>{{ detalhe.regimeTributario || 'Não informado' }}</dd></div>
        <div><dt>Código SCI</dt><dd>{{ detalhe.sciCodigo || 'Não vinculado' }}</dd></div>
        <div><dt>Contato</dt><dd>{{ detalhe.email || detalhe.telefone || 'Não informado' }}</dd></div>
        <div><dt>Localização</dt><dd>{{ localizacao || 'Não informada' }}</dd></div>
        <div><dt>Relacionamento desde</dt><dd>{{ formatDate(detalhe.inicioRelacionamentoEm) }}</dd></div>
      </dl>
      <section class="detail-section"><h3>Sócios <span>{{ detalhe.socios.length }}</span></h3><div v-if="detalhe.socios.length" class="detail-list"><div v-for="socio in detalhe.socios" :key="socio.id"><strong>{{ socio.nome }}</strong><span>{{ socio.participacao }}% · {{ socio.cpf || 'CPF não informado' }}</span></div></div><p v-else class="muted">Nenhum sócio vinculado.</p></section>
      <section class="detail-section"><h3>Contratos <span>{{ detalhe.contratos.length }}</span></h3><div v-if="detalhe.contratos.length" class="detail-list"><div v-for="contrato in detalhe.contratos" :key="contrato.id"><strong>{{ contrato.numero }} · {{ contrato.status }}</strong><span>{{ formatCurrency(contrato.valorTotal) }} · início em {{ formatDate(contrato.dataInicio) }}</span></div></div><p v-else class="muted">Nenhum contrato vinculado.</p></section>
      <section class="detail-section"><h3>Documentos <span>{{ detalhe.documentos.length }}</span></h3><div v-if="detalhe.documentos.length" class="detail-list"><div v-for="documento in detalhe.documentos" :key="documento.id"><strong>{{ documento.nomeArquivo }}</strong><span>{{ documento.tipo }} · {{ documento.legivel ? 'legível' : 'requer revisão' }}</span></div></div><p v-else class="muted">Nenhum documento vinculado.</p></section>
    </div>
    <template #footer><BaseButton label="Fechar" variant="text" @click="detalheOpen = false" /><BaseButton v-if="detalhe" label="Admissões" icon="pi pi-id-card" variant="secondary" @click="abrirAdmissoes(detalhe.id)" /><BaseButton v-if="detalhe" label="Vincular SCI" icon="pi pi-link" variant="secondary" @click="abrirSci(detalhe)" /></template>
  </Dialog>

  <Dialog v-model:visible="sciOpen" modal header="Vincular código SCI" class="workflow-dialog" :style="{ width: 'min(440px, 94vw)' }">
    <form id="sci-form" class="dialog-form" @submit.prevent="salvarSci">
      <Message v-if="dialogError" severity="error" :closable="false">{{ dialogError }}</Message>
      <div class="field"><label for="sci-code">Código do cliente no SCI</label><InputText id="sci-code" v-model="sciCodigo" autocomplete="off" /></div>
    </form>
    <template #footer><BaseButton label="Cancelar" variant="text" @click="sciOpen = false" /><BaseButton label="Vincular" icon="pi pi-link" type="submit" form="sci-form" :loading="saving" /></template>
  </Dialog>
</template>
