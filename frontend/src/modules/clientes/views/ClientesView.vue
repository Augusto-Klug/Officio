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
import InputNumber from 'primevue/inputnumber'
import BaseButton from '../../../components/base/BaseButton.vue'
import BaseCard from '../../../components/base/BaseCard.vue'
import BaseInput from '../../../components/base/BaseInput.vue'
import EnderecoCadastro from '../../../components/shared/EnderecoCadastro.vue'
import { novoEndereco, type EnderecoCadastro as EnderecoForm } from '../../../components/shared/endereco'
import PageHeader from '../../../components/layout/PageHeader.vue'
import StatusBadge from '../../../components/shared/StatusBadge.vue'
import { normalizeError } from '../../../api/errors'
import { formatCurrency, formatDate } from '../../../utils/format'
import { formatarCpf, formatarDocumentoFiscal, formatarTelefone, somenteDigitos } from '../../../utils/inputMasks'
import { atualizarCliente, listarClientes, normalizarRegimeTributario, obterCliente, vincularCodigoSci, type AtualizarClientePayload } from '../services/clientes.service'
import type { Cliente, ClienteDetalhe } from '../types/cliente'
import { regimeTributarioOptions } from '../../prospectos/types/prospecto'
import { listarTiposRelacaoSocietaria } from '../../prospectos/services/prospectos.service'
import { usePageRefresh, type PageRefreshReason } from '../../../composables/usePageRefresh'
import { useRealtimePageRefresh } from '../../../composables/useRealtimePageRefresh'

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
const editOpen = ref(false)
const cancelContractsOpen = ref(false)
const editLoading = ref(false)
const editOriginal = ref('')
const relationTypes = ref<{ id: string; descricao: string }[]>([])
const editForm = ref({
  razaoSocial: '', nomeFantasia: '', cnpj: '', cpf: '', regimeTributario: null as number | null,
  email: '', telefone: '', whatsapp: '', inscricaoEstadual: '', inscricaoMunicipal: '', tipoEmpresa: '',
  observacoes: '', endereco: novoEndereco(), socios: [] as SocioEdicao[],
})
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
const sociosAtuais = computed(() => detalhe.value?.socios.filter(socio => socio.ativo) ?? [])
const sociosHistoricos = computed(() => detalhe.value?.socios.filter(socio => !socio.ativo) ?? [])
const contratosCancelaveis = computed(() => detalhe.value?.contratos.filter(contrato => ['Rascunho', 'AguardandoAssinatura'].includes(contrato.status)) ?? [])
const editChanged = computed(() => editOriginal.value !== JSON.stringify(editForm.value))

interface SocioEdicao {
  id?: string
  nome: string
  cpf: string
  participacao: number | null
  profissao: string
  tipoRelacaoId: string | null
  endereco: EnderecoForm
}

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

const { refreshing, refreshNow, refreshAutomatically } = usePageRefresh(refreshData, { busy: loading })
useRealtimePageRefresh('cliente', refreshAutomatically, computed(() => loading.value || refreshing.value))

async function abrirDetalhe(item: Cliente) {
  detalheOpen.value = true
  detalheLoading.value = true
  dialogError.value = ''
  detalhe.value = null
  try { detalhe.value = await obterCliente(item.id) }
  catch (cause) { dialogError.value = normalizeError(cause).message }
  finally { detalheLoading.value = false }
}

function criarSocioEdicao(socio?: ClienteDetalhe['socios'][number]): SocioEdicao {
  return {
    id: socio?.id, nome: socio?.nome ?? '', cpf: socio?.cpf ?? '', participacao: socio?.participacao ?? null,
    profissao: socio?.profissao ?? '', tipoRelacaoId: socio?.tipoRelacaoId ?? null,
    endereco: {
      cep: socio?.cep ?? '', logradouro: socio?.logradouro ?? '', numero: socio?.numero ?? '',
      complemento: socio?.complemento ?? '', bairro: socio?.bairro ?? '', cidade: socio?.cidade ?? '',
      uf: socio?.uf ?? '', municipioCodigoIbge: socio?.municipioCodigoIbge ?? '',
    },
  }
}

