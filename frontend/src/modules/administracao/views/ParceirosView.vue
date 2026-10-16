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
import { alterarStatusParceiro, convidarAdministradorParceiro, criarParceiro, listarParceiros, reenviarConviteAdministrador } from '../services/parceiros.service'
import type { ParceiroPlataforma } from '../types/parceiro'

const parceiros = ref<ParceiroPlataforma[]>([])
const loading = ref(true)
const error = ref('')
const success = ref('')
const search = ref('')
const createOpen = ref(false)
const confirmOpen = ref(false)
const adminInviteOpen = ref(false)
const selected = ref<ParceiroPlataforma | null>(null)
const invitePartner = ref<ParceiroPlataforma | null>(null)
const savingId = ref<string | null>(null)
const saving = ref(false)
const invitingAdmin = ref(false)
const dialogError = ref('')
const inviteError = ref('')
const razaoSocial = ref('')
const nomeFantasia = ref('')
const cnpj = ref('')
const adminNome = ref('')
const adminEmail = ref('')
const inviteAdminNome = ref('')
const inviteAdminEmail = ref('')
let controller: AbortController | undefined

const filtered = computed(() => {
  const term = search.value.trim().toLocaleLowerCase('pt-BR').replace(/[.\/-]/g, '')
  if (!term) return parceiros.value
  return parceiros.value.filter((item) => `${item.razaoSocial} ${item.nomeFantasia || ''} ${item.cnpj}`.toLocaleLowerCase('pt-BR').replace(/[.\/-]/g, '').includes(term))
})
const activeCount = computed(() => parceiros.value.filter((item) => item.ativo).length)
const userCount = computed(() => parceiros.value.reduce((total, item) => total + item.quantidadeUsuarios, 0))

async function load() {
  controller?.abort(); controller = new AbortController(); loading.value = true; error.value = ''
  try { parceiros.value = await listarParceiros(controller.signal) }
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
    await criarParceiro({ razaoSocial: razaoSocial.value, nomeFantasia: nomeFantasia.value || null, cnpj: cnpj.value, administrador: { nome: adminNome.value, email: adminEmail.value } })
    createOpen.value = false; success.value = 'Parceiro criado e convite do primeiro administrador enviado.'; await load()
  } catch (cause) { dialogError.value = normalizeError(cause).message }
  finally { saving.value = false }
}
async function resend(item: ParceiroPlataforma) {
  if (savingId.value) return
  savingId.value = item.id; error.value = ''
  try { await reenviarConviteAdministrador(item.id); success.value = `Convite reenviado para ${item.administradorEmail}.` }
  catch (cause) { error.value = normalizeError(cause).message }
  finally { savingId.value = null }
}
function openAdminInvite(item: ParceiroPlataforma) {
  invitePartner.value = item
  inviteAdminNome.value = ''
  inviteAdminEmail.value = ''
  inviteError.value = ''
  adminInviteOpen.value = true
}
async function submitAdminInvite() {
  if (!invitePartner.value || invitingAdmin.value) return
  if (!inviteAdminNome.value.trim() || !inviteAdminEmail.value.trim()) {
    inviteError.value = 'Informe o nome e o e-mail do administrador.'
    return
  }
  invitingAdmin.value = true
  inviteError.value = ''
  try {
    await convidarAdministradorParceiro(invitePartner.value.id, {
      nome: inviteAdminNome.value,
      email: inviteAdminEmail.value,
    })
    success.value = `Convite de administrador enviado para ${inviteAdminEmail.value.trim()}.`
    adminInviteOpen.value = false
    await load()
  } catch (cause) {
    inviteError.value = normalizeError(cause).message
  } finally {
    invitingAdmin.value = false
  }
}
function requestStatus(item: ParceiroPlataforma) { selected.value = item; dialogError.value = ''; confirmOpen.value = true }
async function confirmStatus() {
  if (!selected.value || savingId.value) return
  savingId.value = selected.value.id; dialogError.value = ''
  try {
    await alterarStatusParceiro(selected.value.id, !selected.value.ativo)
    success.value = `Parceiro ${selected.value.ativo ? 'suspenso' : 'reativado'}.`; confirmOpen.value = false; await load()
  } catch (cause) { dialogError.value = normalizeError(cause).message }
  finally { savingId.value = null }
}

