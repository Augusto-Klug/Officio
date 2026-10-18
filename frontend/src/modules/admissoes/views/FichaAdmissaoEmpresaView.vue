<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { useRoute } from 'vue-router'
import InputNumber from 'primevue/inputnumber'
import InputText from 'primevue/inputtext'
import Select from 'primevue/select'
import Message from 'primevue/message'
import BaseButton from '../../../components/base/BaseButton.vue'
import StatusBadge from '../../../components/shared/StatusBadge.vue'
import { normalizeError } from '../../../api/errors'
import { formatDate } from '../../../utils/format'
import { formatarCpf, formatarPis, formatarTelefone, somenteDigitos } from '../../../utils/inputMasks'
import { atualizarFichaEmpresaAdmissao, concluirFichaEmpresaAdmissao, obterFichaEmpresaAdmissao } from '../services/admissoes.service'
import type { AtualizarFichaEmpresaPayload, FichaEmpresaPublica } from '../types/admissao'
import { tiposContaAdmissao } from '../types/opcoesAdmissao'

const route = useRoute()
const token = String(route.params.token || '')
const ficha = ref<FichaEmpresaPublica | null>(null)
const form = ref<AtualizarFichaEmpresaPayload>({})
const loading = ref(true)
const saving = ref(false)
const error = ref('')
const success = ref('')
const finalizado = ref(false)

function preencher(dados: FichaEmpresaPublica) {
  form.value = {
    pis: formatarPis(dados.pis),
    dataAdmissaoPrevista: dados.dataAdmissaoPrevista ?? '',
    salario: dados.salario ?? undefined,
    comissao: dados.comissao ?? '',
    funcao: dados.funcao ?? '',
    contratoExperienciaDias: dados.contratoExperienciaDias ?? undefined,
    utilizaValeTransporte: dados.utilizaValeTransporte ?? undefined,
    horarioTrabalho: dados.horarioTrabalho ?? '',
    banco: dados.banco ?? '',
    agencia: dados.agencia ?? '',
    conta: dados.conta ?? '',
    tipoConta: dados.tipoConta ?? '',
  }
  finalizado.value = dados.preenchimentoConcluido
}

async function carregar() {
  loading.value = true
  error.value = ''
  try {
    ficha.value = await obterFichaEmpresaAdmissao(token)
    preencher(ficha.value)
  } catch (cause) { error.value = normalizeError(cause).message }
  finally { loading.value = false }
}

async function salvar() {
  saving.value = true
  error.value = ''
  try { await atualizarFichaEmpresaAdmissao(token, prepararPayload()); success.value = 'Dados salvos.'; await carregar() }
  catch (cause) { error.value = normalizeError(cause).message }
  finally { saving.value = false }
}

async function concluir() {
  saving.value = true
  error.value = ''
  try {
    await atualizarFichaEmpresaAdmissao(token, prepararPayload())
    await concluirFichaEmpresaAdmissao(token)
    success.value = 'Dados de contratação enviados.'
    await carregar()
  } catch (cause) { error.value = normalizeError(cause).message }
  finally { saving.value = false }
}

function prepararPayload(): AtualizarFichaEmpresaPayload {
  return { ...form.value, pis: somenteDigitos(form.value.pis) || undefined }
}

function exibir(valor: string | null | undefined): string { return valor?.trim() || 'Não informado' }
function exibirMoeda(valor: number | null): string {
  return valor == null ? 'Não informado' : valor.toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' })
}

const opcoesValeTransporte = [
  { label: 'Sim', value: true },
  { label: 'Não', value: false },
]

onMounted(() => void carregar())
</script>

