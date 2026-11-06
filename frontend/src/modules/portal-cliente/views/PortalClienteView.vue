<script setup lang="ts">
import { computed, onMounted, ref, watch } from 'vue'
import Column from 'primevue/column'
import DataTable from 'primevue/datatable'
import Dialog from 'primevue/dialog'
import InputNumber from 'primevue/inputnumber'
import InputText from 'primevue/inputtext'
import Message from 'primevue/message'
import Select from 'primevue/select'
import Tab from 'primevue/tab'
import TabList from 'primevue/tablist'
import TabPanel from 'primevue/tabpanel'
import TabPanels from 'primevue/tabpanels'
import Tabs from 'primevue/tabs'
import BaseButton from '../../../components/base/BaseButton.vue'
import BaseCard from '../../../components/base/BaseCard.vue'
import PageHeader from '../../../components/layout/PageHeader.vue'
import StatusBadge from '../../../components/shared/StatusBadge.vue'
import { normalizeError } from '../../../api/errors'
import { formatCnpj, formatCurrency, formatDate } from '../../../utils/format'
import { formatarCpf, formatarPis, formatarTelefone, somenteDigitos } from '../../../utils/inputMasks'
import type { AdmissaoResumo } from '../../admissoes/types/admissao'
import type { Contrato } from '../../contratos/types/contrato'
import { tiposContaAdmissao } from '../../admissoes/types/opcoesAdmissao'
import { atualizarDadosEmpregadorPortal, concluirDadosEmpregadorPortal, criarAdmissaoPortal, obterAdmissaoPortal, obterPdfAssinadoPortal, obterVisaoPortalCliente } from '../services/portal-cliente.service'
import type { DetalheAdmissaoPortal, EmpresaPortalCliente } from '../types/portal-cliente'

const empresas = ref<EmpresaPortalCliente[]>([])
const empresaSelecionada = ref('')
const admissoes = ref<AdmissaoResumo[]>([])
const contratos = ref<Contrato[]>([])
const abaAtiva = ref('admissoes')
const loading = ref(true)
const saving = ref(false)
const error = ref('')
const success = ref('')
const createOpen = ref(false)
const detailOpen = ref(false)
const contratoDetailOpen = ref(false)
const contratoSelecionado = ref<Contrato | null>(null)
const detalhe = ref<DetalheAdmissaoPortal | null>(null)
const createForm = ref({ emailFuncionario: '' })
const employerForm = ref({
  pis: '', dataAdmissaoPrevista: '', salario: null as number | null,
  comissao: '', funcao: '', contratoExperienciaDias: null as number | null,
  utilizaValeTransporte: null as boolean | null, horarioTrabalho: '',
})

const empresaOptions = computed(() => empresas.value.map((empresa) => ({
  label: `${empresa.razaoSocial} · ${formatCnpj(empresa.cnpj) || 'CNPJ não informado'}`,
  value: empresa.id,
})))
const fichaFuncionarioEnviada = computed(() => detalhe.value?.preenchimentoFuncionarioConcluido === true)
const empregadorConcluiu = computed(() => detalhe.value?.preenchimentoEmpresaConcluido === true)
const empregadorEditavel = computed(() => fichaFuncionarioEnviada.value && !empregadorConcluiu.value && !['CONCLUIDA', 'CANCELADA'].includes(detalhe.value?.status ?? ''))
const statusLabel = (codigo: string) => ({
  RASCUNHO: 'Rascunho', AGUARDANDO_ADMITIDO: 'Aguardando funcionário',
  AGUARDANDO_VALIDACAO: 'Em revisão documental', DOCUMENTOS_VALIDADOS: 'Documentos validados',
  AGUARDANDO_EMPREGADOR: 'Aguardando empregador', LIBERADO_PARA_REVISAO_FINAL: 'Em revisão final',
  CONCLUIDA: 'Concluída', CANCELADA: 'Cancelada',
}[codigo] ?? codigo)
const statusSeverity = (codigo: string) => ['CONCLUIDA', 'LIBERADO_PARA_REVISAO_FINAL'].includes(codigo)
  ? 'success' : codigo === 'CANCELADA' ? 'danger' : 'warn'

