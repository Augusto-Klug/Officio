<script setup lang="ts">
import { computed, onBeforeUnmount, ref } from 'vue'
import axios from 'axios'
import Column from 'primevue/column'
import DataTable from 'primevue/datatable'
import Dialog from 'primevue/dialog'
import Message from 'primevue/message'
import Select from 'primevue/select'
import BaseButton from '../../../components/base/BaseButton.vue'
import BaseCard from '../../../components/base/BaseCard.vue'
import BaseInput from '../../../components/base/BaseInput.vue'
import PageHeader from '../../../components/layout/PageHeader.vue'
import StatusBadge from '../../../components/shared/StatusBadge.vue'
import { normalizeError } from '../../../api/errors'
import { session } from '../../../stores/session'
import { formatDate } from '../../../utils/format'
import { alterarPerfil, alterarStatus, cancelarConvite, convidarUsuario, listarUsuarios, reenviarConvite } from '../services/usuarios.service'
import type { PerfilUsuario, UsuarioAcesso } from '../types/usuario'

const usuarios = ref<UsuarioAcesso[]>([])
const loading = ref(true)
const error = ref('')
const success = ref('')
const inviteOpen = ref(false)
const confirmOpen = ref(false)
const selected = ref<UsuarioAcesso | null>(null)
const action = ref<'status' | 'cancelar'>('status')
const savingId = ref<string | null>(null)
const inviteSaving = ref(false)
const dialogError = ref('')
const nome = ref('')
const email = ref('')
const perfil = ref<PerfilUsuario>('Comercial')
let controller: AbortController | undefined

const perfis = ['Admin', 'Comercial', 'Operacional', 'Financeiro', 'Consulta'].map((value) => ({ label: value, value }))
const totalAtivos = computed(() => usuarios.value.filter((item) => item.situacao === 'ATIVO').length)
const totalPendentes = computed(() => usuarios.value.filter((item) => item.situacao.startsWith('CONVITE_')).length)

function statusLabel(item: UsuarioAcesso) {
  return { ATIVO: 'Ativo', INATIVO: 'Inativo', CONVITE_PENDENTE: 'Convite pendente', CONVITE_EXPIRADO: 'Convite expirado' }[item.situacao]
}
function statusSeverity(item: UsuarioAcesso): 'success' | 'secondary' | 'warn' {
  if (item.situacao === 'ATIVO') return 'success'
  if (item.situacao.startsWith('CONVITE_')) return 'warn'
  return 'secondary'
}
function isCurrent(item: UsuarioAcesso) { return item.id === session.value?.usuario.id }

async function load() {
  controller?.abort()
  controller = new AbortController()
  loading.value = true
  error.value = ''
  try { usuarios.value = await listarUsuarios(controller.signal) }
  catch (cause) { if (!axios.isCancel(cause)) error.value = normalizeError(cause).message }
  finally { loading.value = false }
}

function openInvite() {
  nome.value = ''; email.value = ''; perfil.value = 'Comercial'; dialogError.value = ''; inviteOpen.value = true
}
async function submitInvite() {
  if (inviteSaving.value) return
  if (!nome.value.trim() || !email.value.trim()) { dialogError.value = 'Informe nome e e-mail.'; return }
  inviteSaving.value = true
  dialogError.value = ''
  try {
    await convidarUsuario({ nome: nome.value, email: email.value, perfil: perfil.value })
    inviteOpen.value = false
    success.value = `Convite enviado para ${email.value.trim()}.`
    await load()
  } catch (cause) { dialogError.value = normalizeError(cause).message }
  finally { inviteSaving.value = false }
}

async function changeRole(item: UsuarioAcesso, value: PerfilUsuario) {
  if (savingId.value || value === item.perfil) return
  savingId.value = item.id
  error.value = ''
  try { await alterarPerfil(item.id, value); item.perfil = value; success.value = `Perfil de ${item.nome} atualizado.` }
  catch (cause) { error.value = normalizeError(cause).message }
  finally { savingId.value = null }
}

async function resend(item: UsuarioAcesso) {
  if (savingId.value) return
  savingId.value = item.id
  try { await reenviarConvite(item.id); success.value = `Convite reenviado para ${item.email}.` }
  catch (cause) { error.value = normalizeError(cause).message }
  finally { savingId.value = null }
}

function requestConfirmation(item: UsuarioAcesso, nextAction: 'status' | 'cancelar') {
  selected.value = item; action.value = nextAction; dialogError.value = ''; confirmOpen.value = true
}
async function confirmAction() {
  if (!selected.value || savingId.value) return
  savingId.value = selected.value.id
  dialogError.value = ''
  try {
    if (action.value === 'cancelar') {
      await cancelarConvite(selected.value.id)
      success.value = `Convite de ${selected.value.email} cancelado.`
    } else {
      await alterarStatus(selected.value.id, !selected.value.ativo)
      success.value = `Acesso de ${selected.value.nome} ${selected.value.ativo ? 'desativado' : 'reativado'}.`
    }
    confirmOpen.value = false
    await load()
  } catch (cause) { dialogError.value = normalizeError(cause).message }
  finally { savingId.value = null }
}

void load()
onBeforeUnmount(() => controller?.abort())
</script>

