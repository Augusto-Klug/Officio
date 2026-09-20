<script setup lang="ts">
import Message from 'primevue/message'
import { computed, onBeforeUnmount, onMounted, ref } from 'vue'
import BaseButton from '../../../components/base/BaseButton.vue'
import BaseCard from '../../../components/base/BaseCard.vue'
import PageContainer from '../../../components/layout/PageContainer.vue'
import PageHeader from '../../../components/layout/PageHeader.vue'
import { formatCurrency, formatDate } from '../../../utils/format'
import { carregarDashboard } from '../services/dashboard.service'
import type { DashboardAlerta, DashboardProjecaoReceita, DashboardVisaoGeral } from '../types/dashboard'

const visaoGeral = ref<DashboardVisaoGeral | null>(null)
const alertas = ref<DashboardAlerta[]>([])
const projecao = ref<DashboardProjecaoReceita[]>([])
const loading = ref(true)
const error = ref('')
let controller: AbortController | null = null

const maiorReceita = computed(() => Math.max(...projecao.value.map((item) => item.receitaContratada), 1))
const indicadores = computed(() => visaoGeral.value ? [
  { label: 'Prospectos em aberto', value: String(visaoGeral.value.prospectosEmAberto), route: '/prospectos' },
  { label: 'Clientes ativos', value: String(visaoGeral.value.clientesAtivos), route: '/clientes' },
  { label: 'Contratos ativos', value: String(visaoGeral.value.contratosAtivos), route: '/contratos' },
  { label: 'Admissoes pendentes', value: String(visaoGeral.value.admissoesPendentes), route: '/admissoes' },
  { label: 'Orcamentos enviados', value: String(visaoGeral.value.orcamentosAguardandoRetorno), route: '/prospectos' },
] : [])

function mesLabel(value: string): string {
  const [ano, mes] = value.slice(0, 10).split('-').map(Number)
  return new Intl.DateTimeFormat('pt-BR', { month: 'short', year: '2-digit' })
    .format(new Date(ano!, mes! - 1, 1)).replace('.', '')
}

function larguraBarra(valor: number): string {
  return `${Math.max(valor > 0 ? 4 : 0, Math.round((valor / maiorReceita.value) * 100))}%`
}

async function carregar() {
  controller?.abort()
  controller = new AbortController()
  loading.value = true
  error.value = ''
  try {
    const resultado = await carregarDashboard(controller.signal)
    visaoGeral.value = resultado.visaoGeral
    alertas.value = resultado.alertas
    projecao.value = resultado.projecao
  } catch (cause) {
    if (controller.signal.aborted) return
    error.value = cause instanceof Error ? cause.message : 'Nao foi possivel carregar o dashboard.'
  } finally {
    if (!controller.signal.aborted) loading.value = false
  }
}

onMounted(carregar)
onBeforeUnmount(() => controller?.abort())
</script>

<template>
  <PageContainer>
    <PageHeader title="Visao geral" description="Indicadores comerciais e operacionais do escritorio.">
      <BaseButton label="Atualizar" icon="pi pi-refresh" variant="secondary" :loading="loading" @click="carregar" />
    </PageHeader>

    <Message v-if="error" severity="error" :closable="false" class="page-message">{{ error }}</Message>

    <div v-if="loading && !visaoGeral" class="dashboard-loading" aria-live="polite">
      <i class="pi pi-spinner loading-spinner" aria-hidden="true" />
      <span>Carregando indicadores...</span>
    </div>

    <template v-else-if="visaoGeral">
      <BaseCard class="dashboard-metrics">
        <div class="dashboard-revenue">
          <span>Receita mensal contratada</span>
          <strong class="numeric">{{ formatCurrency(visaoGeral.receitaMensalContratada) }}</strong>
        </div>
        <RouterLink v-for="indicador in indicadores" :key="indicador.label" :to="indicador.route" class="dashboard-metric">
          <strong class="numeric">{{ indicador.value }}</strong>
          <span>{{ indicador.label }}</span>
        </RouterLink>
      </BaseCard>

      <div class="dashboard-grid">
        <BaseCard class="dashboard-panel">
          <div class="dashboard-panel-heading">
            <div><h2>Atencao necessaria</h2><p>Itens que pedem acompanhamento da equipe.</p></div>
            <span>{{ alertas.length }}</span>
          </div>
          <div v-if="alertas.length" class="dashboard-alert-list">
            <RouterLink v-for="alerta in alertas" :key="`${alerta.codigo}-${alerta.referenciaId}`" :to="alerta.rota" class="dashboard-alert">
              <span class="dashboard-alert-marker" :class="`is-${alerta.severidade}`" aria-hidden="true" />
              <span class="dashboard-alert-copy"><strong>{{ alerta.titulo }}</strong><span>{{ alerta.descricao }}</span></span>
              <time :datetime="alerta.dataReferencia">{{ formatDate(alerta.dataReferencia) }}</time>
              <i class="pi pi-chevron-right" aria-hidden="true" />
            </RouterLink>
          </div>
          <div v-else class="dashboard-empty"><i class="pi pi-check-circle" aria-hidden="true" /><span>Nenhuma pendencia prioritaria.</span></div>
        </BaseCard>

        <BaseCard class="dashboard-panel">
          <div class="dashboard-panel-heading">
            <div><h2>Receita contratada</h2><p>Projecao dos proximos seis meses.</p></div>
          </div>
          <div class="dashboard-chart" role="img" aria-label="Projecao mensal de receita contratada">
            <div v-for="item in projecao" :key="item.mes" class="dashboard-chart-row">
              <span>{{ mesLabel(item.mes) }}</span>
              <div class="dashboard-chart-track"><span :style="{ width: larguraBarra(item.receitaContratada) }" /></div>
              <strong class="numeric">{{ formatCurrency(item.receitaContratada) }}</strong>
            </div>
          </div>
        </BaseCard>
      </div>
    </template>
  </PageContainer>
</template>