async function abrirEdicao(item: Cliente | ClienteDetalhe) {
  editOpen.value = true
  editLoading.value = true
  dialogError.value = ''
  try {
    const dados = await obterCliente(item.id)
    detalhe.value = dados
    relationTypes.value = await listarTiposRelacaoSocietaria()
    editForm.value = {
      razaoSocial: dados.razaoSocial, nomeFantasia: dados.nomeFantasia ?? '', cnpj: dados.cnpj ?? '', cpf: dados.cpf ?? '',
      regimeTributario: normalizarRegimeTributario(dados.regimeTributario),
      email: dados.email ?? '', telefone: dados.telefone ?? '', whatsapp: dados.whatsapp ?? '',
      inscricaoEstadual: dados.inscricaoEstadual ?? '', inscricaoMunicipal: dados.inscricaoMunicipal ?? '',
      tipoEmpresa: dados.tipoEmpresa ?? '', observacoes: dados.observacoes ?? '',
      endereco: {
        cep: dados.cep ?? '', logradouro: dados.logradouro ?? '', numero: dados.numero ?? '',
        complemento: dados.complemento ?? '', bairro: dados.bairro ?? '', cidade: dados.cidade ?? '',
        uf: dados.uf ?? '', municipioCodigoIbge: dados.municipioCodigoIbge ?? '',
      },
      socios: dados.socios.filter(socio => socio.ativo).map(criarSocioEdicao),
    }
    editOriginal.value = JSON.stringify(editForm.value)
  } catch (cause) { dialogError.value = normalizeError(cause).message }
  finally { editLoading.value = false }
}

function adicionarSocio() { editForm.value.socios.push(criarSocioEdicao()) }
function removerSocio(index: number) { editForm.value.socios.splice(index, 1) }
function aplicarMascaraSocioCpf(index: number, value: string | undefined) {
  editForm.value.socios[index].cpf = formatarCpf(value)
}
function aplicarMascaraTelefone(campo: 'telefone' | 'whatsapp', value: string | undefined) {
  editForm.value[campo] = formatarTelefone(value)
}

async function salvarCliente(confirmarCancelamento = false) {
  if (!detalhe.value) return
  if (!editForm.value.razaoSocial.trim()) { dialogError.value = 'Informe a razão social.'; return }
  if (editForm.value.socios.some(socio => !socio.nome.trim() || socio.participacao === null || socio.participacao < 0 || socio.participacao > 100)) {
    dialogError.value = 'Informe nome e participação entre 0 e 100 para cada sócio.'
    return
  }
  if (!confirmarCancelamento && editChanged.value && contratosCancelaveis.value.length > 0) {
    cancelContractsOpen.value = true
    return
  }
  saving.value = true
  dialogError.value = ''
  try {
    const payload: AtualizarClientePayload = {
      ...editForm.value,
      cnpj: somenteDigitos(editForm.value.cnpj) || null,
      cpf: somenteDigitos(editForm.value.cpf) || null,
      telefone: somenteDigitos(editForm.value.telefone) || null,
      whatsapp: somenteDigitos(editForm.value.whatsapp) || null,
      inscricaoEstadual: editForm.value.inscricaoEstadual || null,
      inscricaoMunicipal: editForm.value.inscricaoMunicipal || null,
      nomeFantasia: editForm.value.nomeFantasia || null,
      tipoEmpresa: editForm.value.tipoEmpresa || null,
      email: editForm.value.email || null,
      observacoes: editForm.value.observacoes || null,
      socios: editForm.value.socios.map(socio => ({
        id: socio.id, nome: socio.nome.trim(), cpf: somenteDigitos(socio.cpf) || null,
        participacao: socio.participacao, profissao: socio.profissao || null,
        tipoRelacaoId: socio.tipoRelacaoId,
        endereco: socio.endereco,
      })),
      confirmarCancelamentoContratos: confirmarCancelamento,
      contratosConfirmados: confirmarCancelamento ? contratosCancelaveis.value.map(contrato => contrato.id) : [],
    }
    await atualizarCliente(detalhe.value.id, payload)
    editOpen.value = false
    cancelContractsOpen.value = false
    success.value = 'Dados do cliente atualizados.'
    await load()
    if (detalheOpen.value) detalhe.value = await obterCliente(detalhe.value.id)
  } catch (cause) { dialogError.value = normalizeError(cause).message }
  finally { saving.value = false }
}

