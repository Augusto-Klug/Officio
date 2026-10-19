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
import { formatarCpf } from '../../../utils/inputMasks'
import { listarClientes } from '../../clientes/services/clientes.service'
import type { Cliente } from '../../clientes/types/cliente'
import AdmissaoDetalheDialog from '../components/AdmissaoDetalheDialog.vue'
import AdmissaoDocumentosDialog from '../components/AdmissaoDocumentosDialog.vue'
import AdmissaoFormDialog from '../components/AdmissaoFormDialog.vue'
import { atualizarAdmissao, cancelarAdmissao, concluirAdmissao, concluirRevisaoDocumentalAdmissao, criarAdmissao, enviarLinkFichaAdmissaoPorEmail, gerarLinkFichaAdmissao, gerarLinkFichaEmpresaAdmissao, listarAdmissoes, obterAdmissao, revogarLinkFichaAdmissao, revogarLinkFichaEmpresaAdmissao } from '../services/admissoes.service'
import { statusAdmissaoOptions, type AdmissaoDetalhe, type AdmissaoResumo, type DadosAdmissaoPayload } from '../types/admissao'

const route = useRoute()
const router = useRouter()
const clientes = ref<Cliente[]>([])
const clientesCarregados = ref(false)
const admissoes = ref<AdmissaoResumo[]>([])
const empresaId = computed(() => typeof route.params.empresaId === 'string' ? route.params.empresaId : '')
const nomeEmpresa = computed(() => typeof route.query.empresaNome === 'string'
  ? route.query.empresaNome
  : admissoes.value[0]?.empresaNome || 'empresa selecionada')
const clientesDaEmpresa = computed(() => clientes.value.filter((cliente) => cliente.empresaId === empresaId.value))
const clienteId = computed(() => clientesDaEmpresa.value[0]?.id || '')
const loading = ref(true)
const error = ref('')
const success = ref('')
const busca = ref('')
const status = ref('')
const formOpen = ref(false)
const detailOpen = ref(false)
const documentsOpen = ref(false)
const documentsLoading = ref(false)
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
const linkToken = ref('')
const linkExpiraEm = ref('')
const linkAdmissaoId = ref('')
const linkPapel = ref<'funcionario' | 'empresa'>('funcionario')
const linkLoading = ref(false)
const emailOpen = ref(false)
const emailDestino = ref('')
let controller: AbortController | undefined
let clientesController: AbortController | undefined
let generation = 0
let documentsGeneration = 0
let searchTimer: ReturnType<typeof setTimeout> | undefined

const terminal = (item: AdmissaoResumo) => item.status === 'CONCLUIDA' || item.status === 'CANCELADA'
const statusSeverity = (codigo: string) => codigo === 'CONCLUIDA' ? 'success' : codigo === 'CANCELADA' ? 'danger' : ['DOCUMENTOS_VALIDADOS', 'AGUARDANDO_EMPREGADOR', 'LIBERADO_PELO_EMPREGADOR'].includes(codigo) ? 'info' : 'warn'

