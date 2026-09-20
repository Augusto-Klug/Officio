<script setup lang="ts">
import { computed, onBeforeUnmount, ref } from 'vue'
import Column from 'primevue/column'
import DataTable from 'primevue/datatable'
import InputText from 'primevue/inputtext'
import Message from 'primevue/message'
import Select from 'primevue/select'
import BaseButton from '../../../components/base/BaseButton.vue'
import BaseCard from '../../../components/base/BaseCard.vue'
import PageHeader from '../../../components/layout/PageHeader.vue'
import StatusBadge from '../../../components/shared/StatusBadge.vue'
import { normalizeError } from '../../../api/errors'
import { formatCurrency } from '../../../utils/format'
import { cancelarFaturamento, fecharFaturamento, gerarCompetencia, listarFaturamentos } from '../services/faturamentos.service'
import type { FaturamentoMensal } from '../types/faturamento'

const hoje = new Date()
const competenciaAtual = `${hoje.getFullYear()}-${String(hoje.getMonth() + 1).padStart(2, '0')}`
const competencia = ref(competenciaAtual)
const situacao = ref('')
const faturamentos = ref<FaturamentoMensal[]>([])
const loading = ref(true)
const actionId = ref('')
const error = ref('')
const success = ref('')
let controller: AbortController | undefined

const statusOptions = [
  { label: 'Todas', value: '' }, { label: 'Aberto', value: 'ABERTO' },
  { label: 'Fechado', value: 'FECHADO' }, { label: 'Cancelado', value: 'CANCELADO' },
]
const valorTotal = computed(() => faturamentos.value.filter((item) => item.status !== 'CANCELADO').reduce((total, item) => total + item.valorTotal, 0))
const folhasTotal = computed(() => faturamentos.value.filter((item) => item.status !== 'CANCELADO').reduce((total, item) => total + item.quantidadeFolhas, 0))

function competenciaLabel(value: string): string {
  const [ano, mes] = value.slice(0, 7).split('-').map(Number)
  return new Intl.DateTimeFormat('pt-BR', { month: 'long', year: 'numeric' }).format(new Date(ano!, mes! - 1, 1))
}

function severity(status: string) {
  if (status === 'FECHADO') return 'success'
  if (status === 'ABERTO') return 'warn'
  return 'secondary'
}

function statusLabel(status: string) {
  return status === 'ABERTO' ? 'Aberto' : status === 'FECHADO' ? 'Fechado' : 'Cancelado'
}

async function carregar() {
  controller?.abort()
  controller = new AbortController()
  loading.value = true
  error.value = ''
  try { faturamentos.value = await listarFaturamentos(competencia.value, situacao.value, controller.signal) }
  catch (cause) { if (!controller.signal.aborted) error.value = normalizeError(cause).message }
  finally { if (!controller.signal.aborted) loading.value = false }
}

async function gerar() {
  if (!competencia.value || !window.confirm(`Gerar o faturamento de ${competenciaLabel(competencia.value)}?`)) return
  actionId.value = 'gerar'
  error.value = ''
  try {
    const resultado = await gerarCompetencia(competencia.value)
    success.value = resultado.gerados
      ? `${resultado.gerados} faturamento(s) gerado(s), totalizando ${formatCurrency(resultado.valorTotal)}.`
      : 'Todos os contratos aptos já estavam calculados nesta competência.'
    await carregar()
  } catch (cause) { error.value = normalizeError(cause).message }
  finally { actionId.value = '' }
}

async function alterar(item: FaturamentoMensal, acao: 'fechar' | 'cancelar') {
  const verbo = acao === 'fechar' ? 'fechar' : 'cancelar'
  if (!window.confirm(`Deseja ${verbo} o faturamento do contrato ${item.numeroContrato}?`)) return
  actionId.value = item.id
  error.value = ''
  try {
    await (acao === 'fechar' ? fecharFaturamento(item.id) : cancelarFaturamento(item.id))
    success.value = acao === 'fechar' ? 'Faturamento fechado.' : 'Faturamento cancelado.'
    await carregar()
  } catch (cause) { error.value = normalizeError(cause).message }
  finally { actionId.value = '' }
}

