<script setup lang="ts">
import { ref, watch } from 'vue'
import Button from 'primevue/button'
import Checkbox from 'primevue/checkbox'
import Dialog from 'primevue/dialog'
import InputNumber from 'primevue/inputnumber'
import InputText from 'primevue/inputtext'
import Message from 'primevue/message'
import Select from 'primevue/select'
import Textarea from 'primevue/textarea'
import BaseButton from '../../../components/base/BaseButton.vue'
import { formatarCpf, formatarPis, formatarTelefone, somenteDigitos } from '../../../utils/inputMasks'
import type { Cliente } from '../../clientes/types/cliente'
import type { AdmissaoDetalhe, DadosAdmissaoPayload, DependenteAdmissaoPayload } from '../types/admissao'
import { escolaridadesAdmissao, estadosCivisAdmissao, opcoesComValorAtual, sexosAdmissao, tiposContaAdmissao } from '../types/opcoesAdmissao'

const visible = defineModel<boolean>('visible', { required: true })
const props = defineProps<{
  clientes: Cliente[]
  admissao: AdmissaoDetalhe | null
  clienteInicialId?: string
  saving?: boolean
  error?: string
}>()
const emit = defineEmits<{ salvar: [clienteId: string, dados: DadosAdmissaoPayload] }>()

interface FormularioAdmissao extends Omit<DadosAdmissaoPayload, 'dependentes'> {
  dependentes: DependenteAdmissaoPayload[]
}

const clienteId = ref('')
const localError = ref('')
const form = ref<FormularioAdmissao>(novoFormulario())

function novoFormulario(): FormularioAdmissao {
  return { nomeFuncionario: '', dependentes: [] }
}

function preencher(admissao: AdmissaoDetalhe | null) {
  localError.value = ''
  clienteId.value = props.clienteInicialId || ''
  if (!admissao) {
    form.value = novoFormulario()
    return
  }
  form.value = {
    nomeFuncionario: admissao.nomeFuncionario,
    cpf: formatarCpf(admissao.cpf) || undefined,
    rg: admissao.rg || undefined,
    dataNascimento: admissao.dataNascimento || undefined,
    sexo: admissao.sexo || undefined,
    estadoCivil: admissao.estadoCivil || undefined,
    escolaridade: admissao.escolaridade || undefined,
    telefone: formatarTelefone(admissao.telefone) || undefined,
    email: admissao.email || undefined,
    pis: formatarPis(admissao.pis) || undefined,
    banco: admissao.banco || undefined,
    agencia: admissao.agencia || undefined,
    conta: admissao.conta || undefined,
    tipoConta: admissao.tipoConta || undefined,
    dataAdmissaoPrevista: admissao.dataAdmissaoPrevista || undefined,
    salario: admissao.salario ?? undefined,
    comissao: admissao.comissao || undefined,
    funcao: admissao.funcao || undefined,
    contratoExperienciaDias: admissao.contratoExperienciaDias ?? undefined,
    utilizaValeTransporte: admissao.utilizaValeTransporte ?? undefined,
    horarioTrabalho: admissao.horarioTrabalho || undefined,
    observacoes: admissao.observacoes || undefined,
    dependentes: admissao.dependentes.map((item) => ({
      nome: item.nome,
      cpf: formatarCpf(item.cpf) || undefined,
      dataNascimento: item.dataNascimento || undefined,
      parentesco: item.parentesco || undefined,
      dependenteIr: item.dependenteIr ?? undefined,
      salarioFamilia: item.salarioFamilia ?? undefined,
    })),
  }
}

function adicionarDependente() {
  form.value.dependentes.push({ nome: '' })
}

function removerDependente(index: number) {
  form.value.dependentes.splice(index, 1)
}