async function load() {
  if (searchTimer) clearTimeout(searchTimer)
  const request = ++generation
  controller?.abort()
  const currentController = new AbortController()
  controller = currentController
  loading.value = true
  error.value = ''
  try {
    const listaAdmissoes = await listarAdmissoes({ empresaId: empresaId.value || undefined, status: status.value || undefined, busca: busca.value.trim() || undefined }, currentController.signal)
    if (request !== generation) return
    admissoes.value = listaAdmissoes
    if (documentsTarget.value) {
      documentsTarget.value = listaAdmissoes.find((item) => item.id === documentsTarget.value?.id) ?? documentsTarget.value
    }
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
  } finally {
    if (!currentController.signal.aborted) clientesCarregados.value = true
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

async function abrirDocumentos(item: AdmissaoResumo) {
  const request = ++documentsGeneration
  documentsTarget.value = null
  documentsOpen.value = true
  documentsLoading.value = true
  error.value = ''
  try {
    const detalhe = await obterAdmissao(item.id)
    if (request === documentsGeneration && documentsOpen.value)
      documentsTarget.value = detalhe
  } catch (cause) {
    if (request === documentsGeneration) error.value = normalizeError(cause).message
  } finally {
    if (request === documentsGeneration) documentsLoading.value = false
  }
}

async function gerarLink(admissaoId: string, papel: 'funcionario' | 'empresa') {
  linkLoading.value = true
  error.value = ''
  try {
    const resultado = papel === 'funcionario'
      ? await gerarLinkFichaAdmissao(admissaoId)
      : await gerarLinkFichaEmpresaAdmissao(admissaoId)
    linkAdmissaoId.value = admissaoId
    linkPapel.value = papel
    linkToken.value = resultado.token
    const rota = papel === 'funcionario' ? 'ficha-admissao' : 'ficha-admissao-empresa'
    linkFicha.value = `${window.location.origin}/${rota}/${resultado.token}`
    linkExpiraEm.value = resultado.expiraEm
    linkOpen.value = true
    await load()
  } catch (cause) { error.value = normalizeError(cause).message }
  finally { linkLoading.value = false }
}

async function enviarLinkEmail() {
  const email = emailDestino.value.trim()
  if (!linkAdmissaoId.value || !linkToken.value || !email) {
    error.value = 'Informe um e-mail para enviar o link ativo.'
    return
  }
  linkLoading.value = true
  error.value = ''
  try {
    await enviarLinkFichaAdmissaoPorEmail(linkAdmissaoId.value, linkPapel.value, email, linkToken.value)
    emailOpen.value = false
    emailDestino.value = ''
    success.value = 'Link da ficha enviado por e-mail.'
  } catch (cause) { error.value = normalizeError(cause).message }
  finally { linkLoading.value = false }
}

async function concluirRevisaoDocumental(admissaoId: string) {
  actionId.value = admissaoId
  error.value = ''
  try {
    await concluirRevisaoDocumentalAdmissao(admissaoId)
    success.value = 'Revisão documental concluída. O empregador já pode receber o link.'
    await load()
  } catch (cause) { error.value = normalizeError(cause).message }
  finally { actionId.value = '' }
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
    if (linkPapel.value === 'funcionario') await revogarLinkFichaAdmissao(linkAdmissaoId.value)
    else await revogarLinkFichaEmpresaAdmissao(linkAdmissaoId.value)
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
  if (acao === 'concluir' && item.status !== 'LIBERADO_PELO_EMPREGADOR') return
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
watch([empresaId, status], () => void load())
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
  <PageHeader title="Admissões" :description="`Funcionários admitidos em ${nomeEmpresa}.`">
    <BaseButton label="Voltar para clientes com admissões" icon="pi pi-arrow-left" variant="text" @click="router.push({ name: 'admissoes' })" />
    <BaseButton label="Nova admissão" icon="pi pi-plus" :disabled="!clienteId || !clientesCarregados" @click="abrirCriacao" />
  </PageHeader>
  <Message v-if="success" severity="success" :closable="true" class="page-message" @close="success = ''">{{ success }}</Message>
  <Message v-if="error" severity="error" :closable="true" class="page-message" @close="error = ''">{{ error }}</Message>
  <Message v-if="clientesCarregados && !clienteId" severity="warn" :closable="false" class="page-message">Não há um cliente ativo vinculado a esta empresa para iniciar uma nova admissão.</Message>

  <BaseCard class="data-card">
    <form class="filters admission-filters" role="search" aria-label="Filtrar admissões" @submit.prevent="load">
      <BaseInput id="admission-search" v-model="busca" label="Buscar funcionário" type="search" placeholder="Nome do funcionário" autocomplete="off" />
      <div class="field"><label for="admission-status-filter">Situação</label><Select input-id="admission-status-filter" v-model="status" :options="statusAdmissaoOptions" option-label="label" option-value="value" /></div>
    </form>
    <div class="list-heading"><h2>Admissões cadastradas</h2><span>{{ loading ? 'Carregando...' : `${admissoes.length} ${admissoes.length === 1 ? 'admissão' : 'admissões'}` }}</span></div>
    <div v-if="loading" class="list-state" aria-busy="true"><i class="pi pi-spinner loading-spinner" /><p>Carregando admissões...</p></div>
    <DataTable v-else :value="admissoes" data-key="id" class="work-table admissions-table" :table-props="{ 'aria-label': 'Lista de admissões' }">
      <Column field="nomeFuncionario" header="Funcionário"><template #body="{ data }"><div class="company-cell"><strong>{{ data.nomeFuncionario }}</strong><span>{{ formatarCpf(data.cpf) || 'CPF não informado' }}</span></div></template></Column>
      <Column field="statusDescricao" header="Situação"><template #body="{ data }"><StatusBadge :label="data.statusDescricao" :severity="statusSeverity(data.status)" /></template></Column>
      <Column header="Vínculo"><template #body="{ data }"><div class="company-cell"><strong>{{ data.funcao || 'Função não informada' }}</strong><span>{{ formatDate(data.dataAdmissaoPrevista) }} · {{ data.salario == null ? 'Salário não informado' : formatCurrency(data.salario) }}</span></div></template></Column>
      <Column header="Ações"><template #body="{ data }"><div class="admission-row-actions">
        <BaseButton label="Documentos" icon="pi pi-upload" variant="text" @click="abrirDocumentos(data)" />
        <BaseButton label="Detalhes" icon="pi pi-eye" variant="text" @click="abrirDetalhe(data)" />
        <BaseButton v-if="!terminal(data)" label="Editar" icon="pi pi-pencil" variant="secondary" @click="abrirEdicao(data)" />
        <BaseButton v-if="!terminal(data) && data.status === 'LIBERADO_PELO_EMPREGADOR'" label="Concluir admissão" icon="pi pi-check" :loading="actionId === data.id" @click="confirmar(data, 'concluir')" />
      </div></template></Column>
      <template #empty><div class="empty-state"><i class="pi pi-id-card" /><h3>Nenhuma admissão encontrada</h3><p>Crie uma admissão manual para iniciar o cadastro do funcionário.</p><BaseButton label="Nova admissão" icon="pi pi-plus" @click="abrirCriacao" /></div></template>
    </DataTable>
  </BaseCard>

  <AdmissaoFormDialog v-model:visible="formOpen" :clientes="clientesDaEmpresa" :admissao="selected" :cliente-inicial-id="clienteId" :saving="saving" :error="formError" @salvar="salvar" />
  <AdmissaoDetalheDialog v-model:visible="detailOpen" :admissao="selected" :loading="detailLoading" @editar="abrirEdicao()" @concluir="selected && confirmar(selected, 'concluir')" @cancelar="selected && confirmar(selected, 'cancelar')" />
  <AdmissaoDocumentosDialog v-if="documentsOpen" v-model:visible="documentsOpen" :admissao="documentsTarget" :loading="documentsLoading" :link-loading="!!actionId || linkLoading" @gerar-link="gerarLink" @concluir-revisao="concluirRevisaoDocumental" />

  <Dialog v-model:visible="linkOpen" modal :header="linkPapel === 'funcionario' ? 'Ficha digital do admitido' : 'Ficha digital do empregador'" class="workflow-dialog" :style="{ width: 'min(620px, 94vw)' }">
    <div class="admission-link-dialog">
      <p>{{ linkPapel === 'funcionario' ? 'Compartilhe este link somente com o admitido. Ele preencherá os dados pessoais e enviará os documentos.' : 'Compartilhe este link somente com o empregador. O admitido já concluiu a etapa anterior.' }} Um novo link invalida o anterior.</p>
      <div class="admission-link-value"><InputText :model-value="linkFicha" readonly /><BaseButton icon="pi pi-copy" title="Copiar link" aria-label="Copiar link" @click="copiarLink" /></div>
      <small>Expira em {{ formatDate(linkExpiraEm) }}</small>
    </div>
    <template #footer><BaseButton label="Revogar link" icon="pi pi-ban" variant="text" :loading="linkLoading" @click="revogarLink" /><BaseButton label="Enviar por e-mail" icon="pi pi-envelope" variant="secondary" :disabled="linkLoading" @click="emailOpen = true" /><BaseButton label="Fechar" variant="secondary" @click="linkOpen = false" /></template>
  </Dialog>

  <Dialog v-model:visible="emailOpen" modal header="Enviar link por e-mail" class="workflow-dialog" :style="{ width: 'min(480px, 94vw)' }">
    <form class="dialog-form" @submit.prevent="enviarLinkEmail">
      <p>O endereço receberá o link ativo da ficha do {{ linkPapel === 'funcionario' ? 'admitido' : 'empregador' }}.</p>
      <div class="field"><label for="admission-link-email">E-mail do destinatário</label><InputText id="admission-link-email" v-model="emailDestino" type="email" autocomplete="email" required /></div>
    </form>
    <template #footer><BaseButton label="Cancelar" variant="text" :disabled="linkLoading" @click="emailOpen = false" /><BaseButton label="Enviar link" icon="pi pi-send" :loading="linkLoading" :disabled="!emailDestino.trim()" @click="enviarLinkEmail" /></template>
  </Dialog>

  <Dialog v-model:visible="confirmOpen" modal :header="confirmAction === 'concluir' ? 'Concluir admissão' : 'Cancelar admissão'" class="workflow-dialog" :style="{ width: 'min(480px, 94vw)' }">
    <p v-if="confirmAction === 'concluir'" class="confirmation-copy">Ao concluir, o funcionário será vinculado à empresa e entrará na contagem de funcionários. Confirme que CPF, função e data de admissão estão preenchidos.</p>
    <p v-else class="confirmation-copy">A admissão será cancelada e não poderá mais ser editada ou concluída.</p>
    <template #footer><BaseButton label="Voltar" variant="text" @click="confirmOpen = false" /><BaseButton :label="confirmAction === 'concluir' ? 'Concluir admissão' : 'Cancelar admissão'" :icon="confirmAction === 'concluir' ? 'pi pi-check' : 'pi pi-ban'" :loading="!!actionId" @click="executarAcao" /></template>
  </Dialog>
</template>

<style scoped>
.admissions-table :deep(.p-datatable-table) { width: 100%; table-layout: fixed; }
.admissions-table :deep(.p-datatable-thead > tr > th),
.admissions-table :deep(.p-datatable-tbody > tr > td) { width: 25%; }
.admission-row-actions { display: flex; flex-direction: column; align-items: flex-start; gap: 2px; }
.admission-row-actions :deep(.base-button.p-button) { min-height: 32px; padding: 4px 8px; font-size: 13px; font-weight: 600; }
</style>
