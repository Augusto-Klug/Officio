<script setup lang="ts">
import { onBeforeUnmount, ref } from 'vue'
import axios from 'axios'
import Column from 'primevue/column'
import DataTable from 'primevue/datatable'
import Dialog from 'primevue/dialog'
import InputText from 'primevue/inputtext'
import Message from 'primevue/message'
import Textarea from 'primevue/textarea'
import BaseButton from '../../../components/base/BaseButton.vue'
import BaseCard from '../../../components/base/BaseCard.vue'
import PageHeader from '../../../components/layout/PageHeader.vue'
import StatusBadge from '../../../components/shared/StatusBadge.vue'
import { normalizeError } from '../../../api/errors'
import { formatCurrency, formatDate } from '../../../utils/format'
import { enviarContratoParaAssinatura, gerarPdfContrato, listarContratos, obterPdfAssinado } from '../services/contratos.service'
import type { Contrato } from '../types/contrato'

const contratos = ref<Contrato[]>([])
const loading = ref(true)
const error = ref('')
const success = ref('')
const actionId = ref('')
const signOpen = ref(false)
const selected = ref<Contrato | null>(null)
const formError = ref('')
const resultLinks = ref<Array<{ label: string; url: string }>>([])
const signer = ref({ nome: '', email: '', cpf: '', tipo: 'cliente', mensagemPersonalizada: '' })
let controller: AbortController | undefined

async function load() {
  controller?.abort()
  controller = new AbortController()
  loading.value = true
  error.value = ''
  try { contratos.value = await listarContratos(controller.signal) }
  catch (cause) { if (!axios.isCancel(cause)) error.value = normalizeError(cause).message }
  finally { loading.value = false }
}

async function gerarPdf(item: Contrato) {
  actionId.value = item.id
  error.value = ''
  resultLinks.value = []
  try {
    const result = await gerarPdfContrato(item.id)
    success.value = 'PDF do contrato gerado com sucesso.'
    resultLinks.value = [{ label: 'Abrir PDF gerado', url: result.pdfUrl }]
    await load()
  } catch (cause) { error.value = normalizeError(cause).message }
  finally { actionId.value = '' }
}

function abrirAssinatura(item: Contrato) {
  selected.value = item
  signer.value = { nome: item.razaoSocial, email: '', cpf: '', tipo: 'cliente', mensagemPersonalizada: '' }
  formError.value = ''
  resultLinks.value = []
  signOpen.value = true
}

async function enviarAssinatura() {
  if (!selected.value) return
  if (!signer.value.nome.trim() || !/^\S+@\S+\.\S+$/.test(signer.value.email)) {
    formError.value = 'Informe o nome e um e-mail válido para o signatário.'
    return
  }
  actionId.value = selected.value.id
  formError.value = ''
  try {
    const result = await enviarContratoParaAssinatura(selected.value.id, {
      signatarios: [{ nome: signer.value.nome.trim(), email: signer.value.email.trim(), cpf: signer.value.cpf.trim() || undefined, tipo: signer.value.tipo }],
      mensagemPersonalizada: signer.value.mensagemPersonalizada.trim() || undefined,
    })
    signOpen.value = false
    success.value = result.metodo === 'digital_api' ? 'Contrato enviado para assinatura.' : (result.mensagem || 'PDF disponível para assinatura manual.')
    resultLinks.value = [
      ...(result.signatarios || []).map((item) => ({ label: `Assinar como ${item.email}`, url: item.link })),
      ...(result.pdfUrl ? [{ label: 'Abrir PDF para assinatura manual', url: result.pdfUrl }] : []),
    ]
    await load()
  } catch (cause) { formError.value = normalizeError(cause).message }
  finally { actionId.value = '' }
}

async function abrirPdfAssinado(item: Contrato) {
  actionId.value = item.id
  resultLinks.value = []
  try { resultLinks.value = [{ label: 'Baixar PDF assinado', url: await obterPdfAssinado(item.id) }]; success.value = 'PDF assinado disponível.' }
  catch (cause) { error.value = normalizeError(cause).message }
  finally { actionId.value = '' }
}

function severity(status: string) {
  if (status === 'Ativo') return 'success'
  if (status === 'AguardandoAssinatura') return 'warn'
  if (status === 'Cancelado' || status === 'Encerrado') return 'danger'
  return 'secondary'
}

void load()
onBeforeUnmount(() => controller?.abort())
</script>