function abrirSci(item: Cliente | ClienteDetalhe) {
  if (!detalhe.value || detalhe.value.id !== item.id) {
    detalhe.value = { ...item, whatsapp: null, cep: null, logradouro: null, numero: null, bairro: null, observacoes: null, prospectOrigemId: null, inicioRelacionamentoEm: null, socios: [], contratos: [], documentos: [], inscricaoEstadual: null, inscricaoMunicipal: null, tipoEmpresa: null, complemento: null, municipioCodigoIbge: null }
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
  const cliente = clientes.value.find((item) => item.id === clienteId)
  if (!cliente) return
  void router.push({
    name: 'admissoes-empresa',
    params: { empresaId: cliente.empresaId },
    query: { empresaNome: cliente.razaoSocial },
  })
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
      <Column field="razaoSocial" header="Cliente"><template #body="{ data }"><div class="company-cell"><strong>{{ data.razaoSocial }}</strong><span>{{ formatarDocumentoFiscal(data.cnpj || data.cpf) || 'CNPJ não informado' }}</span></div></template></Column>
      <Column field="ativo" header="Situação"><template #body="{ data }"><StatusBadge :label="data.ativo ? 'Ativo' : 'Inativo'" :severity="data.ativo ? 'success' : 'secondary'" /></template></Column>
      <Column field="sciCodigo" header="SCI"><template #body="{ data }"><span class="numeric">{{ data.sciCodigo || 'Não vinculado' }}</span></template></Column>
      <Column header="Relacionamentos"><template #body="{ data }"><span class="relation-counts">{{ data.quantidadeContratos }} contratos · {{ data.quantidadeDocumentos }} documentos</span></template></Column>
      <Column header="Ações"><template #body="{ data }"><div class="row-actions"><BaseButton label="Detalhes" icon="pi pi-eye" variant="text" @click="abrirDetalhe(data)" /><BaseButton label="Editar" icon="pi pi-pencil" variant="text" @click="abrirEdicao(data)" /><BaseButton label="Admissões" icon="pi pi-id-card" variant="secondary" @click="abrirAdmissoes(data.id)" /><BaseButton label="Vincular SCI" icon="pi pi-link" variant="secondary" @click="abrirSci(data)" /></div></template></Column>
      <template #empty><div class="empty-state"><i class="pi pi-building" /><h3>Nenhum cliente encontrado</h3><p>Clientes são criados quando o fluxo comercial é concluído.</p></div></template>
    </DataTable>
  </BaseCard>

  <Dialog v-model:visible="detalheOpen" modal header="Detalhes do cliente" class="workflow-dialog client-detail-dialog" :style="{ width: 'min(860px, 95vw)' }">
    <div v-if="detalheLoading" class="list-state compact-state" aria-busy="true"><i class="pi pi-spinner loading-spinner" /><p>Carregando detalhes…</p></div>
    <Message v-else-if="dialogError" severity="error" :closable="false">{{ dialogError }}</Message>
    <div v-else-if="detalhe" class="client-detail">
      <div class="detail-heading"><div><h2>{{ detalhe.razaoSocial }}</h2><p>{{ detalhe.nomeFantasia || formatarDocumentoFiscal(detalhe.cnpj || detalhe.cpf) || 'CNPJ não informado' }}</p></div><StatusBadge :label="detalhe.ativo ? 'Ativo' : 'Inativo'" :severity="detalhe.ativo ? 'success' : 'secondary'" /></div>
      <dl class="detail-grid">
        <div><dt>Regime tributário</dt><dd>{{ detalhe.regimeTributario || 'Não informado' }}</dd></div>
        <div><dt>Código SCI</dt><dd>{{ detalhe.sciCodigo || 'Não vinculado' }}</dd></div>
        <div><dt>Contato</dt><dd>{{ detalhe.email || formatarTelefone(detalhe.telefone) || 'Não informado' }}</dd></div>
        <div><dt>Localização</dt><dd>{{ localizacao || 'Não informada' }}</dd></div>
        <div><dt>Relacionamento desde</dt><dd>{{ formatDate(detalhe.inicioRelacionamentoEm) }}</dd></div>
      </dl>
      <section class="detail-section"><h3>Sócios atuais <span>{{ sociosAtuais.length }}</span></h3><div v-if="sociosAtuais.length" class="detail-list"><div v-for="socio in sociosAtuais" :key="socio.id"><strong>{{ socio.nome }}</strong><span>{{ socio.participacao }}% · {{ formatarCpf(socio.cpf) || 'CPF não informado' }}<template v-if="socio.tipoRelacao"> · {{ socio.tipoRelacao }}</template></span></div></div><p v-else class="muted">Nenhum sócio atual vinculado.</p></section>
      <section v-if="sociosHistoricos.length" class="detail-section"><h3>Histórico societário <span>{{ sociosHistoricos.length }}</span></h3><div class="detail-list"><div v-for="socio in sociosHistoricos" :key="socio.id"><strong>{{ socio.nome }} <StatusBadge label="Encerrado" severity="secondary" /></strong><span>{{ socio.participacao }}% · {{ formatarCpf(socio.cpf) || 'CPF não informado' }} · vigente até {{ formatDate(socio.vigenteAte) }}</span></div></div></section>
      <section class="detail-section"><h3>Contratos <span>{{ detalhe.contratos.length }}</span></h3><div v-if="detalhe.contratos.length" class="detail-list"><div v-for="contrato in detalhe.contratos" :key="contrato.id"><strong>{{ contrato.numero }} · {{ contrato.status }}</strong><span>{{ formatCurrency(contrato.valorTotal) }} · início em {{ formatDate(contrato.dataInicio) }}</span></div></div><p v-else class="muted">Nenhum contrato vinculado.</p></section>
      <section class="detail-section"><h3>Documentos <span>{{ detalhe.documentos.length }}</span></h3><div v-if="detalhe.documentos.length" class="detail-list"><div v-for="documento in detalhe.documentos" :key="documento.id"><strong>{{ documento.nomeArquivo }}</strong><span>{{ documento.tipo }} · {{ documento.legivel ? 'legível' : 'requer revisão' }}</span></div></div><p v-else class="muted">Nenhum documento vinculado.</p></section>
    </div>
    <template #footer><BaseButton label="Fechar" variant="text" @click="detalheOpen = false" /><BaseButton v-if="detalhe" label="Admissões" icon="pi pi-id-card" variant="secondary" @click="abrirAdmissoes(detalhe.id)" /><BaseButton v-if="detalhe" label="Vincular SCI" icon="pi pi-link" variant="secondary" @click="abrirSci(detalhe)" /></template>
  </Dialog>

  <Dialog v-model:visible="editOpen" modal header="Editar cliente" class="workflow-dialog client-edit-dialog" :style="{ width: 'min(940px, 96vw)' }" :closable="!saving">
    <div v-if="editLoading" class="list-state compact-state" aria-busy="true"><i class="pi pi-spinner loading-spinner" /><p>Carregando cadastro…</p></div>
    <form v-else-if="detalhe" id="client-edit-form" class="client-edit-form" @submit.prevent="salvarCliente()">
      <Message v-if="dialogError" severity="error" :closable="false">{{ dialogError }}</Message>
      <section class="edit-section">
        <h3>Dados da empresa</h3>
        <div class="edit-grid">
          <BaseInput id="client-edit-razao" v-model="editForm.razaoSocial" label="Razão social" maxlength="300" required />
          <BaseInput id="client-edit-fantasia" v-model="editForm.nomeFantasia" label="Nome fantasia" maxlength="200" />
          <BaseInput id="client-edit-cnpj" :model-value="formatarDocumentoFiscal(editForm.cnpj)" label="CNPJ" disabled />
          <div class="field"><label for="client-edit-regime">Regime tributário</label><Select input-id="client-edit-regime" v-model="editForm.regimeTributario" :options="[...regimeTributarioOptions]" option-label="label" option-value="value" placeholder="Selecione" show-clear /></div>
          <BaseInput id="client-edit-email" v-model="editForm.email" label="E-mail" type="email" maxlength="254" />
          <BaseInput id="client-edit-phone" :model-value="formatarTelefone(editForm.telefone)" label="Telefone" numeric-only inputmode="numeric" maxlength="16" @update:model-value="value => aplicarMascaraTelefone('telefone', value)" />
          <BaseInput id="client-edit-whatsapp" :model-value="formatarTelefone(editForm.whatsapp)" label="WhatsApp" numeric-only inputmode="numeric" maxlength="16" @update:model-value="value => aplicarMascaraTelefone('whatsapp', value)" />
          <BaseInput id="client-edit-ie" v-model="editForm.inscricaoEstadual" label="Inscrição estadual" maxlength="30" />
          <BaseInput id="client-edit-im" v-model="editForm.inscricaoMunicipal" label="Inscrição municipal" maxlength="30" />
          <BaseInput id="client-edit-type" v-model="editForm.tipoEmpresa" label="Tipo de empresa" maxlength="80" />
        </div>
        <EnderecoCadastro v-model="editForm.endereco" prefix="client-edit-address" />
        <BaseInput id="client-edit-notes" v-model="editForm.observacoes" label="Observações" maxlength="2000" />
      </section>
      <section class="edit-section">
        <div class="edit-section-heading"><h3>Quadro societário atual</h3><BaseButton label="Adicionar sócio" icon="pi pi-plus" variant="secondary" type="button" @click="adicionarSocio" /></div>
        <p class="muted">Alterações encerram a vigência anterior e registram uma nova versão. Os contratos assinados preservam o representante usado na assinatura.</p>
        <div v-if="!editForm.socios.length" class="empty-inline">Nenhum sócio atual. Use “Adicionar sócio” para incluir um.</div>
        <div v-for="(socio, index) in editForm.socios" :key="socio.id || `new-${index}`" class="socio-edit-row">
          <div class="edit-grid socio-fields">
            <BaseInput :id="`client-socio-name-${index}`" v-model="socio.nome" label="Nome" maxlength="200" required />
            <BaseInput :id="`client-socio-cpf-${index}`" :model-value="formatarCpf(socio.cpf)" label="CPF" numeric-only inputmode="numeric" maxlength="14" @update:model-value="value => aplicarMascaraSocioCpf(index, value)" />
            <div class="field"><label :for="`client-socio-participation-${index}`">Participação (%)</label><InputNumber :input-id="`client-socio-participation-${index}`" v-model="socio.participacao" :min="0" :max="100" :min-fraction-digits="0" :max-fraction-digits="2" fluid /></div>
            <BaseInput :id="`client-socio-profession-${index}`" v-model="socio.profissao" label="Profissão" maxlength="120" />
            <div class="field"><label :for="`client-socio-relation-${index}`">Relação societária</label><Select :input-id="`client-socio-relation-${index}`" v-model="socio.tipoRelacaoId" :options="relationTypes" option-label="descricao" option-value="id" placeholder="Selecione" show-clear /></div>
          </div>
          <details class="socio-address"><summary>Endereço do sócio</summary><EnderecoCadastro v-model="socio.endereco" :prefix="`client-socio-address-${index}`" /></details>
          <BaseButton label="Remover sócio" icon="pi pi-trash" variant="text" type="button" @click="removerSocio(index)" />
        </div>
      </section>
    </form>
    <template #footer><BaseButton label="Cancelar" variant="text" :disabled="saving" @click="editOpen = false" /><BaseButton label="Salvar alterações" icon="pi pi-check" type="submit" form="client-edit-form" :loading="saving" /></template>
  </Dialog>

  <Dialog v-model:visible="cancelContractsOpen" modal header="Contratos pendentes encontrados" class="workflow-dialog" :style="{ width: 'min(560px, 94vw)' }">
    <Message severity="warn" :closable="false">Ao salvar as alterações, os contratos abaixo serão cancelados. Contratos assinados ou ativos não serão modificados.</Message>
    <ul class="pending-contract-list"><li v-for="contrato in contratosCancelaveis" :key="contrato.id"><strong>{{ contrato.numero }}</strong><span>{{ contrato.status === 'Rascunho' ? 'Rascunho' : 'Aguardando assinatura' }}</span></li></ul>
    <template #footer><BaseButton label="Voltar à edição" variant="text" @click="cancelContractsOpen = false" /><BaseButton label="Confirmar e salvar" icon="pi pi-ban" :loading="saving" @click="salvarCliente(true)" /></template>
  </Dialog>

  <Dialog v-model:visible="sciOpen" modal header="Vincular código SCI" class="workflow-dialog" :style="{ width: 'min(440px, 94vw)' }">
    <form id="sci-form" class="dialog-form" @submit.prevent="salvarSci">
      <Message v-if="dialogError" severity="error" :closable="false">{{ dialogError }}</Message>
      <div class="field"><label for="sci-code">Código do cliente no SCI</label><InputText id="sci-code" v-model="sciCodigo" autocomplete="off" /></div>
    </form>
    <template #footer><BaseButton label="Cancelar" variant="text" @click="sciOpen = false" /><BaseButton label="Vincular" icon="pi pi-link" type="submit" form="sci-form" :loading="saving" /></template>
  </Dialog>
</template>

<style scoped>
.client-edit-form { display: grid; gap: 1.5rem; max-height: 70vh; overflow-y: auto; padding: 0 0.2rem; }
.edit-section { display: grid; gap: 1rem; }
.edit-section h3 { margin: 0; font-size: 1rem; }
.edit-grid { display: grid; grid-template-columns: repeat(2, minmax(0, 1fr)); gap: 1rem; }
.edit-section-heading { display: flex; align-items: center; justify-content: space-between; gap: 1rem; }
.socio-edit-row { display: grid; gap: 0.85rem; padding: 1rem 0; border-top: 1px solid var(--surface-border); }
.socio-fields { grid-template-columns: repeat(2, minmax(0, 1fr)); }
.socio-address { display: grid; gap: 1rem; }
.socio-address summary { cursor: pointer; font-weight: 600; }
.empty-inline { padding: 0.75rem 0; color: var(--text-color-secondary); }
.pending-contract-list { display: grid; gap: 0.6rem; padding-left: 1.25rem; }
.pending-contract-list li { display: flex; justify-content: space-between; gap: 1rem; }
@media (max-width: 640px) { .edit-grid, .socio-fields { grid-template-columns: minmax(0, 1fr); } }
</style>