function exibirValor(valor: string | null | undefined) {
  return valor?.trim() || 'Não informado'
}

function exibirOpcao(valor: boolean | null | undefined) {
  return valor == null ? 'Não informado' : valor ? 'Sim' : 'Não'
}

async function carregar() {
  loading.value = true
  error.value = ''
  try {
    const visao = await obterVisaoPortalCliente(empresaSelecionada.value || undefined)
    empresas.value = visao.empresas
    admissoes.value = visao.admissoes
    contratos.value = visao.contratos
    if (!empresaSelecionada.value && visao.empresas.length === 1) empresaSelecionada.value = visao.empresas[0]!.id
  } catch (cause) {
    error.value = normalizeError(cause).message
  } finally {
    loading.value = false
  }
}

function abrirNovaAdmissao() {
  createForm.value = { emailFuncionario: '' }
  error.value = ''
  createOpen.value = true
}

async function criarAdmissao() {
  if (!empresaSelecionada.value) return
  saving.value = true
  error.value = ''
  try {
    const criada = await criarAdmissaoPortal({
      empresaId: empresaSelecionada.value,
      emailFuncionario: createForm.value.emailFuncionario.trim(),
    })
    createOpen.value = false
    success.value = criada.linkFuncionarioEnviado
      ? `Admissão criada. Enviamos a ficha para ${criada.emailFuncionario}.`
      : 'Admissão criada, mas não foi possível enviar o e-mail ao funcionário. Contate a contabilidade para reenviar o link.'
    await carregar()
  } catch (cause) {
    error.value = normalizeError(cause).message
  } finally {
    saving.value = false
  }
}

function abrirContrato(contrato: Contrato) {
  contratoSelecionado.value = contrato
  contratoDetailOpen.value = true
}

async function visualizarPdfAssinado() {
  if (!contratoSelecionado.value) return
  saving.value = true
  error.value = ''
  try {
    const url = await obterPdfAssinadoPortal(empresaSelecionada.value, contratoSelecionado.value.id)
    window.open(url, '_blank', 'noopener,noreferrer')
  } catch (cause) {
    error.value = normalizeError(cause).message
  } finally {
    saving.value = false
  }
}

function contratoSeverity(status: string) {
  if (status === 'Ativo') return 'success'
  if (status === 'AguardandoAssinatura') return 'warn'
  if (status === 'Cancelado' || status === 'Encerrado') return 'danger'
  return 'secondary'
}

function preencherEmpregador(admissao: DetalheAdmissaoPortal) {
  employerForm.value = {
    pis: formatarPis(admissao.pis),
    dataAdmissaoPrevista: admissao.dataAdmissaoPrevista ?? '',
    salario: admissao.salario,
    comissao: admissao.comissao ?? '',
    funcao: admissao.funcao ?? '',
    contratoExperienciaDias: admissao.contratoExperienciaDias,
    utilizaValeTransporte: admissao.utilizaValeTransporte,
    horarioTrabalho: admissao.horarioTrabalho ?? '',
  }
}

async function abrirAdmissao(item: AdmissaoResumo) {
  if (!empresaSelecionada.value) return
  detailOpen.value = true
  detalhe.value = null
  error.value = ''
  try {
    detalhe.value = await obterAdmissaoPortal(empresaSelecionada.value, item.id)
    preencherEmpregador(detalhe.value)
  } catch (cause) {
    error.value = normalizeError(cause).message
    detailOpen.value = false
  }
}