<template>
  <PageHeader title="Usuários e acessos" description="Convide pessoas e controle os acessos da sua empresa.">
    <BaseButton label="Convidar usuário" icon="pi pi-user-plus" @click="openInvite" />
  </PageHeader>
  <Message v-if="success" severity="success" closable class="page-message" @close="success = ''">{{ success }}</Message>
  <Message v-if="error" severity="error" :closable="false" class="page-message">{{ error }}</Message>

  <div class="access-summary" aria-label="Resumo de acessos">
    <div><strong class="numeric">{{ usuarios.length }}</strong><span>Usuários</span></div>
    <div><strong class="numeric">{{ totalAtivos }}</strong><span>Ativos</span></div>
    <div><strong class="numeric">{{ totalPendentes }}</strong><span>Convites pendentes</span></div>
  </div>

  <BaseCard class="data-card">
    <div class="list-heading"><div><h2>Equipe da empresa</h2><span>Perfis determinam quais áreas cada pessoa pode acessar.</span></div></div>
    <div v-if="loading" class="list-state" aria-busy="true"><i class="pi pi-spinner loading-spinner" /><p>Carregando usuários...</p></div>
    <div v-else-if="error" class="list-state"><BaseButton label="Tentar novamente" icon="pi pi-refresh" variant="secondary" @click="load" /></div>
    <DataTable v-else :value="usuarios" data-key="id" class="work-table access-table" :table-props="{ 'aria-label': 'Usuários da empresa' }">
      <Column field="nome" header="Usuário"><template #body="{ data }"><div class="company-cell"><strong>{{ data.nome }} <small v-if="isCurrent(data)">(você)</small></strong><span>{{ data.email }}</span></div></template></Column>
      <Column field="perfil" header="Perfil"><template #body="{ data }"><Select :model-value="data.perfil" :options="perfis" option-label="label" option-value="value" :disabled="savingId === data.id || isCurrent(data)" :aria-label="`Perfil de ${data.nome}`" @update:model-value="changeRole(data, $event)" /></template></Column>
      <Column field="situacao" header="Situação"><template #body="{ data }"><StatusBadge :label="statusLabel(data)" :severity="statusSeverity(data)" /></template></Column>
      <Column header="Última atividade"><template #body="{ data }"><span class="muted">{{ data.ultimoAcessoEm ? formatDate(data.ultimoAcessoEm) : data.convidadoEm ? `Convidado em ${formatDate(data.convidadoEm)}` : 'Sem acesso registrado' }}</span></template></Column>
      <Column header="Ações"><template #body="{ data }"><div class="row-actions">
        <template v-if="data.situacao.startsWith('CONVITE_')"><BaseButton label="Reenviar" icon="pi pi-send" variant="text" :loading="savingId === data.id" @click="resend(data)" /><BaseButton label="Cancelar" icon="pi pi-times" variant="text" :disabled="!!savingId" @click="requestConfirmation(data, 'cancelar')" /></template>
        <BaseButton v-else :label="data.ativo ? 'Desativar' : 'Reativar'" :icon="data.ativo ? 'pi pi-user-minus' : 'pi pi-user-plus'" variant="text" :disabled="isCurrent(data) || !!savingId" @click="requestConfirmation(data, 'status')" />
      </div></template></Column>
      <template #empty><div class="empty-state"><i class="pi pi-users" /><h3>Sua equipe ainda não foi convidada</h3><p>Envie o primeiro convite para começar a distribuir os acessos.</p><BaseButton label="Convidar usuário" icon="pi pi-user-plus" @click="openInvite" /></div></template>
    </DataTable>
  </BaseCard>

  <Dialog v-model:visible="inviteOpen" modal header="Convidar usuário" class="workflow-dialog" :style="{ width: 'min(520px, 94vw)' }">
    <form id="invite-form" class="dialog-form" @submit.prevent="submitInvite">
      <Message v-if="dialogError" severity="error" :closable="false">{{ dialogError }}</Message>
      <BaseInput id="invite-name" v-model="nome" label="Nome completo" autocomplete="name" required :disabled="inviteSaving" />
      <BaseInput id="invite-email" v-model="email" label="E-mail" type="email" autocomplete="email" required :disabled="inviteSaving" />
      <div class="field"><label for="invite-role">Perfil de acesso</label><Select input-id="invite-role" v-model="perfil" :options="perfis" option-label="label" option-value="value" :disabled="inviteSaving" /></div>
      <p class="form-note"><i class="pi pi-info-circle" aria-hidden="true" />A pessoa receberá um link individual para criar a senha.</p>
    </form>
    <template #footer><BaseButton label="Cancelar" variant="text" :disabled="inviteSaving" @click="inviteOpen = false" /><BaseButton label="Enviar convite" icon="pi pi-send" type="submit" form="invite-form" :loading="inviteSaving" /></template>
  </Dialog>

  <Dialog v-model:visible="confirmOpen" modal :header="action === 'cancelar' ? 'Cancelar convite' : selected?.ativo ? 'Desativar acesso' : 'Reativar acesso'" class="workflow-dialog" :style="{ width: 'min(460px, 94vw)' }">
    <Message v-if="dialogError" severity="error" :closable="false">{{ dialogError }}</Message>
    <p v-if="selected" class="confirmation-copy">{{ action === 'cancelar' ? `O convite enviado para ${selected.email} deixará de ser válido.` : selected.ativo ? `${selected.nome} perderá o acesso ao Officio até ser reativado.` : `${selected.nome} poderá voltar a acessar o Officio.` }}</p>
    <template #footer><BaseButton label="Voltar" variant="text" :disabled="!!savingId" @click="confirmOpen = false" /><BaseButton :label="action === 'cancelar' ? 'Cancelar convite' : selected?.ativo ? 'Desativar' : 'Reativar'" :icon="action === 'cancelar' ? 'pi pi-times' : 'pi pi-check'" :loading="!!savingId" @click="confirmAction" /></template>
  </Dialog>
</template>
