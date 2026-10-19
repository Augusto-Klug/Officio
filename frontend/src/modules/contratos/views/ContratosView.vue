<script setup lang="ts">
import { computed, onBeforeUnmount, onMounted, ref, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import axios from 'axios'
import Column from 'primevue/column'
import DataTable from 'primevue/datatable'
import Dialog from 'primevue/dialog'
import InputText from 'primevue/inputtext'
import Message from 'primevue/message'
import Select from 'primevue/select'
import Textarea from 'primevue/textarea'
import BaseButton from '../../../components/base/BaseButton.vue'
import BaseCard from '../../../components/base/BaseCard.vue'
import PageHeader from '../../../components/layout/PageHeader.vue'
import StatusBadge from '../../../components/shared/StatusBadge.vue'
import { normalizeError } from '../../../api/errors'
import { formatCnpj, formatCurrency, formatDate } from '../../../utils/format'
import { formatarCpf, somenteDigitos } from '../../../utils/inputMasks'
import { enviarContratoParaAssinatura, gerarPdfContrato, listarContratos, listarEmpresasContrato, obterPdfAssinado } from '../services/contratos.service'
import type { Contrato, EmpresaContratoFiltro } from '../types/contrato'
import { usePageRefresh, type PageRefreshReason } from '../../../composables/usePageRefresh'
import { useRealtimePageRefresh } from '../../../composables/useRealtimePageRefresh'

const route = useRoute()
const router = useRouter()
const contratos = ref<Contrato[]>([])
const empresas = ref<EmpresaContratoFiltro[]>([])
const loading = ref(true)
const empresasLoading = ref(true)
const error = ref('')
const empresasError = ref('')
const success = ref('')
const actionId = ref('')
const signOpen = ref(false)
const selected = ref<Contrato | null>(null)
const formError = ref('')
const resultLinks = ref<Array<{ label: string; url: string }>>([])
const signer = ref({ nome: '', email: '', cpf: '', tipo: 'cliente', mensagemPersonalizada: '' })
let controller: AbortController | undefined
let empresasController: AbortController | undefined
let generation = 0
const empresaId = computed(() => typeof route.query.empresaId === 'string' ? route.query.empresaId : null)
const statusFiltro = computed(() => typeof route.query.status === 'string' ? route.query.status : null)
const statusOptions = [
  { label: 'Todos os status', value: null },
  { label: 'Rascunho', value: 'Rascunho' },
  { label: 'Aguardando assinatura', value: 'AguardandoAssinatura' },
  { label: 'Ativo', value: 'Ativo' },
  { label: 'Encerrado', value: 'Encerrado' },
  { label: 'Cancelado', value: 'Cancelado' },
]
const empresaOptions = computed(() => empresas.value.map((empresa) => ({
  label: empresa.cnpj ? `${empresa.razaoSocial} · ${formatCnpj(empresa.cnpj)}` : empresa.razaoSocial,
  value: empresa.id,
})))
const paginaOcupada = computed(() => loading.value || !!actionId.value)

async function load(options: { background?: boolean; reportErrors?: boolean } = {}) {
  const { background = false, reportErrors = true } = options
  const request = ++generation
  controller?.abort()
  controller = new AbortController()
  if (!background) loading.value = true
  if (reportErrors) error.value = ''
  try {
    const resultado = await listarContratos(empresaId.value || undefined, controller.signal, statusFiltro.value || undefined)
    if (request === generation) {
      contratos.value = resultado
      error.value = ''
    }
  } catch (cause) {
    if (request === generation && reportErrors && !axios.isCancel(cause)) error.value = normalizeError(cause).message
  } finally {
    if (request === generation && !background) loading.value = false
  }
}

async function refreshData(reason: PageRefreshReason) {
  await load({ background: true, reportErrors: reason === 'manual' })
}

const { refreshing, refreshNow, refreshAutomatically } = usePageRefresh(refreshData, {
  busy: paginaOcupada,
})
useRealtimePageRefresh('contrato', refreshAutomatically, computed(() => paginaOcupada.value || refreshing.value))

async function loadEmpresas() {
  empresasController?.abort()
  empresasController = new AbortController()
  empresasLoading.value = true
  empresasError.value = ''
  try { empresas.value = await listarEmpresasContrato(empresasController.signal) }
  catch (cause) { if (!axios.isCancel(cause)) empresasError.value = normalizeError(cause).message }
  finally { empresasLoading.value = false }
}

function aplicarFiltroEmpresa(value: string | null) {
  void router.replace({ query: { ...route.query, empresaId: value || undefined } })
}

function aplicarFiltroStatus(value: string | null) {
  void router.replace({ query: { ...route.query, status: value || undefined } })
}

function limparFiltros() {
  void router.replace({ query: { ...route.query, empresaId: undefined, status: undefined } })
}

async function gerarPdf(item: Contrato) {
  actionId.value = item.id
  error.value = ''
  resultLinks.value = []
  try {
    const result = await gerarPdfContrato(item.id)
    success.value = 'PDF do contrato gerado com sucesso.'
    resultLinks.value = [{ label: 'Abrir PDF gerado', url: result.pdfUrl }]
    await load()
  } catch (cause) { error.value = normalizeError(cause).message }
  finally { actionId.value = '' }
}

function abrirAssinatura(item: Contrato) {
  selected.value = item
  signer.value = { nome: item.razaoSocial, email: '', cpf: '', tipo: 'cliente', mensagemPersonalizada: '' }
  formError.value = ''
  resultLinks.value = []
  signOpen.value = true
}

async function enviarAssinatura() {
  if (!selected.value) return
  if (!signer.value.nome.trim() || !/^\S+@\S+\.\S+$/.test(signer.value.email)) {
    formError.value = 'Informe o nome e um e-mail válido para o signatário.'
    return
  }
  actionId.value = selected.value.id
  formError.value = ''
  try {
    const result = await enviarContratoParaAssinatura(selected.value.id, {
      signatarios: [{ nome: signer.value.nome.trim(), email: signer.value.email.trim(), cpf: somenteDigitos(signer.value.cpf) || undefined, tipo: signer.value.tipo }],
      mensagemPersonalizada: signer.value.mensagemPersonalizada.trim() || undefined,
    })
    signOpen.value = false
    success.value = result.metodo === 'digital_api' ? 'Contrato enviado para assinatura.' : (result.mensagem || 'PDF disponível para assinatura manual.')
    resultLinks.value = [
      ...(result.signatarios || []).map((item) => ({ label: `Assinar como ${item.email}`, url: item.link })),
      ...(result.pdfUrl ? [{ label: 'Abrir PDF para assinatura manual', url: result.pdfUrl }] : []),
    ]
    await load()
  } catch (cause) { formError.value = normalizeError(cause).message }
  finally { actionId.value = '' }
}

async function abrirPdfAssinado(item: Contrato) {
  actionId.value = item.id
  resultLinks.value = []
  try { resultLinks.value = [{ label: 'Baixar PDF assinado', url: await obterPdfAssinado(item.id) }]; success.value = 'PDF assinado disponível.' }
  catch (cause) { error.value = normalizeError(cause).message }
  finally { actionId.value = '' }
}

function severity(status: string) {
  if (status === 'Ativo') return 'success'
  if (status === 'AguardandoAssinatura') return 'warn'
  if (status === 'Cancelado' || status === 'Encerrado') return 'danger'
  return 'secondary'
}

watch(() => [route.query.empresaId, route.query.status], () => void load(), { immediate: true })
void loadEmpresas()
onBeforeUnmount(() => {
  ++generation
  controller?.abort()
  empresasController?.abort()
})
</script>

<template>
  <PageHeader title="Contratos" description="Gere documentos, acompanhe assinaturas e acesse os arquivos concluídos.">
    <BaseButton label="Atualizar" icon="pi pi-refresh" variant="secondary" :loading="refreshing" @click="refreshNow" />
  </PageHeader>
  <Message v-if="success" severity="success" :closable="true" class="page-message" @close="success = ''">{{ success }}</Message>
  <div v-if="resultLinks.length" class="result-links" aria-label="Links da operação">
    <a v-for="item in resultLinks" :key="item.url" :href="item.url" target="_blank" rel="noopener noreferrer"><i class="pi pi-external-link" aria-hidden="true" />{{ item.label }}</a>
  </div>
  <Message v-if="error" severity="error" :closable="false" class="page-message">{{ error }}</Message>
  <BaseCard class="data-card">
    <form class="filters" role="search" aria-label="Filtrar contratos" @submit.prevent>
      <div class="field">
        <label for="contract-company">Empresa</label>
        <Select input-id="contract-company" :model-value="empresaId" :options="empresaOptions" option-label="label" option-value="value" filter show-clear placeholder="Todas as empresas" :loading="empresasLoading" :disabled="empresasLoading || !!empresasError" @update:model-value="aplicarFiltroEmpresa" />
        <small v-if="empresasError" class="field-error">{{ empresasError }}</small>
      </div>
      <div class="field">
        <label for="contract-status">Status</label>
        <Select input-id="contract-status" :model-value="statusFiltro" :options="statusOptions" option-label="label" option-value="value" placeholder="Todos os status" @update:model-value="aplicarFiltroStatus" />
      </div>
      <BaseButton v-if="empresaId || statusFiltro" label="Limpar filtros" variant="text" @click="limparFiltros" />
    </form>
    <div class="list-heading"><h2>Contratos</h2><span>{{ loading ? 'Carregando…' : `${contratos.length} ${contratos.length === 1 ? 'contrato' : 'contratos'}` }}</span></div>
    <div v-if="loading" class="list-state" aria-busy="true"><i class="pi pi-spinner loading-spinner" /><p>Carregando contratos…</p></div>
    <div v-else-if="error" class="list-state"><BaseButton label="Tentar novamente" icon="pi pi-refresh" variant="secondary" @click="() => load()" /></div>
    <DataTable v-else :value="contratos" data-key="id" class="work-table contracts-table" :table-props="{ 'aria-label': 'Lista de contratos' }">
      <Column field="numero" header="Contrato"><template #body="{ data }"><div class="company-cell"><strong>{{ data.numero }}</strong><span>{{ data.razaoSocial }}</span></div></template></Column>
      <Column field="status" header="Status"><template #body="{ data }"><StatusBadge :label="data.status" :severity="severity(data.status)" /></template></Column>
      <Column field="valorTotal" header="Valor"><template #body="{ data }"><span class="numeric">{{ formatCurrency(data.valorTotal) }}</span></template></Column>
      <Column field="dataInicio" header="Início"><template #body="{ data }"><span class="numeric">{{ formatDate(data.dataInicio) }}</span></template></Column>
      <Column header="Ações"><template #body="{ data }"><div class="row-actions"><BaseButton v-if="!data.pdfGerado" label="Gerar PDF" icon="pi pi-file-pdf" variant="secondary" :loading="actionId === data.id" :disabled="!!actionId && actionId !== data.id" @click="gerarPdf(data)" /><BaseButton v-else-if="data.status === 'Rascunho'" label="Enviar" icon="pi pi-send" variant="secondary" :loading="actionId === data.id" :disabled="!!actionId && actionId !== data.id" @click="abrirAssinatura(data)" /><BaseButton v-if="data.pdfAssinadoDisponivel" label="Assinado" icon="pi pi-download" variant="text" :loading="actionId === data.id" :disabled="!!actionId && actionId !== data.id" @click="abrirPdfAssinado(data)" /></div></template></Column>
      <template #empty><div class="empty-state"><i class="pi pi-file" /><h3>{{ empresaId ? 'Nenhum contrato para esta empresa' : statusFiltro ? 'Nenhum contrato com este status' : 'Nenhum contrato criado' }}</h3><p>{{ empresaId || statusFiltro ? 'Ajuste ou limpe os filtros para ver outros contratos.' : 'Contratos gerados a partir de orçamentos aceitos aparecerão aqui.' }}</p><BaseButton v-if="empresaId || statusFiltro" label="Limpar filtros" variant="secondary" @click="limparFiltros" /></div></template>
    </DataTable>
  </BaseCard>

  <Dialog v-model:visible="signOpen" modal header="Enviar para assinatura" class="workflow-dialog" :style="{ width: 'min(580px, 94vw)' }">
    <form id="signature-form" class="dialog-form" @submit.prevent="enviarAssinatura">
      <Message v-if="formError" severity="error" :closable="false">{{ formError }}</Message>
      <div class="field"><label for="signer-name">Nome do signatário</label><InputText id="signer-name" v-model="signer.nome" autocomplete="name" /></div>
      <div class="form-grid two-columns"><div class="field"><label for="signer-email">E-mail</label><InputText id="signer-email" v-model="signer.email" type="email" autocomplete="email" /></div><div class="field"><label for="signer-cpf">CPF (opcional)</label><InputText id="signer-cpf" :model-value="signer.cpf" inputmode="numeric" maxlength="14" placeholder="000.000.000-00" @update:model-value="signer.cpf = formatarCpf($event)" /></div></div>
      <div class="field"><label for="signature-message">Mensagem (opcional)</label><Textarea id="signature-message" v-model="signer.mensagemPersonalizada" rows="3" auto-resize /></div>
    </form>
    <template #footer><BaseButton label="Cancelar" variant="text" @click="signOpen = false" /><BaseButton label="Enviar contrato" icon="pi pi-send" type="submit" form="signature-form" :loading="!!actionId" /></template>
  </Dialog>
</template>