async function salvarEmpregador(concluir = false) {
  if (!detalhe.value) return
  saving.value = true
  error.value = ''
  try {
    await atualizarDadosEmpregadorPortal(detalhe.value.id, {
      pis: somenteDigitos(employerForm.value.pis) || undefined,
      dataAdmissaoPrevista: employerForm.value.dataAdmissaoPrevista || undefined,
      salario: employerForm.value.salario ?? undefined,
      comissao: employerForm.value.comissao || undefined,
      funcao: employerForm.value.funcao || undefined,
      contratoExperienciaDias: employerForm.value.contratoExperienciaDias ?? undefined,
      utilizaValeTransporte: employerForm.value.utilizaValeTransporte ?? undefined,
      horarioTrabalho: employerForm.value.horarioTrabalho || undefined,
    })
    if (concluir) await concluirDadosEmpregadorPortal(detalhe.value.id)
    success.value = concluir ? 'Dados do empregador enviados à contabilidade.' : 'Dados do empregador salvos.'
    await carregar()
    detalhe.value = await obterAdmissaoPortal(empresaSelecionada.value, detalhe.value.id)
    preencherEmpregador(detalhe.value)
  } catch (cause) {
    error.value = normalizeError(cause).message
  } finally {
    saving.value = false
  }
}

watch(empresaSelecionada, () => { if (empresaSelecionada.value) void carregar() })
onMounted(() => void carregar())
</script>