<template>
  <main class="public-admission-page">
    <header class="public-admission-header"><a href="/" class="brand">Officio<span class="brand-dot">.</span></a><span>Dados do empregador</span></header>
    <div v-if="loading" class="list-state" aria-busy="true"><i class="pi pi-spinner loading-spinner" /><p>Carregando ficha...</p></div>
    <div v-else-if="!ficha" class="public-admission-state"><i class="pi pi-link" /><h1>Link indisponível</h1><p>{{ error || 'Este link não está mais disponível.' }}</p></div>
    <form v-else class="public-admission-form" @submit.prevent="salvar">
      <div class="public-admission-title"><div><h1>{{ ficha.empresaNome }}</h1><p>Dados de contratação de {{ ficha.nomeFuncionario }}</p></div><StatusBadge :label="finalizado ? 'Enviada' : 'Em preenchimento'" :severity="finalizado ? 'success' : 'warn'" /></div>
      <Message v-if="success" severity="success" :closable="true" @close="success = ''">{{ success }}</Message>
      <Message v-if="error" severity="error" :closable="true" @close="error = ''">{{ error }}</Message>
      <div v-if="finalizado" class="public-admission-complete"><i class="pi pi-check-circle" /><div><h2>Dados enviados</h2><p>O preenchimento do empregador foi concluído. A contabilidade pode revisar a admissão.</p></div></div>

      <fieldset :disabled="finalizado || saving">
        <section class="public-form-section"><h2>Dados do admitido</h2><div class="form-grid three-columns">
          <div class="field field-span-2"><label>Nome completo</label><InputText :model-value="ficha.nomeFuncionario" disabled /></div>
          <div class="field"><label>CPF</label><InputText :model-value="exibir(formatarCpf(ficha.cpf))" disabled /></div>
          <div class="field"><label>RG</label><InputText :model-value="exibir(ficha.rg)" disabled /></div>
          <div class="field"><label>Data de nascimento</label><InputText :model-value="formatDate(ficha.dataNascimento)" disabled /></div>
          <div class="field"><label>Sexo</label><InputText :model-value="exibir(ficha.sexo)" disabled /></div>
          <div class="field"><label>Estado civil</label><InputText :model-value="exibir(ficha.estadoCivil)" disabled /></div>
          <div class="field"><label>Escolaridade</label><InputText :model-value="exibir(ficha.escolaridade)" disabled /></div>
          <div class="field"><label>Telefone</label><InputText :model-value="exibir(formatarTelefone(ficha.telefone))" disabled /></div>
          <div class="field field-span-2"><label>E-mail</label><InputText :model-value="exibir(ficha.email)" disabled /></div>
        </div></section>

        <section class="public-form-section"><h2>Dados de contratação</h2><div class="form-grid three-columns">
          <div class="field"><label for="employer-pis">PIS/PASEP</label><InputText id="employer-pis" :model-value="form.pis" inputmode="numeric" maxlength="14" placeholder="000.00000.00-0" @update:model-value="form.pis = formatarPis($event)" /></div>
          <div class="field"><label for="employer-date">Data de admissão *</label><InputText id="employer-date" v-model="form.dataAdmissaoPrevista" type="date" required /></div>
          <div class="field"><label for="employer-salary">Salário</label><InputNumber input-id="employer-salary" v-model="form.salario" mode="currency" currency="BRL" locale="pt-BR" :min="0" /></div>
          <div class="field"><label for="employer-role">Função *</label><InputText id="employer-role" v-model="form.funcao" required /></div>
          <div class="field"><label for="employer-commission">Comissão</label><InputText id="employer-commission" v-model="form.comissao" /></div>
          <div class="field"><label for="employer-trial">Experiência (dias)</label><InputNumber input-id="employer-trial" v-model="form.contratoExperienciaDias" :min="0" :max="90" /></div>
          <div class="field"><label>Vale-transporte</label><div class="public-radio-group"><label v-for="option in opcoesValeTransporte" :key="option.label"><input v-model="form.utilizaValeTransporte" type="radio" name="vale-transporte" :value="option.value">{{ option.label }}</label></div></div>
          <div class="field field-span-2"><label for="employer-hours">Horário de trabalho</label><InputText id="employer-hours" v-model="form.horarioTrabalho" /></div>
        </div></section>

        <section class="public-form-section"><h2>Dados bancários</h2><div class="form-grid four-columns">
          <div class="field"><label for="employer-bank">Banco</label><InputText id="employer-bank" v-model="form.banco" /></div>
          <div class="field"><label for="employer-branch">Agência</label><InputText id="employer-branch" v-model="form.agencia" /></div>
          <div class="field"><label for="employer-account">Conta</label><InputText id="employer-account" v-model="form.conta" /></div>
          <div class="field"><label for="employer-account-type">Tipo de conta</label><Select input-id="employer-account-type" v-model="form.tipoConta" :options="tiposContaAdmissao" placeholder="Selecione" :disabled="finalizado || saving" /></div>
        </div></section>
      </fieldset>

      <footer v-if="!finalizado" class="public-form-actions"><span>Link válido até {{ formatDate(ficha.expiraEm) }}</span><div><BaseButton label="Salvar" icon="pi pi-save" variant="secondary" :loading="saving" type="submit" /><BaseButton label="Concluir preenchimento" icon="pi pi-check" :loading="saving" type="button" @click="concluir" /></div></footer>
    </form>
  </main>
</template>