void carregar()
onBeforeUnmount(() => controller?.abort())
</script>

<template>
  <PageHeader title="Faturamento mensal" description="Calcule e consolide os honorários contratados por competência.">
    <BaseButton label="Gerar competência" icon="pi pi-calculator" :loading="actionId === 'gerar'" @click="gerar" />
  </PageHeader>
  <Message v-if="success" severity="success" :closable="true" class="page-message" @close="success = ''">{{ success }}</Message>
  <Message v-if="error" severity="error" :closable="false" class="page-message">{{ error }}</Message>

  <form class="filters billing-filters" @submit.prevent="carregar">
    <div class="field"><label for="billing-period">Competência</label><InputText id="billing-period" v-model="competencia" type="month" required /></div>
    <div class="field"><label for="billing-status">Situação</label><Select input-id="billing-status" v-model="situacao" :options="statusOptions" option-label="label" option-value="value" /></div>
    <BaseButton label="Aplicar" icon="pi pi-filter" variant="secondary" type="submit" :loading="loading" />
  </form>

  <div class="billing-summary" aria-label="Resumo da competência">
    <div><span>Valor da competência</span><strong class="numeric">{{ formatCurrency(valorTotal) }}</strong></div>
    <div><span>Contratos calculados</span><strong class="numeric">{{ faturamentos.length }}</strong></div>
    <div><span>Folhas consideradas</span><strong class="numeric">{{ folhasTotal }}</strong></div>
  </div>

  <BaseCard class="data-card">
    <div class="list-heading"><h2>{{ competenciaLabel(competencia) }}</h2><span>{{ loading ? 'Carregando…' : `${faturamentos.length} registro(s)` }}</span></div>
    <div v-if="loading" class="list-state" aria-busy="true"><i class="pi pi-spinner loading-spinner" /><p>Carregando faturamentos…</p></div>
    <div v-else-if="error" class="list-state"><BaseButton label="Tentar novamente" icon="pi pi-refresh" variant="secondary" @click="carregar" /></div>
    <DataTable v-else :value="faturamentos" data-key="id" class="work-table billing-table" :table-props="{ 'aria-label': 'Faturamentos da competência' }">
      <Column field="numeroContrato" header="Contrato"><template #body="{ data }"><div class="company-cell"><strong>{{ data.numeroContrato }}</strong><span>{{ data.cliente }}</span></div></template></Column>
      <Column field="quantidadeFolhas" header="Folhas"><template #body="{ data }"><span class="numeric">{{ data.quantidadeFolhas }}</span></template></Column>
      <Column field="valorHonorarioBase" header="Honorário"><template #body="{ data }"><span class="numeric">{{ formatCurrency(data.valorHonorarioBase) }}</span></template></Column>
      <Column field="valorPorFolha" header="Por folha"><template #body="{ data }"><span class="numeric">{{ formatCurrency(data.valorPorFolha) }}</span></template></Column>
      <Column field="valorTotal" header="Total"><template #body="{ data }"><strong class="numeric">{{ formatCurrency(data.valorTotal) }}</strong></template></Column>
      <Column field="status" header="Status"><template #body="{ data }"><StatusBadge :label="statusLabel(data.status)" :severity="severity(data.status)" /></template></Column>
      <Column header="Ações"><template #body="{ data }"><div v-if="data.status === 'ABERTO'" class="row-actions"><BaseButton label="Fechar" icon="pi pi-lock" variant="secondary" :loading="actionId === data.id" :disabled="!!actionId && actionId !== data.id" @click="alterar(data, 'fechar')" /><BaseButton icon="pi pi-ban" variant="text" title="Cancelar faturamento" aria-label="Cancelar faturamento" :loading="actionId === data.id" :disabled="!!actionId && actionId !== data.id" @click="alterar(data, 'cancelar')" /></div><span v-else class="muted">Sem ações</span></template></Column>
      <template #empty><div class="empty-state"><i class="pi pi-receipt" /><h3>Nenhum faturamento nesta competência</h3><p>Gere a competência para calcular os contratos ativos no período.</p></div></template>
    </DataTable>
  </BaseCard>
</template>
