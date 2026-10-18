<script setup lang="ts">
import { computed } from 'vue'
import Dialog from 'primevue/dialog'
import BaseButton from '../../../components/base/BaseButton.vue'
import StatusBadge from '../../../components/shared/StatusBadge.vue'
import { formatCurrency, formatDate } from '../../../utils/format'
import { formatarCpf, formatarPis, formatarTelefone } from '../../../utils/inputMasks'
import type { AdmissaoDetalhe } from '../types/admissao'

const visible = defineModel<boolean>('visible', { required: true })
const props = defineProps<{ admissao: AdmissaoDetalhe | null; loading?: boolean }>()
defineEmits<{ editar: []; concluir: []; cancelar: [] }>()

const terminal = computed(() => props.admissao?.status === 'CONCLUIDA' || props.admissao?.status === 'CANCELADA')
const severity = computed(() => {
  if (props.admissao?.status === 'CONCLUIDA') return 'success'
  if (props.admissao?.status === 'CANCELADA') return 'danger'
  return 'warn'
})

function informar(value: string | number | null | undefined): string {
  return value == null || value === '' ? 'Não informado' : String(value)
}
</script>

<template>
  <Dialog v-model:visible="visible" modal header="Detalhes da admissão" class="workflow-dialog admission-detail-dialog" :style="{ width: 'min(900px, 96vw)' }">
    <div v-if="loading" class="list-state compact-state" aria-busy="true"><i class="pi pi-spinner loading-spinner" /><p>Carregando detalhes...</p></div>
    <div v-else-if="admissao" class="admission-detail">
      <div class="detail-heading">
        <div><h2>{{ admissao.nomeFuncionario }}</h2><p>{{ admissao.empresaNome }}</p></div>
        <StatusBadge :label="admissao.statusDescricao" :severity="severity" />
      </div>

      <dl class="detail-grid admission-summary">
        <div><dt>CPF</dt><dd>{{ informar(formatarCpf(admissao.cpf)) }}</dd></div>
        <div><dt>Função</dt><dd>{{ informar(admissao.funcao) }}</dd></div>
        <div><dt>Admissão prevista</dt><dd>{{ formatDate(admissao.dataAdmissaoPrevista) }}</dd></div>
        <div><dt>Salário</dt><dd>{{ admissao.salario == null ? 'Não informado' : formatCurrency(admissao.salario) }}</dd></div>
        <div><dt>Telefone</dt><dd>{{ informar(formatarTelefone(admissao.telefone)) }}</dd></div>
        <div><dt>E-mail</dt><dd>{{ informar(admissao.email) }}</dd></div>
      </dl>

      <section class="detail-section"><h3>Dados pessoais</h3><dl class="detail-grid detail-grid-compact">
        <div><dt>RG</dt><dd>{{ informar(admissao.rg) }}</dd></div>
        <div><dt>Nascimento</dt><dd>{{ formatDate(admissao.dataNascimento) }}</dd></div>
        <div><dt>Sexo</dt><dd>{{ informar(admissao.sexo) }}</dd></div>
        <div><dt>Estado civil</dt><dd>{{ informar(admissao.estadoCivil) }}</dd></div>
        <div><dt>Escolaridade</dt><dd>{{ informar(admissao.escolaridade) }}</dd></div>
        <div><dt>PIS/PASEP</dt><dd>{{ informar(formatarPis(admissao.pis)) }}</dd></div>
      </dl></section>

      <section class="detail-section"><h3>Vínculo e pagamento</h3><dl class="detail-grid detail-grid-compact">
        <div><dt>Experiência</dt><dd>{{ admissao.contratoExperienciaDias == null ? 'Não informado' : `${admissao.contratoExperienciaDias} dias` }}</dd></div>
        <div><dt>Comissão</dt><dd>{{ informar(admissao.comissao) }}</dd></div>
        <div><dt>Vale-transporte</dt><dd>{{ admissao.utilizaValeTransporte == null ? 'Não informado' : admissao.utilizaValeTransporte ? 'Sim' : 'Não' }}</dd></div>
        <div><dt>Horário</dt><dd>{{ informar(admissao.horarioTrabalho) }}</dd></div>
        <div><dt>Banco</dt><dd>{{ informar(admissao.banco) }}</dd></div>
        <div><dt>Conta</dt><dd>{{ [admissao.agencia, admissao.conta, admissao.tipoConta].filter(Boolean).join(' / ') || 'Não informada' }}</dd></div>
      </dl></section>

      <section class="detail-section"><h3>Dependentes <span>{{ admissao.dependentes.length }}</span></h3>
        <div v-if="admissao.dependentes.length" class="detail-list">
          <div v-for="dependente in admissao.dependentes" :key="dependente.id"><strong>{{ dependente.nome }}</strong><span>{{ dependente.parentesco || 'Parentesco não informado' }} · {{ formatarCpf(dependente.cpf) || 'CPF não informado' }}</span></div>
        </div>
        <p v-else class="muted">Nenhum dependente informado.</p>
      </section>

      <section v-if="admissao.observacoes" class="detail-section"><h3>Observações</h3><p>{{ admissao.observacoes }}</p></section>
    </div>
    <template #footer>
      <BaseButton label="Fechar" variant="text" @click="visible = false" />
      <BaseButton v-if="admissao && !terminal" label="Cancelar admissão" icon="pi pi-ban" variant="secondary" @click="$emit('cancelar')" />
      <BaseButton v-if="admissao && !terminal" label="Editar" icon="pi pi-pencil" variant="secondary" @click="$emit('editar')" />
      <BaseButton v-if="admissao && !terminal && admissao.status === 'LIBERADO_PELO_EMPREGADOR'" label="Concluir admissão" icon="pi pi-check" @click="$emit('concluir')" />
    </template>
  </Dialog>
</template>
