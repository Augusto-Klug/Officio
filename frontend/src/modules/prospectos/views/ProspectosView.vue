<script setup lang="ts">
import { computed, onBeforeUnmount, ref, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import axios from 'axios'
import DataTable from 'primevue/datatable'
import Column from 'primevue/column'
import Paginator, { type PageState } from 'primevue/paginator'
import Message from 'primevue/message'
import BaseButton from '../../../components/base/BaseButton.vue'
import BaseInput from '../../../components/base/BaseInput.vue'
import BaseSelect from '../../../components/base/BaseSelect.vue'
import BaseCard from '../../../components/base/BaseCard.vue'
import PageHeader from '../../../components/layout/PageHeader.vue'
import ProspectoStatus from '../components/ProspectoStatus.vue'
import { listarProspectos } from '../services/prospectos.service'
import { statusOptions, type ProspectosPagina } from '../types/prospecto'
import { normalizeError } from '../../../api/errors'
import { formatCnpj, formatDate } from '../../../utils/format'

const route = useRoute()
const router = useRouter()
const busca = ref(typeof route.query.busca === 'string' ? route.query.busca : '')
const status = ref<string | null>(typeof route.query.status === 'string' ? route.query.status : null)
const loading = ref(true)
const error = ref('')
const result = ref<ProspectosPagina | null>(null)
const pagina = computed(() => Math.max(1, Number.parseInt(String(route.query.pagina || '1'), 10) || 1))
const filtered = computed(() => !!(route.query.busca || route.query.status || route.query.responsavelId))
let controller: AbortController | undefined
let timer: ReturnType<typeof setTimeout> | undefined
let generation = 0

async function load() {
  const request = ++generation
  controller?.abort()
  controller = new AbortController()
  loading.value = true
  error.value = ''
  result.value = null
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
  } catch (cause) {
    if (request === generation && !axios.isCancel(cause)) error.value = normalizeError(cause).message
  } finally { if (request === generation) loading.value = false }
}
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
    <div class="new-prospect"><BaseButton label="Novo prospecto" icon="pi pi-plus" disabled aria-describedby="new-prospect-hint" /><small id="new-prospect-hint">Cadastro disponível em uma próxima etapa.</small></div>
  </PageHeader>

  <BaseCard class="prospects-card">
    <form class="filters" role="search" aria-label="Filtrar prospectos" @submit.prevent="applyFilters">
      <BaseInput id="busca" v-model="busca" label="Buscar prospecto" name="busca" type="search" autocomplete="off" placeholder="Razão social ou CNPJ…" @update:model-value="scheduleSearch" />
      <BaseSelect id="status" v-model="status" label="Status" :options="statusOptions" placeholder="Todos os status" @update:model-value="applyFilters" />
      <BaseButton v-if="filtered || busca || status" label="Limpar filtros" variant="text" @click="clearFilters" />
    </form>

    <div class="list-heading"><h2>Lista de prospectos</h2><span role="status" aria-live="polite">{{ loading ? 'Carregando…' : result ? `${result.totalRegistros.toLocaleString('pt-BR')} ${result.totalRegistros === 1 ? 'prospecto' : 'prospectos'}` : 'Consulta indisponível' }}</span></div>

    <div v-if="error" class="list-state"><Message severity="error" :closable="false" role="alert">{{ error }}</Message><BaseButton label="Tentar novamente" icon="pi pi-refresh" variant="secondary" @click="load" /></div>
    <div v-else-if="loading" class="list-state" aria-busy="true"><i class="pi pi-spinner loading-spinner" aria-hidden="true" /><p role="status">Carregando prospectos…</p></div>
    <template v-else-if="result">
      <DataTable :value="result.dados" data-key="id" class="prospects-table" :table-props="{ 'aria-label': 'Lista de prospectos' }">
        <Column field="razaoSocial" header="Razão social / CNPJ"><template #body="{ data }"><div class="company-cell"><strong>{{ data.razaoSocial }}</strong><span>{{ formatCnpj(data.cnpj) }}</span></div></template></Column>
        <Column field="status" header="Status"><template #body="{ data }"><ProspectoStatus :status="data.status" /></template></Column>
        <Column field="responsavel" header="Responsável"><template #body="{ data }"><span :class="{ muted: !data.responsavel }">{{ data.responsavel || 'Não atribuído' }}</span></template></Column>
        <Column field="criadoEm" header="Criado em"><template #body="{ data }"><span class="numeric">{{ formatDate(data.criadoEm) }}</span></template></Column>
        <template #empty><div class="empty-state"><i class="pi pi-users" aria-hidden="true" /><h3>{{ filtered ? 'Nenhum resultado encontrado' : 'Nenhum prospecto por aqui' }}</h3><p>{{ filtered ? 'Tente outra busca ou remova os filtros aplicados.' : 'Os prospectos cadastrados no escritório aparecerão nesta lista.' }}</p><BaseButton v-if="filtered" label="Limpar filtros" variant="secondary" @click="clearFilters" /></div></template>
      </DataTable>
      <div v-if="result.totalRegistros > 0" class="pagination-footer">
        <span>{{ ((result.pagina - 1) * result.tamanhoPagina + 1).toLocaleString('pt-BR') }}–{{ Math.min(result.pagina * result.tamanhoPagina, result.totalRegistros).toLocaleString('pt-BR') }} de {{ result.totalRegistros.toLocaleString('pt-BR') }}</span>
        <Paginator :first="(pagina - 1) * 20" :rows="20" :total-records="result.totalRegistros" :page-link-size="3" template="PrevPageLink PageLinks NextPageLink" @page="changePage" />
      </div>
    </template>
  </BaseCard>
  <p class="list-footnote">20 prospectos por página</p>
</template>
