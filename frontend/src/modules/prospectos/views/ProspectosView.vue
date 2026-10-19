<script setup lang="ts">
import { computed, onBeforeUnmount, ref, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import axios from 'axios'
import DataTable from 'primevue/datatable'
import Column from 'primevue/column'
import Paginator, { type PageState } from 'primevue/paginator'
import Message from 'primevue/message'
import Dialog from 'primevue/dialog'
import Button from 'primevue/button'
import BaseButton from '../../../components/base/BaseButton.vue'
import BaseInput from '../../../components/base/BaseInput.vue'
import BaseSelect from '../../../components/base/BaseSelect.vue'
import BaseCard from '../../../components/base/BaseCard.vue'
import PageHeader from '../../../components/layout/PageHeader.vue'
import ProspectoStatus from '../components/ProspectoStatus.vue'
import ProspectoCadastroDialog from '../components/ProspectoCadastroDialog.vue'
import { excluirProspecto, listarProspectos } from '../services/prospectos.service'
import { prospectoNaEtapaInicial, statusOptions, type Prospecto, type ProspectosPagina } from '../types/prospecto'
import { normalizeError } from '../../../api/errors'
import { formatCnpj, formatDate } from '../../../utils/format'
import { usePageRefresh, type PageRefreshReason } from '../../../composables/usePageRefresh'
import { useRealtimePageRefresh } from '../../../composables/useRealtimePageRefresh'

const route = useRoute()
const router = useRouter()
const busca = ref(typeof route.query.busca === 'string' ? route.query.busca : '')
const status = ref<string | null>(typeof route.query.status === 'string' ? route.query.status : null)
const loading = ref(true)
const error = ref('')
const result = ref<ProspectosPagina | null>(null)
const cadastroOpen = ref(false)
const prospectoEdicaoId = ref<string | null>(null)
const exclusaoOpen = ref(false)
const prospectoExclusao = ref<Prospecto | null>(null)
const excluindo = ref(false)
const exclusaoErro = ref('')
const sucesso = ref('')
const pagina = computed(() => Math.max(1, Number.parseInt(String(route.query.pagina || '1'), 10) || 1))
const filtered = computed(() => !!(route.query.busca || route.query.status || route.query.responsavelId))
let controller: AbortController | undefined
let timer: ReturnType<typeof setTimeout> | undefined
let generation = 0

async function load(options: { background?: boolean; reportErrors?: boolean } = {}) {
  const { background = false, reportErrors = true } = options
  const request = ++generation
  controller?.abort()
  controller = new AbortController()
  if (!background) loading.value = true
  if (reportErrors) error.value = ''
  try {
    const data = await listarProspectos({
      pagina: pagina.value, tamanhoPagina: 20,
      busca: typeof route.query.busca === 'string' ? route.query.busca.trim() || undefined : undefined,
      status: typeof route.query.status === 'string' ? route.query.status || undefined : undefined,
      responsavelId: typeof route.query.responsavelId === 'string' ? route.query.responsavelId || undefined : undefined,
    }, controller.signal)
    if (request !== generation) return
    const lastPage = Math.max(1, data.totalPaginas)
    if (pagina.value > lastPage) {
      await router.replace({ query: { ...route.query, pagina: String(lastPage) } })
      return
    }
    result.value = data
    error.value = ''
  } catch (cause) {
    if (request === generation && reportErrors && !axios.isCancel(cause)) error.value = normalizeError(cause).message
  } finally { if (request === generation && !background) loading.value = false }
}

async function refreshData(reason: PageRefreshReason) {
  await load({ background: true, reportErrors: reason === 'manual' })
}

const { refreshing, refreshNow, refreshAutomatically } = usePageRefresh(refreshData, { busy: loading })
useRealtimePageRefresh('prospecto', refreshAutomatically, computed(() => loading.value || refreshing.value))
function applyFilters() {
  clearTimeout(timer)
  void router.replace({ query: { ...route.query, pagina: undefined, busca: busca.value.trim() || undefined, status: status.value || undefined } })
}
function clearFilters() {
  clearTimeout(timer)
  busca.value = ''
  status.value = null
  void router.replace({ query: {} })
}
function changePage(event: PageState) {
  void router.push({ query: { ...route.query, pagina: String(event.page + 1) } })
}
function scheduleSearch() {
  clearTimeout(timer)
  timer = setTimeout(applyFilters, 350)
}
function prospectoCriado() {
  cadastroOpen.value = false
  sucesso.value = 'Prospecto cadastrado.'
  void router.replace({ query: {} }).then(() => load())
}
function abrirCadastro() {
  prospectoEdicaoId.value = null
  cadastroOpen.value = true
}
function abrirEdicao(prospecto: Prospecto) {
  prospectoEdicaoId.value = prospecto.id
  cadastroOpen.value = true
}
function prospectoAtualizado() {
  cadastroOpen.value = false
  sucesso.value = 'Prospecto atualizado.'
  void load()
}
function solicitarExclusao(prospecto: Prospecto) {
  prospectoExclusao.value = prospecto
  exclusaoErro.value = ''
  exclusaoOpen.value = true
}
async function confirmarExclusao() {
  if (!prospectoExclusao.value || excluindo.value) return
  excluindo.value = true
  exclusaoErro.value = ''
  try {
    await excluirProspecto(prospectoExclusao.value.id)
    exclusaoOpen.value = false
    sucesso.value = 'Prospecto excluído.'
    await load()
  } catch (cause) {
    exclusaoErro.value = normalizeError(cause).message
  } finally {
    excluindo.value = false
  }
}
function abrirOrcamentos(id: string, nome: string) {
  void router.push({ name: 'orcamentos-prospecto', params: { prospectoId: id }, query: { nome } })
}
watch(() => route.query, () => {
  clearTimeout(timer)
  busca.value = typeof route.query.busca === 'string' ? route.query.busca : ''
  status.value = typeof route.query.status === 'string' ? route.query.status : null
  void load()
}, { immediate: true })
onBeforeUnmount(() => { ++generation; controller?.abort(); clearTimeout(timer) })
</script>

<template>
  <PageHeader title="Prospectos" description="Acompanhe as oportunidades comerciais do seu escritório.">
    <BaseButton label="Atualizar" icon="pi pi-refresh" variant="secondary" :loading="refreshing" @click="refreshNow" />
    <BaseButton label="Novo prospecto" icon="pi pi-plus" @click="abrirCadastro" />
  </PageHeader>

  <Message v-if="sucesso" severity="success" :closable="true" aria-live="polite" @close="sucesso = ''">{{ sucesso }}</Message>
  <BaseCard class="prospects-card">
    <form class="filters" role="search" aria-label="Filtrar prospectos" @submit.prevent="applyFilters">
      <BaseInput id="busca" v-model="busca" label="Buscar prospecto" name="busca" type="search" autocomplete="off" placeholder="Razão social ou CNPJ…" @update:model-value="scheduleSearch" />
      <BaseSelect id="status" v-model="status" label="Status" :options="statusOptions" placeholder="Todos os status" @update:model-value="applyFilters" />
      <BaseButton v-if="filtered || busca || status" label="Limpar filtros" variant="text" @click="clearFilters" />
    </form>

    <div class="list-heading"><h2>Lista de prospectos</h2><span role="status" aria-live="polite">{{ loading ? 'Carregando…' : result ? `${result.totalRegistros.toLocaleString('pt-BR')} ${result.totalRegistros === 1 ? 'prospecto' : 'prospectos'}` : 'Consulta indisponível' }}</span></div>

    <div v-if="error" class="list-state"><Message severity="error" :closable="false" role="alert">{{ error }}</Message><BaseButton label="Tentar novamente" icon="pi pi-refresh" variant="secondary" @click="() => load()" /></div>
    <div v-else-if="loading" class="list-state" aria-busy="true"><i class="pi pi-spinner loading-spinner" aria-hidden="true" /><p role="status">Carregando prospectos…</p></div>
    <template v-else-if="result">
      <DataTable :value="result.dados" data-key="id" class="prospects-table" :table-props="{ 'aria-label': 'Lista de prospectos' }">
        <Column field="razaoSocial" header="Razão social / CNPJ"><template #body="{ data }"><div class="company-cell"><strong>{{ data.razaoSocial }}</strong><span>{{ formatCnpj(data.cnpj) }}</span></div></template></Column>
        <Column field="status" header="Status"><template #body="{ data }"><ProspectoStatus :status="data.status" /></template></Column>
        <Column field="responsavel" header="Responsável"><template #body="{ data }"><span :class="{ muted: !data.responsavel }">{{ data.responsavel || 'Não atribuído' }}</span></template></Column>
        <Column field="criadoEm" header="Criado em"><template #body="{ data }"><span class="numeric">{{ formatDate(data.criadoEm) }}</span></template></Column>
        <Column header="Ações"><template #body="{ data }"><div class="prospect-actions">
          <BaseButton label="Orçamentos" icon="pi pi-file-edit" variant="text" @click="abrirOrcamentos(data.id, data.razaoSocial)" />
          <template v-if="prospectoNaEtapaInicial(data)">
            <BaseButton label="Editar" icon="pi pi-pencil" variant="text" :aria-label="`Editar ${data.razaoSocial}`" @click="abrirEdicao(data)" />
            <BaseButton label="Excluir" icon="pi pi-trash" variant="text" :aria-label="`Excluir ${data.razaoSocial}`" @click="solicitarExclusao(data)" />
          </template>
        </div></template></Column>
        <template #empty><div class="empty-state"><i class="pi pi-users" aria-hidden="true" /><h3>{{ filtered ? 'Nenhum resultado encontrado' : 'Nenhum prospecto por aqui' }}</h3><p>{{ filtered ? 'Tente outra busca ou remova os filtros aplicados.' : 'Os prospectos cadastrados no escritório aparecerão nesta lista.' }}</p><BaseButton v-if="filtered" label="Limpar filtros" variant="secondary" @click="clearFilters" /></div></template>
      </DataTable>
      <div v-if="result.totalRegistros > 0" class="pagination-footer">
        <span>{{ ((result.pagina - 1) * result.tamanhoPagina + 1).toLocaleString('pt-BR') }}–{{ Math.min(result.pagina * result.tamanhoPagina, result.totalRegistros).toLocaleString('pt-BR') }} de {{ result.totalRegistros.toLocaleString('pt-BR') }}</span>
        <Paginator :first="(pagina - 1) * 20" :rows="20" :total-records="result.totalRegistros" :page-link-size="3" template="PrevPageLink PageLinks NextPageLink" @page="changePage" />
      </div>
    </template>
  </BaseCard>
  <p class="list-footnote">20 prospectos por página</p>
  <ProspectoCadastroDialog v-model:visible="cadastroOpen" :prospecto-id="prospectoEdicaoId" @criado="prospectoCriado" @atualizado="prospectoAtualizado" />
  <Dialog v-model:visible="exclusaoOpen" modal header="Excluir prospecto" :style="{ width: 'min(460px, calc(100vw - 24px))' }"
    :closable="!excluindo" :close-on-escape="!excluindo" :draggable="false">
    <Message v-if="exclusaoErro" severity="error" :closable="false" role="alert">{{ exclusaoErro }}</Message>
    <p class="confirmation-copy">Excluir <strong>{{ prospectoExclusao?.razaoSocial }}</strong>? O cadastro e os sócios serão removidos. Esta ação não pode ser desfeita.</p>
    <template #footer>
      <BaseButton label="Cancelar" variant="text" :disabled="excluindo" @click="exclusaoOpen = false" />
      <Button label="Excluir prospecto" icon="pi pi-trash" severity="danger" :loading="excluindo" :disabled="excluindo" @click="confirmarExclusao" />
    </template>
  </Dialog>
</template>

<style scoped>
.prospect-actions { display: flex; flex-direction: column; align-items: flex-start; gap: 2px; }
.prospect-actions :deep(.base-button.p-button) { min-height: 32px; padding: 4px 8px; font-size: 13px; font-weight: 600; }
.confirmation-copy { overflow-wrap: anywhere; }
:deep(.prospects-table th:first-child) { width: 30%; }
:deep(.prospects-table th:nth-child(2)) { width: 19%; }
:deep(.prospects-table th:last-child) { width: 210px; }
</style>
