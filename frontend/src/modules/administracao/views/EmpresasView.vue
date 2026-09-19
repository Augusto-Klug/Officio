<script setup lang="ts">
import { computed, onBeforeUnmount, ref } from 'vue'
import axios from 'axios'
import Column from 'primevue/column'
import DataTable from 'primevue/datatable'
import Dialog from 'primevue/dialog'
import InputText from 'primevue/inputtext'
import Message from 'primevue/message'
import BaseButton from '../../../components/base/BaseButton.vue'
import BaseCard from '../../../components/base/BaseCard.vue'
import BaseInput from '../../../components/base/BaseInput.vue'
import PageHeader from '../../../components/layout/PageHeader.vue'
import StatusBadge from '../../../components/shared/StatusBadge.vue'
import { normalizeError } from '../../../api/errors'
import { formatCnpj, formatDate } from '../../../utils/format'
import { alterarStatusEmpresa, criarEmpresa, listarEmpresas, reenviarConviteAdministrador } from '../services/empresas.service'
import type { EmpresaPlataforma } from '../types/empresa'

const empresas = ref<EmpresaPlataforma[]>([])
const loading = ref(true)
const error = ref('')
const success = ref('')
const search = ref('')
const createOpen = ref(false)
const confirmOpen = ref(false)
const selected = ref<EmpresaPlataforma | null>(null)
const savingId = ref<string | null>(null)
const saving = ref(false)
const dialogError = ref('')
const razaoSocial = ref('')
const nomeFantasia = ref('')
const cnpj = ref('')
const adminNome = ref('')
const adminEmail = ref('')
let controller: AbortController | undefined

const filtered = computed(() => {
  const term = search.value.trim().toLocaleLowerCase('pt-BR').replace(/[.\/-]/g, '')
  if (!term) return empresas.value
  return empresas.value.filter((item) => `${item.razaoSocial} ${item.nomeFantasia || ''} ${item.cnpj}`.toLocaleLowerCase('pt-BR').replace(/[.\/-]/g, '').includes(term))
})
const activeCount = computed(() => empresas.value.filter((item) => item.ativa).length)
const userCount = computed(() => empresas.value.reduce((total, item) => total + item.quantidadeUsuarios, 0))

async function load() {
  controller?.abort(); controller = new AbortController(); loading.value = true; error.value = ''
  try { empresas.value = await listarEmpresas(controller.signal) }
  catch (cause) { if (!axios.isCancel(cause)) error.value = normalizeError(cause).message }
  finally { loading.value = false }
}
function openCreate() {
  razaoSocial.value = ''; nomeFantasia.value = ''; cnpj.value = ''; adminNome.value = ''; adminEmail.value = ''; dialogError.value = ''; createOpen.value = true
}
async function submitCreate() {
  if (saving.value) return
  if (!razaoSocial.value.trim() || cnpj.value.replace(/\D/g, '').length !== 14 || !adminNome.value.trim() || !adminEmail.value.trim()) {
    dialogError.value = 'Preencha a razão social, um CNPJ válido e os dados do administrador.'; return
  }
  saving.value = true; dialogError.value = ''
  try {
    await criarEmpresa({ razaoSocial: razaoSocial.value, nomeFantasia: nomeFantasia.value || null, cnpj: cnpj.value, administrador: { nome: adminNome.value, email: adminEmail.value } })
    createOpen.value = false; success.value = 'Empresa criada e convite do primeiro administrador enviado.'; await load()
  } catch (cause) { dialogError.value = normalizeError(cause).message }
  finally { saving.value = false }
}
async function resend(item: EmpresaPlataforma) {
  if (savingId.value) return
  savingId.value = item.id; error.value = ''
  try { await reenviarConviteAdministrador(item.id); success.value = `Convite reenviado para ${item.administradorEmail}.` }
  catch (cause) { error.value = normalizeError(cause).message }
  finally { savingId.value = null }
}
function requestStatus(item: EmpresaPlataforma) { selected.value = item; dialogError.value = ''; confirmOpen.value = true }
async function confirmStatus() {
  if (!selected.value || savingId.value) return
  savingId.value = selected.value.id; dialogError.value = ''
  try {
    await alterarStatusEmpresa(selected.value.id, !selected.value.ativa)
    success.value = `Empresa ${selected.value.ativa ? 'suspensa' : 'reativada'}.`; confirmOpen.value = false; await load()
  } catch (cause) { dialogError.value = normalizeError(cause).message }
  finally { savingId.value = null }
}

void load()
onBeforeUnmount(() => controller?.abort())
</script>