void load()
onBeforeUnmount(() => controller?.abort())
</script>

<template>
  <div class="platform-context"><i class="pi pi-shield" aria-hidden="true" /><span>Administração da plataforma</span></div>
  <PageHeader title="Parceiros" description="Gerencie as organizações que utilizam o Officio e seus primeiros acessos.">
    <BaseButton label="Cadastrar parceiro" icon="pi pi-plus" @click="openCreate" />
  </PageHeader>
  <Message v-if="success" severity="success" closable class="page-message" @close="success = ''">{{ success }}</Message>
  <Message v-if="error" severity="error" :closable="false" class="page-message">{{ error }}</Message>

  <div class="access-summary" aria-label="Resumo da plataforma">
    <div><strong class="numeric">{{ parceiros.length }}</strong><span>Parceiros</span></div>
    <div><strong class="numeric">{{ activeCount }}</strong><span>Operações ativas</span></div>
    <div><strong class="numeric">{{ userCount }}</strong><span>Usuários cadastrados</span></div>
  </div>

  <BaseCard class="data-card">
    <div class="filters platform-filters"><BaseInput id="company-search" v-model="search" label="Buscar parceiro" type="search" placeholder="Razão social, nome fantasia ou CNPJ" autocomplete="off" /></div>
    <div class="list-heading"><h2>Parceiros da plataforma</h2><span>{{ filtered.length }} {{ filtered.length === 1 ? 'parceiro' : 'parceiros' }}</span></div>
    <div v-if="loading" class="list-state" aria-busy="true"><i class="pi pi-spinner loading-spinner" /><p>Carregando parceiros...</p></div>
    <div v-else-if="error" class="list-state"><BaseButton label="Tentar novamente" icon="pi pi-refresh" variant="secondary" @click="load" /></div>
    <DataTable v-else :value="filtered" data-key="id" class="work-table companies-table" :table-props="{ 'aria-label': 'Parceiros da plataforma' }">
      <Column field="razaoSocial" header="Parceiro"><template #body="{ data }"><div class="company-cell"><strong>{{ data.razaoSocial }}</strong><span>{{ data.nomeFantasia || formatCnpj(data.cnpj) }}</span></div></template></Column>
      <Column field="ativo" header="Situação"><template #body="{ data }"><StatusBadge :label="data.ativo ? 'Ativo' : 'Suspenso'" :severity="data.ativo ? 'success' : 'danger'" /></template></Column>
      <Column field="administradorEmail" header="Primeiro administrador"><template #body="{ data }"><div class="company-cell"><strong>{{ data.administradorEmail || 'Não informado' }}</strong><span>{{ data.quantidadeUsuarios }} {{ data.quantidadeUsuarios === 1 ? 'usuário' : 'usuários' }}</span></div></template></Column>
      <Column field="criadaEm" header="Cadastro"><template #body="{ data }"><span class="muted">{{ formatDate(data.criadaEm) }}</span></template></Column>
      <Column header="Ações"><template #body="{ data }"><div class="row-actions"><BaseButton label="Convidar administrador" icon="pi pi-user-plus" variant="text" :disabled="!!savingId || !data.ativo" @click="openAdminInvite(data)" /><BaseButton v-if="data.administradorEmail" label="Reenviar convite" icon="pi pi-send" variant="text" :loading="savingId === data.id" @click="resend(data)" /><BaseButton :label="data.ativo ? 'Suspender' : 'Reativar'" :icon="data.ativo ? 'pi pi-ban' : 'pi pi-check'" variant="text" :disabled="!!savingId" @click="requestStatus(data)" /></div></template></Column>
      <template #empty><div class="empty-state"><i class="pi pi-building" /><h3>Nenhum parceiro encontrado</h3><p>{{ search ? 'Revise os termos da busca.' : 'Cadastre o primeiro parceiro para iniciar o onboarding.' }}</p><BaseButton v-if="!search" label="Cadastrar parceiro" icon="pi pi-plus" @click="openCreate" /></div></template>
    </DataTable>
  </BaseCard>

  <Dialog v-model:visible="createOpen" modal header="Cadastrar parceiro" class="workflow-dialog company-create-dialog" :style="{ width: 'min(720px, 95vw)' }">
    <form id="company-form" class="dialog-form" @submit.prevent="submitCreate">
      <Message v-if="dialogError" severity="error" :closable="false">{{ dialogError }}</Message>
      <fieldset><legend>Dados do parceiro</legend><div class="form-grid two-columns">
        <BaseInput id="company-name" v-model="razaoSocial" label="Razão social" required :disabled="saving" />
        <BaseInput id="company-trade-name" v-model="nomeFantasia" label="Nome fantasia" :disabled="saving" />
        <BaseInput id="company-cnpj" v-model="cnpj" label="CNPJ" inputmode="numeric" placeholder="00.000.000/0000-00" required :disabled="saving" />
      </div></fieldset>
      <fieldset><legend>Primeiro administrador</legend><p>Esta pessoa receberá o convite para ativar a conta e gerenciar a equipe do parceiro.</p><div class="form-grid two-columns">
        <BaseInput id="admin-name" v-model="adminNome" label="Nome completo" autocomplete="name" required :disabled="saving" />
        <BaseInput id="admin-email" v-model="adminEmail" label="E-mail" type="email" autocomplete="email" required :disabled="saving" />
      </div></fieldset>
    </form>
    <template #footer><BaseButton label="Cancelar" variant="text" :disabled="saving" @click="createOpen = false" /><BaseButton label="Criar e convidar" icon="pi pi-send" type="submit" form="company-form" :loading="saving" /></template>
  </Dialog>

  <Dialog v-model:visible="adminInviteOpen" modal header="Convidar administrador" class="workflow-dialog" :style="{ width: 'min(560px, 94vw)' }">
    <form id="admin-invite-form" class="dialog-form" @submit.prevent="submitAdminInvite">
      <Message v-if="inviteError" severity="error" :closable="false">{{ inviteError }}</Message>
      <p v-if="invitePartner" class="confirmation-copy">O convite será para administrar {{ invitePartner.razaoSocial }}. A pessoa receberá um e-mail para ativar a conta e definir a senha.</p>
      <div class="form-grid">
        <BaseInput id="partner-admin-name" v-model="inviteAdminNome" label="Nome completo" autocomplete="name" required :disabled="invitingAdmin" />
        <BaseInput id="partner-admin-email" v-model="inviteAdminEmail" label="E-mail" type="email" autocomplete="email" required :disabled="invitingAdmin" />
      </div>
    </form>
    <template #footer><BaseButton label="Cancelar" variant="text" :disabled="invitingAdmin" @click="adminInviteOpen = false" /><BaseButton label="Enviar convite" icon="pi pi-send" type="submit" form="admin-invite-form" :loading="invitingAdmin" /></template>
  </Dialog>

  <Dialog v-model:visible="confirmOpen" modal :header="selected?.ativo ? 'Suspender parceiro' : 'Reativar parceiro'" class="workflow-dialog" :style="{ width: 'min(460px, 94vw)' }">
    <Message v-if="dialogError" severity="error" :closable="false">{{ dialogError }}</Message>
    <p v-if="selected" class="confirmation-copy">{{ selected.ativo ? `Os usuários de ${selected.razaoSocial} perderão o acesso até o parceiro ser reativado.` : `Os usuários ativos de ${selected.razaoSocial} poderão voltar a acessar o Officio.` }}</p>
    <template #footer><BaseButton label="Voltar" variant="text" :disabled="!!savingId" @click="confirmOpen = false" /><BaseButton :label="selected?.ativo ? 'Suspender parceiro' : 'Reativar parceiro'" :icon="selected?.ativo ? 'pi pi-ban' : 'pi pi-check'" :loading="!!savingId" @click="confirmStatus" /></template>
  </Dialog>
</template>