<template>
  <PageHeader title="Contratos" description="Gere documentos, acompanhe assinaturas e acesse os arquivos concluídos." />
  <Message v-if="success" severity="success" :closable="true" class="page-message" @close="success = ''">{{ success }}</Message>
  <div v-if="resultLinks.length" class="result-links" aria-label="Links da operação">
    <a v-for="item in resultLinks" :key="item.url" :href="item.url" target="_blank" rel="noopener noreferrer"><i class="pi pi-external-link" aria-hidden="true" />{{ item.label }}</a>
  </div>
  <Message v-if="error" severity="error" :closable="false" class="page-message">{{ error }}</Message>
  <BaseCard class="data-card">
    <div class="list-heading"><h2>Contratos</h2><span>{{ loading ? 'Carregando…' : `${contratos.length} ${contratos.length === 1 ? 'contrato' : 'contratos'}` }}</span></div>
    <div v-if="loading" class="list-state" aria-busy="true"><i class="pi pi-spinner loading-spinner" /><p>Carregando contratos…</p></div>
    <div v-else-if="error" class="list-state"><BaseButton label="Tentar novamente" icon="pi pi-refresh" variant="secondary" @click="load" /></div>
    <DataTable v-else :value="contratos" data-key="id" class="work-table contracts-table" :table-props="{ 'aria-label': 'Lista de contratos' }">
      <Column field="numero" header="Contrato"><template #body="{ data }"><div class="company-cell"><strong>{{ data.numero }}</strong><span>{{ data.razaoSocial }}</span></div></template></Column>
      <Column field="status" header="Status"><template #body="{ data }"><StatusBadge :label="data.status" :severity="severity(data.status)" /></template></Column>
      <Column field="valorTotal" header="Valor"><template #body="{ data }"><span class="numeric">{{ formatCurrency(data.valorTotal) }}</span></template></Column>
      <Column field="dataInicio" header="Início"><template #body="{ data }"><span class="numeric">{{ formatDate(data.dataInicio) }}</span></template></Column>
      <Column header="Ações"><template #body="{ data }"><div class="row-actions"><BaseButton v-if="!data.pdfGerado" label="Gerar PDF" icon="pi pi-file-pdf" variant="secondary" :loading="actionId === data.id" :disabled="!!actionId && actionId !== data.id" @click="gerarPdf(data)" /><BaseButton v-else-if="data.status === 'Rascunho'" label="Enviar" icon="pi pi-send" variant="secondary" :loading="actionId === data.id" :disabled="!!actionId && actionId !== data.id" @click="abrirAssinatura(data)" /><BaseButton v-if="data.pdfAssinadoDisponivel" label="Assinado" icon="pi pi-download" variant="text" :loading="actionId === data.id" :disabled="!!actionId && actionId !== data.id" @click="abrirPdfAssinado(data)" /></div></template></Column>
      <template #empty><div class="empty-state"><i class="pi pi-file" /><h3>Nenhum contrato criado</h3><p>Contratos gerados a partir de orçamentos aceitos aparecerão aqui.</p></div></template>
    </DataTable>
  </BaseCard>

  <Dialog v-model:visible="signOpen" modal header="Enviar para assinatura" class="workflow-dialog" :style="{ width: 'min(580px, 94vw)' }">
    <form id="signature-form" class="dialog-form" @submit.prevent="enviarAssinatura">
      <Message v-if="formError" severity="error" :closable="false">{{ formError }}</Message>
      <div class="field"><label for="signer-name">Nome do signatário</label><InputText id="signer-name" v-model="signer.nome" autocomplete="name" /></div>
      <div class="form-grid two-columns"><div class="field"><label for="signer-email">E-mail</label><InputText id="signer-email" v-model="signer.email" type="email" autocomplete="email" /></div><div class="field"><label for="signer-cpf">CPF (opcional)</label><InputText id="signer-cpf" v-model="signer.cpf" inputmode="numeric" /></div></div>
      <div class="field"><label for="signature-message">Mensagem (opcional)</label><Textarea id="signature-message" v-model="signer.mensagemPersonalizada" rows="3" auto-resize /></div>
    </form>
    <template #footer><BaseButton label="Cancelar" variant="text" @click="signOpen = false" /><BaseButton label="Enviar contrato" icon="pi pi-send" type="submit" form="signature-form" :loading="!!actionId" /></template>
  </Dialog>
</template>