function salvar() {
  localError.value = ''
  if (!props.admissao && !clienteId.value) { localError.value = 'Selecione o cliente.'; return }
  if (!form.value.nomeFuncionario.trim()) { localError.value = 'Informe o nome do funcionário.'; return }
  if (!form.value.cpf?.trim()) { localError.value = 'Informe o CPF do funcionário.'; return }
  if (!form.value.rg?.trim()) { localError.value = 'Informe o RG do funcionário.'; return }
  if (!form.value.dataNascimento) { localError.value = 'Informe a data de nascimento.'; return }
  if (!form.value.sexo) { localError.value = 'Selecione o sexo do funcionário.'; return }
  if (form.value.dependentes.some((item) => !item.nome.trim())) { localError.value = 'Informe o nome de todos os dependentes.'; return }
  emit('salvar', clienteId.value, {
    ...form.value,
    nomeFuncionario: form.value.nomeFuncionario.trim(),
    cpf: somenteDigitos(form.value.cpf) || undefined,
    telefone: somenteDigitos(form.value.telefone) || undefined,
    pis: somenteDigitos(form.value.pis) || undefined,
    dataNascimento: form.value.dataNascimento || undefined,
    dataAdmissaoPrevista: form.value.dataAdmissaoPrevista || undefined,
    dependentes: form.value.dependentes.map((item) => ({
      ...item,
      nome: item.nome.trim(),
      cpf: somenteDigitos(item.cpf) || undefined,
      dataNascimento: item.dataNascimento || undefined,
    })),
  })
}

watch(() => visible.value, (aberta) => { if (aberta) preencher(props.admissao) })
watch(() => props.admissao, (admissao) => { if (visible.value) preencher(admissao) })
</script>

