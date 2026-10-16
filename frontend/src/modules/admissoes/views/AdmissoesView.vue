<script setup lang="ts">
import { computed, onBeforeUnmount, ref, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
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
import { formatCurrency, formatDate } from '../../../utils/format'
import { listarClientes } from '../../clientes/services/clientes.service'
import type { Cliente } from '../../clientes/types/cliente'
import AdmissaoDetalheDialog from '../components/AdmissaoDetalheDialog.vue'
import AdmissaoDocumentosDialog from '../components/AdmissaoDocumentosDialog.vue'
import AdmissaoFormDialog from '../components/AdmissaoFormDialog.vue'
import { atualizarAdmissao, cancelarAdmissao, concluirAdmissao, criarAdmissao, gerarLinkFichaAdmissao, listarAdmissoes, obterAdmissao, revogarLinkFichaAdmissao } from '../services/admissoes.service'
import { statusAdmissaoOptions, type AdmissaoDetalhe, type AdmissaoResumo, type DadosAdmissaoPayload } from '../types/admissao'

const route = useRoute()
const router = useRouter()
const clientes = ref<Cliente[]>([])
const admissoes = ref<AdmissaoResumo[]>([])
const loading = ref(true)
const error = ref('')
const success = ref('')
const busca = ref('')
const status = ref('')
const clienteId = ref(typeof route.query.clienteId === 'string' ? route.query.clienteId : '')
const formOpen = ref(false)
const detailOpen = ref(false)
const documentsOpen = ref(false)
const confirmOpen = ref(false)
const selected = ref<AdmissaoDetalhe | null>(null)
const documentsTarget = ref<AdmissaoResumo | null>(null)
const actionTarget = ref<AdmissaoResumo | null>(null)
const detailLoading = ref(false)
const saving = ref(false)
const actionId = ref('')
const formError = ref('')
const confirmAction = ref<'concluir' | 'cancelar'>('concluir')
const linkOpen = ref(false)
const linkFicha = ref('')
const linkExpiraEm = ref('')
const linkAdmissaoId = ref('')
const linkLoading = ref(false)
let controller: AbortController | undefined
let clientesController: AbortController | undefined
let generation = 0
let searchTimer: ReturnType<typeof setTimeout> | undefined

const clienteOptions = computed(() => [{ id: '', razaoSocial: 'Todos os clientes' } as Cliente, ...clientes.value])
const terminal = (item: AdmissaoResumo) => item.status === 'CONCLUIDA' || item.status === 'CANCELADA'
const statusSeverity = (codigo: string) => codigo === 'CONCLUIDA' ? 'success' : codigo === 'CANCELADA' ? 'danger' : 'warn'

async function load() {
  if (searchTimer) clearTimeout(searchTimer)
  const request = ++generation
  controller?.abort()
  const currentController = new AbortController()
  controller = currentController
  loading.value = true
  error.value = ''
  try {
    const listaAdmissoes = await listarAdmissoes({ clienteId: clienteId.value || undefined, status: status.value || undefined, busca: busca.value.trim() || undefined }, currentController.signal)
    if (request !== generation) return
    admissoes.value = listaAdmissoes
    await router.replace({ query: clienteId.value ? { clienteId: clienteId.value } : {} })
  } catch (cause) {
    if (request === generation && !axios.isCancel(cause)) error.value = normalizeError(cause).message
  } finally { if (request === generation) loading.value = false }
}

async function carregarClientes() {
  clientesController?.abort()
  const currentController = new AbortController()
  clientesController = currentController
  try { clientes.value = await listarClientes({ ativo: true }, currentController.signal) }
  catch (cause) {
    if (!axios.isCancel(cause)) error.value = normalizeError(cause).message
  }
}

function agendarBusca() {
  if (searchTimer) clearTimeout(searchTimer)
  searchTimer = setTimeout(() => void load(), 350)
}

function abrirCriacao() {
  selected.value = null
  formError.value = ''
  formOpen.value = true
}

async function carregarDetalhe(item: AdmissaoResumo): Promise<AdmissaoDetalhe | null> {
  detailLoading.value = true
  error.value = ''
  try { return await obterAdmissao(item.id) }
  catch (cause) { error.value = normalizeError(cause).message; return null }
  finally { detailLoading.value = false }
}

async function abrirDetalhe(item: AdmissaoResumo) {
  detailOpen.value = true
  selected.value = await carregarDetalhe(item)
}

function abrirDocumentos(item: AdmissaoResumo) {
  documentsTarget.value = item
  documentsOpen.value = true
}

async function gerarLink(admissaoId: string) {
  linkLoading.value = true
  error.value = ''
  try {
    const resultado = await gerarLinkFichaAdmissao(admissaoId)
    linkAdmissaoId.value = admissaoId
    linkFicha.value = `${window.location.origin}/ficha-admissao/${resultado.token}`
    linkExpiraEm.value = resultado.expiraEm
    linkOpen.value = true
    await load()
  } catch (cause) { error.value = normalizeError(cause).message }
  finally { linkLoading.value = false }
}

async function copiarLink() {
  try {
    await navigator.clipboard.writeText(linkFicha.value)
    success.value = 'Link da ficha copiado.'
  } catch {
    error.value = 'Não foi possível copiar automaticamente. Selecione o link e copie manualmente.'
  }
}

async function revogarLink() {
  if (!linkAdmissaoId.value) return
  linkLoading.value = true
  try {
    await revogarLinkFichaAdmissao(linkAdmissaoId.value)
    linkOpen.value = false
    success.value = 'Link da ficha revogado.'
    await load()
  } catch (cause) { error.value = normalizeError(cause).message }
  finally { linkLoading.value = false }
}

async function abrirEdicao(item?: AdmissaoResumo) {
  if (item) selected.value = await carregarDetalhe(item)
  if (!selected.value) return
  detailOpen.value = false
  formError.value = ''
  formOpen.value = true
}

async function salvar(clienteSelecionadoId: string, dados: DadosAdmissaoPayload) {
  saving.value = true
  formError.value = ''
  try {
    const editando = selected.value
    if (editando) await atualizarAdmissao(editando.id, dados)
    else await criarAdmissao({ clienteId: clienteSelecionadoId, dados })
    formOpen.value = false
    selected.value = null
    success.value = editando ? 'Admissão atualizada.' : 'Admissão manual criada.'
    await load()
  } catch (cause) { formError.value = normalizeError(cause).message }
  finally { saving.value = false }
}

function confirmar(item: AdmissaoResumo | AdmissaoDetalhe, acao: 'concluir' | 'cancelar') {
  actionTarget.value = item
  confirmAction.value = acao
  confirmOpen.value = true
}

async function executarAcao() {
  if (!actionTarget.value) return
  const id = actionTarget.value.id
  actionId.value = id
  error.value = ''
  try {
    if (confirmAction.value === 'concluir') await concluirAdmissao(id)
    else await cancelarAdmissao(id)
    confirmOpen.value = false
    detailOpen.value = false
    selected.value = null
    actionTarget.value = null
    success.value = confirmAction.value === 'concluir' ? 'Admissão concluída e funcionário vinculado à empresa.' : 'Admissão cancelada.'
    await load()
  } catch (cause) { error.value = normalizeError(cause).message; confirmOpen.value = false }
  finally { actionId.value = '' }
}

watch(busca, agendarBusca)
watch([clienteId, status], () => void load())
void load()
void carregarClientes()
onBeforeUnmount(() => {
  if (searchTimer) clearTimeout(searchTimer)
  ++generation
  controller?.abort()
  clientesController?.abort()
})
</script>

<template>
  <PageHeader title="Admissões" description="Cadastre funcionários, revise os dados e conclua a admissão na empresa.">
    <BaseButton label="Nova admissão" icon="pi pi-plus" @click="abrirCriacao" />
  </PageHeader>
  <Message v-if="success" severity="success" :closable="true" class="page-message" @close="success = ''">{{ success }}</Message>
  <Message v-if="error" severity="error" :closable="true" class="page-message" @close="error = ''">{{ error }}</Message>

  <BaseCard class="data-card">
    <form class="filters admission-filters" role="search" aria-label="Filtrar admissões" @submit.prevent="load">
      <BaseInput id="admission-search" v-model="busca" label="Buscar funcionário" type="search" placeholder="Nome ou CPF" autocomplete="off" />
      <div class="field"><label for="admission-client-filter">Cliente</label><Select input-id="admission-client-filter" v-model="clienteId" :options="clienteOptions" option-label="razaoSocial" option-value="id" filter /></div>
      <div class="field"><label for="admission-status-filter">Situação</label><Select input-id="admission-status-filter" v-model="status" :options="statusAdmissaoOptions" option-label="label" option-value="value" /></div>
    </form>
    <div class="list-heading"><h2>Admissões cadastradas</h2><span>{{ loading ? 'Carregando...' : `${admissoes.length} ${admissoes.length === 1 ? 'admissão' : 'admissões'}` }}</span></div>
    <div v-if="loading" class="list-state" aria-busy="true"><i class="pi pi-spinner loading-spinner" /><p>Carregando admissões...</p></div>
    <DataTable v-else :value="admissoes" data-key="id" class="work-table admissions-table" :table-props="{ 'aria-label': 'Lista de admissões' }">
      <Column field="nomeFuncionario" header="Funcionário"><template #body="{ data }"><div class="company-cell"><strong>{{ data.nomeFuncionario }}</strong><span>{{ data.cpf || 'CPF não informado' }}</span></div></template></Column>
      <Column field="empresaNome" header="Cliente" />
      <Column field="statusDescricao" header="Situação"><template #body="{ data }"><StatusBadge :label="data.statusDescricao" :severity="statusSeverity(data.status)" /></template></Column>
      <Column header="Vínculo"><template #body="{ data }"><div class="company-cell"><strong>{{ data.funcao || 'Função não informada' }}</strong><span>{{ formatDate(data.dataAdmissaoPrevista) }} · {{ data.salario == null ? 'Salário não informado' : formatCurrency(data.salario) }}</span></div></template></Column>
      <Column header="Ações"><template #body="{ data }"><div class="admission-row-actions">
        <BaseButton label="Documentos" icon="pi pi-upload" variant="text" @click="abrirDocumentos(data)" />
        <BaseButton label="Detalhes" icon="pi pi-eye" variant="text" @click="abrirDetalhe(data)" />
        <BaseButton v-if="!terminal(data)" label="Editar" icon="pi pi-pencil" variant="secondary" @click="abrirEdicao(data)" />
        <BaseButton v-if="!terminal(data)" label="Concluir" icon="pi pi-check" :loading="actionId === data.id" @click="confirmar(data, 'concluir')" />
      </div></template></Column>
      <template #empty><div class="empty-state"><i class="pi pi-id-card" /><h3>Nenhuma admissão encontrada</h3><p>Crie uma admissão manual para iniciar o cadastro do funcionário.</p><BaseButton label="Nova admissão" icon="pi pi-plus" @click="abrirCriacao" /></div></template>
    </DataTable>
  </BaseCard>

  <AdmissaoFormDialog v-model:visible="formOpen" :clientes="clientes" :admissao="selected" :cliente-inicial-id="clienteId" :saving="saving" :error="formError" @salvar="salvar" />
  <AdmissaoDetalheDialog v-model:visible="detailOpen" :admissao="selected" :loading="detailLoading" @editar="abrirEdicao()" @concluir="selected && confirmar(selected, 'concluir')" @cancelar="selected && confirmar(selected, 'cancelar')" />
  <AdmissaoDocumentosDialog v-if="documentsOpen" v-model:visible="documentsOpen" :admissao="documentsTarget" :link-loading="linkLoading" @atualizado="() => load()" @gerar-link="gerarLink" />

  <Dialog v-model:visible="linkOpen" modal header="Ficha digital de admissão" class="workflow-dialog" :style="{ width: 'min(620px, 94vw)' }">
    <div class="admission-link-dialog">
      <p>Compartilhe este link somente com o funcionário. Um novo link invalida o anterior.</p>
      <div class="admission-link-value"><InputText :model-value="linkFicha" readonly /><BaseButton icon="pi pi-copy" title="Copiar link" aria-label="Copiar link" @click="copiarLink" /></div>
      <small>Expira em {{ formatDate(linkExpiraEm) }}</small>
    </div>
    <template #footer><BaseButton label="Revogar link" icon="pi pi-ban" variant="text" :loading="linkLoading" @click="revogarLink" /><BaseButton label="Fechar" variant="secondary" @click="linkOpen = false" /></template>
  </Dialog>

  <Dialog v-model:visible="confirmOpen" modal :header="confirmAction === 'concluir' ? 'Concluir admissão' : 'Cancelar admissão'" class="workflow-dialog" :style="{ width: 'min(480px, 94vw)' }">
    <p v-if="confirmAction === 'concluir'" class="confirmation-copy">Ao concluir, o funcionário será vinculado à empresa e entrará na contagem de funcionários. Confirme que CPF, função e data de admissão estão preenchidos.</p>
    <p v-else class="confirmation-copy">A admissão será cancelada e não poderá mais ser editada ou concluída.</p>
    <template #footer><BaseButton label="Voltar" variant="text" @click="confirmOpen = false" /><BaseButton :label="confirmAction === 'concluir' ? 'Concluir admissão' : 'Cancelar admissão'" :icon="confirmAction === 'concluir' ? 'pi pi-check' : 'pi pi-ban'" :loading="!!actionId" @click="executarAcao" /></template>
  </Dialog>
</template>

<style scoped>
.admission-row-actions { display: flex; flex-direction: column; align-items: flex-start; gap: 2px; }
.admission-row-actions :deep(.base-button.p-button) { min-height: 32px; padding: 4px 8px; font-size: 13px; font-weight: 600; }
</style>
