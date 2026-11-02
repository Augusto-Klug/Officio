<script setup lang="ts">
import { onBeforeUnmount, ref, watch } from 'vue'
import { useRouter } from 'vue-router'
import axios from 'axios'
import Column from 'primevue/column'
import DataTable from 'primevue/datatable'
import Skeleton from 'primevue/skeleton'
import BaseButton from '../../../components/base/BaseButton.vue'
import BaseCard from '../../../components/base/BaseCard.vue'
import BaseInput from '../../../components/base/BaseInput.vue'
import PageHeader from '../../../components/layout/PageHeader.vue'
import { normalizeError } from '../../../api/errors'
import { formatCnpj } from '../../../utils/format'
import { listarEmpresasComAdmissoes } from '../services/admissoes.service'
import type { EmpresaAdmissoesResumo } from '../types/admissao'
import { useRealtimePageRefresh } from '../../../composables/useRealtimePageRefresh'

const router = useRouter()
const empresas = ref<EmpresaAdmissoesResumo[]>([])
const busca = ref('')
const loading = ref(true)
const error = ref('')
let controller: AbortController | undefined
let generation = 0
let searchTimer: ReturnType<typeof setTimeout> | undefined

async function load() {
  if (searchTimer) clearTimeout(searchTimer)
  const request = ++generation
  controller?.abort()
  const currentController = new AbortController()
  controller = currentController
  loading.value = true
  error.value = ''
  try {
    const resultado = await listarEmpresasComAdmissoes(busca.value.trim() || undefined, currentController.signal)
    if (request === generation) empresas.value = resultado
  } catch (cause) {
    if (request === generation && !axios.isCancel(cause)) error.value = normalizeError(cause).message
  } finally {
    if (request === generation) loading.value = false
  }
}

useRealtimePageRefresh('admissao', load, loading)

function agendarBusca() {
  if (searchTimer) clearTimeout(searchTimer)
  searchTimer = setTimeout(() => void load(), 350)
}

function abrirAdmissoes(empresa: EmpresaAdmissoesResumo) {
  void router.push({
    name: 'admissoes-empresa',
    params: { empresaId: empresa.empresaId },
    query: { empresaNome: empresa.empresaNome },
  })
}

watch(busca, agendarBusca)
void load()
onBeforeUnmount(() => {
  if (searchTimer) clearTimeout(searchTimer)
  ++generation
  controller?.abort()
})
</script>

<template>
  <PageHeader title="Admissões por cliente" description="Selecione uma empresa para consultar e gerenciar as admissões." />
  <Message v-if="error" severity="error" :closable="true" class="page-message" @close="error = ''">{{ error }}</Message>

  <BaseCard class="data-card">
    <form class="filters" role="search" aria-label="Filtrar clientes com admissões" @submit.prevent="load">
      <BaseInput id="admission-company-search" v-model="busca" label="Buscar cliente" type="search" placeholder="Razão social, nome fantasia ou CNPJ" autocomplete="off" />
    </form>
    <div class="list-heading">
      <h2>Clientes com admissões</h2>
      <span>{{ loading ? 'Carregando...' : `${empresas.length} ${empresas.length === 1 ? 'cliente' : 'clientes'}` }}</span>
    </div>
    <div v-if="loading" class="table-skeleton" aria-busy="true" aria-label="Carregando clientes">
      <div v-for="linha in 5" :key="linha" class="skeleton-row">
        <Skeleton width="65%" height="1rem" />
        <Skeleton width="50%" height="1rem" />
        <Skeleton width="40%" height="1rem" />
        <Skeleton width="70%" height="2rem" />
      </div>
    </div>
    <DataTable v-else :value="empresas" data-key="empresaId" class="work-table admissions-company-table" :table-props="{ 'aria-label': 'Clientes com admissões' }">
      <Column field="empresaNome" header="Cliente" />
      <Column field="cnpj" header="CNPJ"><template #body="{ data }">{{ formatCnpj(data.cnpj) || 'Não informado' }}</template></Column>
      <Column field="quantidadeAdmissoes" header="Admissões" />
      <Column header="Ações"><template #body="{ data }"><BaseButton label="Ver admissões" icon="pi pi-arrow-right" variant="text" @click="abrirAdmissoes(data)" /></template></Column>
      <template #empty>
        <div class="empty-state"><i class="pi pi-id-card" /><h3>Nenhum cliente encontrado</h3><p>Nenhum cliente possui admissões cadastradas.</p></div>
      </template>
    </DataTable>
  </BaseCard>
</template>

<style scoped>
.table-skeleton { display: grid; }
.skeleton-row { display: grid; grid-template-columns: repeat(4, minmax(0, 1fr)); align-items: center; gap: 1rem; min-height: 58px; border-bottom: 1px solid var(--surface-border); }
.admissions-company-table :deep(.p-datatable-table) { width: 100%; table-layout: fixed; }
.admissions-company-table :deep(.p-datatable-thead > tr > th),
.admissions-company-table :deep(.p-datatable-tbody > tr > td) { width: 25%; }
@media (max-width: 700px) {
  .skeleton-row { gap: 0.5rem; }
}
</style>