<template>
  <Dialog v-model:visible="visible" modal :header="admissao ? 'Editar admissão' : 'Nova admissão manual'" class="workflow-dialog admission-form-dialog" :style="{ width: 'min(920px, 96vw)' }">
    <form id="admission-form" class="dialog-form admission-form" @submit.prevent="salvar">
      <Message v-if="localError || error" severity="error" :closable="false">{{ localError || error }}</Message>

      <fieldset v-if="!admissao"><legend>Cliente</legend><p>Empresa onde o funcionário será admitido.</p><div class="field"><label for="admission-client">Cliente</label><Select input-id="admission-client" v-model="clienteId" :options="clientes" option-label="razaoSocial" option-value="id" filter placeholder="Selecione" /></div></fieldset>

      <fieldset><legend>Dados pessoais</legend><div class="form-grid three-columns">
        <div class="field field-span-2"><label for="employee-name">Nome completo</label><InputText id="employee-name" v-model="form.nomeFuncionario" required /></div>
        <div class="field"><label for="employee-cpf">CPF</label><InputText id="employee-cpf" :model-value="form.cpf" inputmode="numeric" maxlength="14" placeholder="000.000.000-00" required @update:model-value="form.cpf = formatarCpf($event)" /></div>
        <div class="field"><label for="employee-rg">RG</label><InputText id="employee-rg" v-model="form.rg" required /></div>
        <div class="field"><label for="employee-birth">Nascimento</label><InputText id="employee-birth" v-model="form.dataNascimento" type="date" required /></div>
        <div class="field"><label for="employee-sex">Sexo</label><Select input-id="employee-sex" v-model="form.sexo" :options="opcoesComValorAtual(form.sexo, sexosAdmissao)" placeholder="Selecione" required /></div>
        <div class="field"><label for="employee-marital">Estado civil</label><Select input-id="employee-marital" v-model="form.estadoCivil" :options="opcoesComValorAtual(form.estadoCivil, estadosCivisAdmissao)" placeholder="Selecione" /></div>
        <div class="field"><label for="employee-school">Escolaridade</label><Select input-id="employee-school" v-model="form.escolaridade" :options="opcoesComValorAtual(form.escolaridade, escolaridadesAdmissao)" placeholder="Selecione" /></div>
        <div class="field"><label for="employee-phone">Telefone</label><InputText id="employee-phone" :model-value="form.telefone" inputmode="tel" autocomplete="tel" maxlength="15" placeholder="(00) 00000-0000" @update:model-value="form.telefone = formatarTelefone($event)" /></div>
        <div class="field field-span-2"><label for="employee-email">E-mail</label><InputText id="employee-email" v-model="form.email" type="email" /></div>
      </div></fieldset>

      <fieldset><legend>Dados trabalhistas</legend><div class="form-grid three-columns">
        <div class="field"><label for="employee-pis">PIS/PASEP</label><InputText id="employee-pis" :model-value="form.pis" inputmode="numeric" maxlength="14" placeholder="000.00000.00-0" @update:model-value="form.pis = formatarPis($event)" /></div>
        <div class="field"><label for="admission-date">Data de admissão</label><InputText id="admission-date" v-model="form.dataAdmissaoPrevista" type="date" /></div>
        <div class="field"><label for="employee-role">Função</label><InputText id="employee-role" v-model="form.funcao" /></div>
        <div class="field"><label for="employee-salary">Salário</label><InputNumber input-id="employee-salary" v-model="form.salario" mode="currency" currency="BRL" locale="pt-BR" :min="0" /></div>
        <div class="field"><label for="employee-trial">Experiência (dias)</label><InputNumber input-id="employee-trial" v-model="form.contratoExperienciaDias" :min="0" /></div>
        <div class="field"><label for="employee-commission">Comissão</label><InputText id="employee-commission" v-model="form.comissao" /></div>
        <div class="field field-span-2"><label for="employee-schedule">Horário de trabalho</label><InputText id="employee-schedule" v-model="form.horarioTrabalho" /></div>
        <label class="check-field"><Checkbox v-model="form.utilizaValeTransporte" binary input-id="employee-transit" /><span>Utiliza vale-transporte</span></label>
      </div></fieldset>

      <fieldset><legend>Dados bancários</legend><div class="form-grid four-columns">
        <div class="field"><label for="employee-bank">Banco</label><InputText id="employee-bank" v-model="form.banco" /></div>
        <div class="field"><label for="employee-agency">Agência</label><InputText id="employee-agency" v-model="form.agencia" /></div>
        <div class="field"><label for="employee-account">Conta</label><InputText id="employee-account" v-model="form.conta" /></div>
        <div class="field"><label for="employee-account-type">Tipo</label><Select input-id="employee-account-type" v-model="form.tipoConta" :options="tiposContaAdmissao" placeholder="Selecione" /></div>
      </div></fieldset>

      <fieldset><div class="form-section-heading"><div><legend>Dependentes</legend><p>Cadastre somente quando aplicável.</p></div><Button label="Adicionar dependente" icon="pi pi-plus" text type="button" @click="adicionarDependente" /></div>
        <div v-if="!form.dependentes.length" class="form-empty">Nenhum dependente informado.</div>
        <div v-for="(dependente, index) in form.dependentes" :key="index" class="dependent-row">
          <div class="field dependent-name"><label :for="`dependent-name-${index}`">Nome</label><InputText :id="`dependent-name-${index}`" v-model="dependente.nome" /></div>
          <div class="field"><label :for="`dependent-cpf-${index}`">CPF</label><InputText :id="`dependent-cpf-${index}`" :model-value="dependente.cpf" inputmode="numeric" maxlength="14" placeholder="000.000.000-00" @update:model-value="dependente.cpf = formatarCpf($event)" /></div>
          <div class="field"><label :for="`dependent-birth-${index}`">Nascimento</label><InputText :id="`dependent-birth-${index}`" v-model="dependente.dataNascimento" type="date" /></div>
          <div class="field"><label :for="`dependent-relation-${index}`">Parentesco</label><InputText :id="`dependent-relation-${index}`" v-model="dependente.parentesco" /></div>
          <label class="check-field"><Checkbox v-model="dependente.dependenteIr" binary /><span>IR</span></label>
          <label class="check-field"><Checkbox v-model="dependente.salarioFamilia" binary /><span>Salário-família</span></label>
          <Button icon="pi pi-trash" text severity="danger" type="button" aria-label="Remover dependente" @click="removerDependente(index)" />
        </div>
      </fieldset>

      <div class="field"><label for="admission-notes">Observações</label><Textarea id="admission-notes" v-model="form.observacoes" rows="3" auto-resize /></div>
    </form>
    <template #footer><BaseButton label="Cancelar" variant="text" @click="visible = false" /><BaseButton :label="admissao ? 'Salvar alterações' : 'Criar admissão'" type="submit" form="admission-form" :loading="saving" /></template>
  </Dialog>
</template>