<template>
  <PageHeader title="Área de trabalho" description="Acompanhe as admissões e os contratos da empresa.">
    <BaseButton icon="pi pi-refresh" variant="secondary" title="Atualizar dados" aria-label="Atualizar dados" :loading="loading" @click="carregar" />
    <BaseButton v-if="abaAtiva === 'admissoes'" label="Nova admissão" icon="pi pi-plus" :disabled="!empresaSelecionada" @click="abrirNovaAdmissao" />
  </PageHeader>

  <Message v-if="success" severity="success" :closable="true" class="page-message" @close="success = ''">{{ success }}</Message>
  <Message v-if="error" severity="error" :closable="true" class="page-message" @close="error = ''">{{ error }}</Message>
  <div class="portal-company-filter">
    <label for="portal-company">Empresa</label>
    <Select input-id="portal-company" v-model="empresaSelecionada" :options="empresaOptions" option-label="label" option-value="value" placeholder="Selecione a empresa" :disabled="empresas.length <= 1" />
  </div>

  <Tabs v-model:value="abaAtiva" class="client-workspace-tabs">
    <TabList aria-label="Seções da empresa">
      <Tab value="admissoes">Admissões</Tab>
      <Tab value="contratos">Contratos</Tab>
    </TabList>
    <TabPanels>
      <TabPanel value="admissoes">
  <section class="portal-section">
    <div class="portal-section-heading"><h2>Admissões</h2><span>{{ admissoes.length }}</span></div>
    <BaseCard class="portal-table">
      <div v-if="loading" class="portal-loading" aria-busy="true"><i class="pi pi-spinner loading-spinner" />Carregando admissões...</div>
      <DataTable v-else :value="admissoes" data-key="id" :table-props="{ 'aria-label': 'Admissões da empresa' }">
        <Column field="nomeFuncionario" header="Funcionário"><template #body="{ data }">{{ data.nomeFuncionario || 'Não informado' }}</template></Column>
        <Column field="cpf" header="CPF"><template #body="{ data }">{{ formatarCpf(data.cpf) || 'Não informado' }}</template></Column>
        <Column field="status" header="Situação"><template #body="{ data }"><StatusBadge :label="statusLabel(data.status)" :severity="statusSeverity(data.status)" /></template></Column>
        <Column field="dataAdmissaoPrevista" header="Admissão"><template #body="{ data }">{{ formatDate(data.dataAdmissaoPrevista) }}</template></Column>
        <Column header="Ações"><template #body="{ data }"><BaseButton label="Abrir" icon="pi pi-arrow-right" variant="text" @click="abrirAdmissao(data)" /></template></Column>
        <template #empty><div class="portal-empty">Nenhuma admissão cadastrada para esta empresa.</div></template>
      </DataTable>
    </BaseCard>
  </section>
      </TabPanel>
      <TabPanel value="contratos">

  <section class="portal-section">
    <div class="portal-section-heading"><h2>Contratos</h2><span>{{ contratos.length }}</span></div>
    <BaseCard class="portal-table">
      <DataTable :value="contratos" data-key="id" :table-props="{ 'aria-label': 'Contratos da empresa' }">
        <Column field="numero" header="Número" />
        <Column field="status" header="Situação"><template #body="{ data }"><StatusBadge :label="data.status" :severity="contratoSeverity(data.status)" /></template></Column>
        <Column header="Assinaturas"><template #body="{ data }"><div v-if="data.signatariosAssinatura.length" class="portal-signatures"><span v-for="signatario in data.signatariosAssinatura" :key="`${signatario.tipo}-${signatario.nome}`">{{ signatario.tipo }}: {{ signatario.assinado ? 'Assinado' : 'Pendente' }}</span></div><span v-else class="muted">—</span></template></Column>
        <Column field="valorTotal" header="Valor"><template #body="{ data }">{{ formatCurrency(data.valorTotal) }}</template></Column>
        <Column field="dataInicio" header="Início"><template #body="{ data }">{{ formatDate(data.dataInicio) }}</template></Column>
        <Column field="dataFim" header="Término"><template #body="{ data }">{{ formatDate(data.dataFim) }}</template></Column>
        <Column header="Ações"><template #body="{ data }"><BaseButton label="Visualizar" icon="pi pi-eye" variant="text" @click="abrirContrato(data)" /></template></Column>
        <template #empty><div class="portal-empty">Nenhum contrato encontrado.</div></template>
      </DataTable>
    </BaseCard>
  </section>
      </TabPanel>
    </TabPanels>
  </Tabs>

  <Dialog v-model:visible="createOpen" modal header="Nova admissão" :style="{ width: 'min(36rem, calc(100vw - 2rem))' }" :breakpoints="{ '640px': 'calc(100vw - 1rem)' }">
    <form class="portal-form" @submit.prevent="criarAdmissao">
      <p>A ficha será enviada para o funcionário preencher os dados pessoais, bancários e anexar os documentos.</p>
      <div class="field"><label for="new-employee-email">E-mail do funcionário</label><InputText id="new-employee-email" v-model="createForm.emailFuncionario" required type="email" maxlength="450" autocomplete="email" /></div>
      <footer class="portal-dialog-actions"><BaseButton label="Cancelar" variant="secondary" type="button" @click="createOpen = false" /><BaseButton label="Criar e enviar ficha" icon="pi pi-send" :loading="saving" type="submit" /></footer>
    </form>
  </Dialog>

  <Dialog v-model:visible="contratoDetailOpen" modal header="Contrato" :style="{ width: 'min(44rem, calc(100vw - 2rem))' }" :breakpoints="{ '640px': 'calc(100vw - 1rem)' }">
    <div v-if="contratoSelecionado" class="contract-readonly">
      <div class="contract-readonly-heading"><div><strong>{{ contratoSelecionado.numero }}</strong><span>{{ contratoSelecionado.razaoSocial }}</span></div><StatusBadge :label="contratoSelecionado.status" :severity="contratoSeverity(contratoSelecionado.status)" /></div>
      <dl class="contract-readonly-grid">
        <div><dt>Valor total</dt><dd>{{ formatCurrency(contratoSelecionado.valorTotal) }}</dd></div>
        <div><dt>Início</dt><dd>{{ formatDate(contratoSelecionado.dataInicio) }}</dd></div>
        <div><dt>Término</dt><dd>{{ formatDate(contratoSelecionado.dataFim) }}</dd></div>
        <div><dt>Assinado em</dt><dd>{{ formatDate(contratoSelecionado.assinadoEm) }}</dd></div>
      </dl>
      <section class="contract-signers"><h3>Assinaturas</h3><p v-for="signatario in contratoSelecionado.signatariosAssinatura" :key="`${signatario.tipo}-${signatario.nome}`">{{ signatario.tipo }} · {{ signatario.nome }} · {{ signatario.assinado ? 'Assinado' : 'Pendente' }}</p><p v-if="!contratoSelecionado.signatariosAssinatura.length" class="muted">Nenhuma assinatura registrada.</p></section>
      <BaseButton v-if="contratoSelecionado.pdfAssinadoDisponivel" label="Visualizar PDF assinado" icon="pi pi-external-link" variant="secondary" :loading="saving" @click="visualizarPdfAssinado" />
    </div>
  </Dialog>

  <Dialog v-model:visible="detailOpen" modal header="Admissão" :style="{ width: 'min(58rem, calc(100vw - 2rem))' }" :breakpoints="{ '720px': 'calc(100vw - 1rem)' }">
    <div v-if="detalhe" class="portal-detail">
      <div class="portal-detail-summary"><div><strong>{{ detalhe.nomeFuncionario }}</strong><span>{{ formatarCpf(detalhe.cpf) || 'CPF não informado' }}</span></div><StatusBadge :label="statusLabel(detalhe.status)" :severity="statusSeverity(detalhe.status)" /></div>
      <Message v-if="!fichaFuncionarioEnviada" severity="info" :closable="false">O funcionário ainda precisa preencher a ficha e enviar os documentos. Os dados do empregador serão liberados depois do envio.</Message>
      <Message v-else-if="empregadorConcluiu" severity="success" :closable="false">Dados enviados. A contabilidade fará a revisão final da admissão.</Message>
      <section v-if="fichaFuncionarioEnviada" class="portal-employee-readonly" aria-labelledby="employee-info-heading">
        <div class="portal-readonly-heading">
          <div><h3 id="employee-info-heading">Dados enviados pelo funcionário</h3><p>Ficha preenchida pelo funcionário; consulta somente leitura.</p></div>
        </div>
        <dl class="portal-employee-grid">
          <div><dt>Nome completo</dt><dd>{{ exibirValor(detalhe.nomeFuncionario) }}</dd></div>
          <div><dt>CPF</dt><dd>{{ formatarCpf(detalhe.cpf) || 'Não informado' }}</dd></div>
          <div><dt>RG</dt><dd>{{ exibirValor(detalhe.rg) }}</dd></div>
          <div><dt>Data de nascimento</dt><dd>{{ formatDate(detalhe.dataNascimento) }}</dd></div>
          <div><dt>Sexo</dt><dd>{{ exibirValor(detalhe.sexo) }}</dd></div>
          <div><dt>Etnia</dt><dd>{{ exibirValor(detalhe.etnia) }}</dd></div>
          <div><dt>Estado civil</dt><dd>{{ exibirValor(detalhe.estadoCivil) }}</dd></div>
          <div><dt>Escolaridade</dt><dd>{{ exibirValor(detalhe.escolaridade) }}</dd></div>
          <div><dt>Telefone</dt><dd>{{ formatarTelefone(detalhe.telefone) || 'Não informado' }}</dd></div>
          <div><dt>E-mail</dt><dd>{{ exibirValor(detalhe.email) }}</dd></div>
          <div><dt>Banco</dt><dd>{{ exibirValor(detalhe.banco) }}</dd></div>
          <div><dt>Agência</dt><dd>{{ exibirValor(detalhe.agencia) }}</dd></div>
          <div><dt>Conta</dt><dd>{{ exibirValor(detalhe.conta) }}</dd></div>
          <div><dt>Tipo de conta</dt><dd>{{ exibirValor(detalhe.tipoConta) }}</dd></div>
        </dl>
        <div class="portal-dependents-readonly">
          <h4>Dependentes</h4>
          <div v-if="detalhe.dependentes.length" class="portal-dependent-list">
            <article v-for="dependente in detalhe.dependentes" :key="dependente.id" class="portal-dependent-row">
              <strong>{{ exibirValor(dependente.nome) }}</strong>
              <span>CPF: {{ formatarCpf(dependente.cpf) || 'Não informado' }} | Nascimento: {{ formatDate(dependente.dataNascimento) }}</span>
              <span>Parentesco: {{ exibirValor(dependente.parentesco) }} | Dependente IRPF: {{ exibirOpcao(dependente.dependenteIr) }} | Salário-família: {{ exibirOpcao(dependente.salarioFamilia) }}</span>
            </article>
          </div>
          <p v-else class="muted">Nenhum dependente informado.</p>
        </div>
      </section>
      <section class="portal-employer-form"><h3>Dados da empresa</h3>
        <fieldset :disabled="!empregadorEditavel || saving" class="portal-form-grid">
          <div class="field"><label for="employer-pis">PIS/PASEP</label><InputText id="employer-pis" :model-value="employerForm.pis" inputmode="numeric" maxlength="14" placeholder="000.00000.00-0" @update:model-value="employerForm.pis = formatarPis($event)" /></div>
          <div class="field"><label for="employer-date">Data de admissão *</label><InputText id="employer-date" v-model="employerForm.dataAdmissaoPrevista" type="date" required /></div>
          <div class="field"><label for="employer-salary">Salário</label><InputNumber input-id="employer-salary" v-model="employerForm.salario" mode="currency" currency="BRL" locale="pt-BR" :min="0" /></div>
          <div class="field"><label for="employer-function">Função *</label><InputText id="employer-function" v-model="employerForm.funcao" required maxlength="200" /></div>
          <div class="field"><label for="employer-commission">Comissão</label><InputText id="employer-commission" v-model="employerForm.comissao" /></div>
          <div class="field"><label for="employer-trial">Experiência (dias)</label><InputNumber input-id="employer-trial" v-model="employerForm.contratoExperienciaDias" :min="0" :max="90" /></div>
          <div class="field"><label for="employer-vt">Vale-transporte</label><Select input-id="employer-vt" v-model="employerForm.utilizaValeTransporte" :options="[{ label: 'Sim', value: true }, { label: 'Não', value: false }]" option-label="label" option-value="value" placeholder="Selecione" :disabled="!empregadorEditavel || saving" /></div>
          <div class="field field-span-2"><label for="employer-hours">Horário de trabalho</label><InputText id="employer-hours" v-model="employerForm.horarioTrabalho" maxlength="500" /></div>
        </fieldset>
        <footer v-if="empregadorEditavel" class="portal-dialog-actions"><BaseButton label="Salvar" icon="pi pi-save" variant="secondary" :loading="saving" @click="salvarEmpregador(false)" /><BaseButton label="Enviar à contabilidade" icon="pi pi-send" :loading="saving" @click="salvarEmpregador(true)" /></footer>
      </section>
    </div>
  </Dialog>