<template>
  <div class="platform-context"><i class="pi pi-shield" aria-hidden="true" /><span>Administração da plataforma</span></div>
  <PageHeader title="Empresas" description="Gerencie os tenants e o primeiro acesso administrativo de cada operação.">
    <BaseButton label="Cadastrar empresa" icon="pi pi-plus" @click="openCreate" />
  </PageHeader>
  <Message v-if="success" severity="success" closable class="page-message" @close="success = ''">{{ success }}</Message>
  <Message v-if="error" severity="error" :closable="false" class="page-message">{{ error }}</Message>

  <div class="access-summary" aria-label="Resumo da plataforma">
    <div><strong class="numeric">{{ empresas.length }}</strong><span>Empresas</span></div>
    <div><strong class="numeric">{{ activeCount }}</strong><span>Operações ativas</span></div>
    <div><strong class="numeric">{{ userCount }}</strong><span>Usuários cadastrados</span></div>
  </div>

  <BaseCard class="data-card">
    <div class="filters platform-filters"><div class="field"><label for="company-search">Buscar empresa</label><InputText id="company-search" v-model="search" placeholder="Razão social, nome fantasia ou CNPJ" /></div></div>
    <div class="list-heading"><h2>Tenants da plataforma</h2><span>{{ filtered.length }} {{ filtered.length === 1 ? 'empresa' : 'empresas' }}</span></div>
    <div v-if="loading" class="list-state" aria-busy="true"><i class="pi pi-spinner loading-spinner" /><p>Carregando empresas...</p></div>
    <div v-else-if="error" class="list-state"><BaseButton label="Tentar novamente" icon="pi pi-refresh" variant="secondary" @click="load" /></div>
    <DataTable v-else :value="filtered" data-key="id" class="work-table companies-table" :table-props="{ 'aria-label': 'Empresas da plataforma' }">
      <Column field="razaoSocial" header="Empresa"><template #body="{ data }"><div class="company-cell"><strong>{{ data.razaoSocial }}</strong><span>{{ data.nomeFantasia || formatCnpj(data.cnpj) }}</span></div></template></Column>
      <Column field="ativa" header="Situação"><template #body="{ data }"><StatusBadge :label="data.ativa ? 'Ativa' : 'Suspensa'" :severity="data.ativa ? 'success' : 'danger'" /></template></Column>
      <Column field="administradorEmail" header="Primeiro administrador"><template #body="{ data }"><div class="company-cell"><strong>{{ data.administradorEmail || 'Não informado' }}</strong><span>{{ data.quantidadeUsuarios }} {{ data.quantidadeUsuarios === 1 ? 'usuário' : 'usuários' }}</span></div></template></Column>
      <Column field="criadaEm" header="Cadastro"><template #body="{ data }"><span class="muted">{{ formatDate(data.criadaEm) }}</span></template></Column>
      <Column header="Ações"><template #body="{ data }"><div class="row-actions"><BaseButton v-if="data.administradorEmail" label="Reenviar convite" icon="pi pi-send" variant="text" :loading="savingId === data.id" @click="resend(data)" /><BaseButton :label="data.ativa ? 'Suspender' : 'Reativar'" :icon="data.ativa ? 'pi pi-ban' : 'pi pi-check'" variant="text" :disabled="!!savingId" @click="requestStatus(data)" /></div></template></Column>
      <template #empty><div class="empty-state"><i class="pi pi-building" /><h3>Nenhuma empresa encontrada</h3><p>{{ search ? 'Revise os termos da busca.' : 'Cadastre a primeira empresa para iniciar o onboarding.' }}</p><BaseButton v-if="!search" label="Cadastrar empresa" icon="pi pi-plus" @click="openCreate" /></div></template>
    </DataTable>
  </BaseCard>

  <Dialog v-model:visible="createOpen" modal header="Cadastrar empresa" class="workflow-dialog company-create-dialog" :style="{ width: 'min(720px, 95vw)' }">
    <form id="company-form" class="dialog-form" @submit.prevent="submitCreate">
      <Message v-if="dialogError" severity="error" :closable="false">{{ dialogError }}</Message>
      <fieldset><legend>Dados da empresa</legend><div class="form-grid two-columns">
        <BaseInput id="company-name" v-model="razaoSocial" label="Razão social" required :disabled="saving" />
        <BaseInput id="company-trade-name" v-model="nomeFantasia" label="Nome fantasia" :disabled="saving" />
        <BaseInput id="company-cnpj" v-model="cnpj" label="CNPJ" inputmode="numeric" placeholder="00.000.000/0000-00" required :disabled="saving" />
      </div></fieldset>
      <fieldset><legend>Primeiro administrador</legend><p>Esta pessoa receberá o convite para ativar a conta e gerenciar a equipe da empresa.</p><div class="form-grid two-columns">
        <BaseInput id="admin-name" v-model="adminNome" label="Nome completo" autocomplete="name" required :disabled="saving" />
        <BaseInput id="admin-email" v-model="adminEmail" label="E-mail" type="email" autocomplete="email" required :disabled="saving" />
      </div></fieldset>
    </form>
    <template #footer><BaseButton label="Cancelar" variant="text" :disabled="saving" @click="createOpen = false" /><BaseButton label="Criar e convidar" icon="pi pi-send" type="submit" form="company-form" :loading="saving" /></template>
  </Dialog>

  <Dialog v-model:visible="confirmOpen" modal :header="selected?.ativa ? 'Suspender empresa' : 'Reativar empresa'" class="workflow-dialog" :style="{ width: 'min(460px, 94vw)' }">
    <Message v-if="dialogError" severity="error" :closable="false">{{ dialogError }}</Message>
    <p v-if="selected" class="confirmation-copy">{{ selected.ativa ? `Os usuários de ${selected.razaoSocial} perderão o acesso até a empresa ser reativada.` : `Os usuários ativos de ${selected.razaoSocial} poderão voltar a acessar o Officio.` }}</p>
    <template #footer><BaseButton label="Voltar" variant="text" :disabled="!!savingId" @click="confirmOpen = false" /><BaseButton :label="selected?.ativa ? 'Suspender empresa' : 'Reativar empresa'" :icon="selected?.ativa ? 'pi pi-ban' : 'pi pi-check'" :loading="!!savingId" @click="confirmStatus" /></template>
  </Dialog>
</template>
