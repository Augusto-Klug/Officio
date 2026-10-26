<script setup lang="ts">
import Dialog from 'primevue/dialog'
import BaseButton from '../../../components/base/BaseButton.vue'
import StatusBadge from '../../../components/shared/StatusBadge.vue'
import { formatCurrency } from '../../../utils/format'
import { naturezaCobrancaOptions, naturezaCobrancaPorNome, rotuloStatusOrcamento, secaoOptions, tipoOptions, secaoPorNome, tipoPorNome, type Orcamento } from '../types/orcamento'

const visible = defineModel<boolean>('visible', { required: true })
defineProps<{ orcamento: Orcamento | null; busy?: boolean }>()
const emit = defineEmits<{ editar: [orcamento: Orcamento]; aceitar: [orcamento: Orcamento]; converter: [orcamento: Orcamento] }>()

function severity(status: string) {
  if (status === 'Aceito') return 'success'
  if (status === 'Recusado' || status === 'Expirado') return 'danger'
  if (status === 'Enviado' || status === 'ProntoParaEnvio') return 'info'
  return 'secondary'
}

function podeEditar(status: string) {
  return status === 'Rascunho' || status === 'EmRevisao'
}

function podeAceitar(status: string) {
  return !['Aceito', 'Recusado', 'Expirado', 'Substituido'].includes(status)
}

function nomeOpcao(nome: string, mapa: Record<string, number>, opcoes: readonly { label: string; value: number }[]) {
  const valor = mapa[nome]
  return opcoes.find((opcao) => opcao.value === valor)?.label ?? nome
}
</script>

<template>
  <Dialog v-model:visible="visible" modal header="Visão geral do orçamento" class="workflow-dialog budget-detail-dialog" :style="{ width: 'min(920px, 95vw)' }">
    <div v-if="orcamento" class="budget-detail">
      <div class="detail-heading">
        <div>
          <h2>{{ orcamento.titulo || 'Sem título' }}</h2>
          <p>Proposta comercial com {{ orcamento.itens.length }} {{ orcamento.itens.length === 1 ? 'item' : 'itens' }}.</p>
        </div>
        <StatusBadge :label="rotuloStatusOrcamento(orcamento.status)" :severity="severity(orcamento.status)" />
      </div>

      <dl class="detail-grid budget-summary">
        <div><dt>Valor total</dt><dd class="numeric budget-total">{{ formatCurrency(orcamento.valorTotal) }}</dd></div>
        <div><dt>Vencimento</dt><dd>Dia {{ orcamento.diaVencimento }} de cada mês</dd></div>
        <div><dt>Situação</dt><dd>{{ rotuloStatusOrcamento(orcamento.status) }}</dd></div>
      </dl>

      <section class="detail-section">
        <h3>Itens da proposta <span>{{ orcamento.itens.length }}</span></h3>
        <div class="budget-items-table-wrap">
          <table class="budget-items-table">
            <thead><tr><th>Descrição</th><th>Seção</th><th>Tipo</th><th>Cobrança</th><th class="numeric">Qtd.</th><th class="numeric">Unitário</th><th class="numeric">Total</th></tr></thead>
            <tbody>
              <tr v-for="item in [...orcamento.itens].sort((a, b) => a.ordem - b.ordem)" :key="item.id">
                <td><strong>{{ item.descricao }}</strong></td>
                <td>{{ nomeOpcao(item.secao, secaoPorNome, secaoOptions) }}</td>
                <td>{{ nomeOpcao(item.tipo, tipoPorNome, tipoOptions) }}</td>
                <td>{{ nomeOpcao(item.naturezaCobranca, naturezaCobrancaPorNome, naturezaCobrancaOptions) }}</td>
                <td class="numeric">{{ item.quantidade }}</td>
                <td class="numeric">{{ formatCurrency(item.valorUnitario) }}</td>
                <td class="numeric"><strong>{{ formatCurrency(item.valorTotal) }}</strong></td>
              </tr>
            </tbody>
            <tfoot><tr><td colspan="6">Total da proposta</td><td class="numeric">{{ formatCurrency(orcamento.valorTotal) }}</td></tr></tfoot>
          </table>
        </div>
      </section>

      <section v-if="orcamento.observacoes" class="detail-section"><h3>Observações</h3><p>{{ orcamento.observacoes }}</p></section>
      <section v-if="orcamento.motivoRecusa" class="detail-section"><h3>Motivo da recusa</h3><p>{{ orcamento.motivoRecusa }}</p></section>
    </div>

    <template #footer>
      <BaseButton label="Fechar" variant="text" @click="visible = false" />
      <BaseButton v-if="orcamento && podeEditar(orcamento.status)" label="Editar" icon="pi pi-pencil" variant="secondary" :disabled="busy" @click="emit('editar', orcamento)" />
      <BaseButton v-if="orcamento && podeAceitar(orcamento.status)" label="Aceitar" icon="pi pi-check" :loading="busy" @click="emit('aceitar', orcamento)" />
      <BaseButton v-if="orcamento?.status === 'Aceito'" label="Gerar contrato" icon="pi pi-file" :disabled="busy" @click="emit('converter', orcamento)" />
    </template>
  </Dialog>
</template>