</template>

<style scoped>
.portal-company-filter { display: grid; grid-template-columns: minmax(12rem, 20rem); gap: .4rem; margin: .5rem 0 1.5rem; }
.portal-company-filter label, .field label { font-size: .875rem; font-weight: 600; color: var(--text-color); }
.portal-company-filter :deep(.p-select) { width: 100%; }
.portal-section { margin-top: 1.5rem; }
.portal-section-heading { display: flex; align-items: baseline; gap: .7rem; margin-bottom: .65rem; }
.portal-section-heading h2 { margin: 0; font-size: 1.1rem; }
.portal-section-heading span { color: var(--text-color-secondary); font-size: .85rem; font-variant-numeric: tabular-nums; }
.portal-table { padding: 0; overflow: hidden; }
.portal-loading, .portal-empty { padding: 1.5rem; color: var(--text-color-secondary); }
.portal-loading { display: flex; align-items: center; gap: .6rem; }
.portal-form { display: grid; gap: 1rem; }
.portal-form p { margin: 0; color: var(--text-color-secondary); line-height: 1.5; }
.field { display: grid; gap: .4rem; min-width: 0; }
.field :deep(.p-inputtext), .field :deep(.p-inputnumber), .field :deep(.p-select) { width: 100%; }
.portal-dialog-actions { display: flex; justify-content: flex-end; flex-wrap: wrap; gap: .6rem; margin-top: 1.25rem; }
.portal-detail { display: grid; gap: 1.25rem; }
.portal-detail-summary { display: flex; justify-content: space-between; align-items: center; gap: 1rem; }
.portal-detail-summary div { display: grid; gap: .2rem; }
.portal-detail-summary span { color: var(--text-color-secondary); font-size: .9rem; }
.portal-employee-readonly { border-top: 1px solid var(--surface-border); padding-top: 1rem; }
.portal-readonly-heading { margin-bottom: .85rem; }
.portal-readonly-heading h3, .portal-dependents-readonly h4 { margin: 0; font-size: 1rem; }
.portal-readonly-heading p { margin: .25rem 0 0; color: var(--text-color-secondary); font-size: .875rem; }
.portal-employee-grid { display: grid; grid-template-columns: repeat(3, minmax(0, 1fr)); gap: .85rem 1rem; margin: 0; }
.portal-employee-grid div { min-width: 0; }
.portal-employee-grid dt { color: var(--text-color-secondary); font-size: .8rem; }
.portal-employee-grid dd { margin: .2rem 0 0; overflow-wrap: anywhere; }
.portal-dependents-readonly { margin-top: 1.2rem; }
.portal-dependents-readonly h4 { margin-bottom: .65rem; }
.portal-dependent-list { display: grid; gap: .6rem; }
.portal-dependent-row { display: grid; gap: .2rem; padding: .7rem 0; border-top: 1px solid var(--surface-border); }
.portal-dependent-row span { color: var(--text-color-secondary); font-size: .875rem; overflow-wrap: anywhere; }
.portal-dependents-readonly > p { margin: 0; }
.portal-employer-form { border-top: 1px solid var(--surface-border); padding-top: 1rem; }
.portal-employer-form h3 { margin: 0 0 1rem; font-size: 1rem; }
.portal-form-grid { display: grid; grid-template-columns: repeat(3, minmax(0, 1fr)); gap: 1rem; border: 0; margin: 0; padding: 0; min-width: 0; }
.portal-signatures { display: grid; gap: .2rem; font-size: .85rem; }
.contract-readonly { display: grid; gap: 1.25rem; }
.contract-readonly-heading { display: flex; justify-content: space-between; align-items: center; gap: 1rem; }
.contract-readonly-heading div { display: grid; gap: .25rem; }
.contract-readonly-heading span { color: var(--text-color-secondary); }
.contract-readonly-grid { display: grid; grid-template-columns: repeat(2, minmax(0, 1fr)); gap: 1rem; margin: 0; }
.contract-readonly-grid div { min-width: 0; }
.contract-readonly-grid dt { color: var(--text-color-secondary); font-size: .85rem; }
.contract-readonly-grid dd { margin: .2rem 0 0; font-weight: 600; overflow-wrap: anywhere; }
.contract-signers { border-top: 1px solid var(--surface-border); padding-top: 1rem; }
.contract-signers h3 { margin: 0 0 .6rem; font-size: 1rem; }
.contract-signers p { margin: .3rem 0; }
.field-span-2 { grid-column: span 2; }
@media (max-width: 720px) {
  .portal-form-grid { grid-template-columns: repeat(2, minmax(0, 1fr)); }
  .portal-employee-grid { grid-template-columns: repeat(2, minmax(0, 1fr)); }
  .portal-detail-summary { align-items: flex-start; }
}
@media (max-width: 480px) {
  .portal-form-grid { grid-template-columns: minmax(0, 1fr); }
  .portal-employee-grid { grid-template-columns: minmax(0, 1fr); }
  .field-span-2 { grid-column: auto; }
}
</style>
