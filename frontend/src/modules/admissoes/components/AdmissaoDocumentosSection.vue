<script setup lang="ts">
import { ref, watch } from 'vue'
import Message from 'primevue/message'
import BaseButton from '../../../components/base/BaseButton.vue'
import StatusBadge from '../../../components/shared/StatusBadge.vue'
import { normalizeError } from '../../../api/errors'
import { formatDate } from '../../../utils/format'
import { aprovarDocumentoAdmissao, enviarDocumentoAdmissao, listarDocumentosAdmissao, obterUrlDocumentoAdmissao, removerDocumentoAdmissao, validarDocumentoAdmissao } from '../services/admissoes.service'
import type { AdmissaoDocumento } from '../types/admissao'

const props = defineProps<{ admissaoId: string; editable: boolean }>()
const emit = defineEmits<{ atualizado: [] }>()
const documentos = ref<AdmissaoDocumento[]>([])
const loading = ref(false)
const actionId = ref('')
const error = ref('')
const success = ref('')

const statusConfig: Record<string, { label: string; severity: 'info' | 'success' | 'warn' | 'danger' | 'secondary' }> = {
  PENDENTE_ENVIO: { label: 'Pendente', severity: 'warn' },
  ENVIADO: { label: 'Enviado', severity: 'info' },
  PENDENTE_VALIDACAO: { label: 'Validando', severity: 'info' },
  APROVADO: { label: 'Aprovado', severity: 'success' },
  REPROVADO: { label: 'Reprovado', severity: 'danger' },
  REVISAO_MANUAL: { label: 'Revisão manual', severity: 'warn' },
}

function status(documento: AdmissaoDocumento) {
  return statusConfig[documento.statusValidacao] ?? { label: documento.statusValidacao, severity: 'secondary' as const }
}

function tamanho(bytes: number | null): string {
  if (bytes == null) return ''
  if (bytes < 1024 * 1024) return `${Math.max(1, Math.round(bytes / 1024))} KB`
  return `${(bytes / 1024 / 1024).toFixed(1)} MB`
}

const rotulosCampos: Record<string, string> = {
  nome: 'Nome',
  cpf: 'CPF',
  rg: 'RG',
  data_nascimento: 'Data de nascimento',
  razao_social: 'Razão social',
  nome_fantasia: 'Nome fantasia',
  cnpj: 'CNPJ',
  email: 'E-mail',
}

function rotuloCampo(campo: string): string {
  return rotulosCampos[campo] ?? campo.replaceAll('_', ' ')
}

function percentual(valor: number | null): string {
  return valor == null ? 'Não informada' : `${Math.round(valor * 100)}%`
}

function possuiDetalhesOcr(documento: AdmissaoDocumento): boolean {
  return Object.keys(documento.camposExtraidos).length > 0 || documento.divergencias.length > 0
}

async function carregar() {
  loading.value = true
  error.value = ''
  try { documentos.value = await listarDocumentosAdmissao(props.admissaoId) }
  catch (cause) { error.value = normalizeError(cause).message }
  finally { loading.value = false }
}

async function executar(itemId: string, acao: () => Promise<void>, mensagem: string) {
  actionId.value = itemId
  error.value = ''
  success.value = ''
  try {
    await acao()
    await carregar()
    success.value = mensagem
    emit('atualizado')
  } catch (cause) { error.value = normalizeError(cause).message }
  finally { actionId.value = '' }
}

async function enviar(documento: AdmissaoDocumento, event: Event) {
  const input = event.target as HTMLInputElement
  const arquivo = input.files?.[0]
  input.value = ''
  if (!arquivo) return
  if (arquivo.size > 10 * 1024 * 1024) {
    error.value = 'O arquivo excede o limite de 10 MB.'
    return
  }
  await executar(documento.id, () => enviarDocumentoAdmissao(props.admissaoId, documento.id, arquivo), 'Documento enviado.')
}

async function visualizar(documento: AdmissaoDocumento) {
  actionId.value = documento.id
  error.value = ''
  try {
    const resultado = await obterUrlDocumentoAdmissao(props.admissaoId, documento.id)
    window.open(resultado.url, '_blank', 'noopener,noreferrer')
  } catch (cause) { error.value = normalizeError(cause).message }
  finally { actionId.value = '' }
}

async function validar(documento: AdmissaoDocumento) {
  await executar(documento.id, () => validarDocumentoAdmissao(props.admissaoId, documento.id).then(() => undefined), 'Validação concluída.')
}

async function aprovar(documento: AdmissaoDocumento) {
  await executar(documento.id, () => aprovarDocumentoAdmissao(
    props.admissaoId, documento.id, 'Documento conferido no painel da admissão.'), 'Documento aprovado manualmente.')
}

async function remover(documento: AdmissaoDocumento) {
  if (!window.confirm(`Remover o arquivo ${documento.nomeArquivo ?? ''}?`)) return
  await executar(documento.id, () => removerDocumentoAdmissao(props.admissaoId, documento.id), 'Documento removido.')
}

watch(() => props.admissaoId, () => void carregar(), { immediate: true })
</script>

<template>
  <section class="detail-section admission-documents">
    <div class="document-section-heading">
      <h3>Documentos <span>{{ documentos.filter((item) => item.documentoId).length }}/{{ documentos.length }}</span></h3>
      <span v-if="documentos.some((item) => item.obrigatorio && !item.documentoId)" class="document-pending-count">Obrigatórios pendentes</span>
    </div>
    <Message v-if="success" severity="success" :closable="true" @close="success = ''">{{ success }}</Message>
    <Message v-if="error" severity="error" :closable="true" @close="error = ''">{{ error }}</Message>
    <div v-if="loading" class="document-loading" aria-busy="true"><i class="pi pi-spinner loading-spinner" /><span>Carregando documentos...</span></div>
    <p v-else-if="!documentos.length" class="muted">Nenhum documento configurado para esta admissão.</p>
    <div v-else class="document-checklist">
      <article v-for="documento in documentos" :key="documento.id" class="document-row">
        <div class="document-icon" aria-hidden="true"><i class="pi pi-file" /></div>
        <div class="document-copy">
          <div class="document-title"><strong>{{ documento.rotulo }}</strong><span v-if="documento.obrigatorio">Obrigatório</span></div>
          <span v-if="documento.nomeArquivo" class="document-file">{{ documento.nomeArquivo }} · {{ tamanho(documento.tamanhoBytes) }} · {{ formatDate(documento.enviadoEm) }}</span>
          <span v-else class="document-file">Arquivo não enviado</span>
          <small v-if="documento.mensagemValidacao">{{ documento.mensagemValidacao }}</small>
        </div>
        <StatusBadge :label="status(documento).label" :severity="status(documento).severity" />
        <div class="document-actions">
          <BaseButton v-if="documento.documentoId" icon="pi pi-external-link" variant="text" title="Visualizar documento" aria-label="Visualizar documento" :loading="actionId === documento.id" @click="visualizar(documento)" />
          <BaseButton v-if="editable && documento.documentoId && documento.statusValidacao !== 'APROVADO'" label="Validar" icon="pi pi-sparkles" variant="secondary" :loading="actionId === documento.id" @click="validar(documento)" />
          <BaseButton v-if="editable && ['REVISAO_MANUAL', 'REPROVADO'].includes(documento.statusValidacao)" label="Aprovar" icon="pi pi-check" :loading="actionId === documento.id" @click="aprovar(documento)" />
          <label v-if="editable" class="document-upload" :for="`documento-${documento.id}`"><i :class="documento.documentoId ? 'pi pi-refresh' : 'pi pi-upload'" />{{ documento.documentoId ? 'Substituir' : 'Enviar' }}</label>
          <input :id="`documento-${documento.id}`" class="visually-hidden" type="file" accept=".pdf,.jpg,.jpeg,.png,application/pdf,image/jpeg,image/png" :disabled="actionId === documento.id" @change="enviar(documento, $event)" />
          <BaseButton v-if="editable && documento.documentoId" icon="pi pi-trash" variant="text" title="Remover documento" aria-label="Remover documento" :loading="actionId === documento.id" @click="remover(documento)" />
        </div>
        <details v-if="possuiDetalhesOcr(documento)" class="ocr-review">
          <summary><span><i class="pi pi-sparkles" />Detalhes da validação</span><strong>Confiança {{ percentual(documento.confiancaOcr) }}</strong></summary>
          <div v-if="documento.divergencias.length" class="ocr-divergence-list">
            <div class="ocr-comparison ocr-comparison-heading" aria-hidden="true"><span>Campo</span><span>Documento</span><span>Cadastro</span><span>Similaridade</span></div>
            <div v-for="divergencia in documento.divergencias" :key="divergencia.campo" class="ocr-comparison">
              <strong>{{ rotuloCampo(divergencia.campo) }}</strong>
              <span>{{ divergencia.valorDocumento || 'Não identificado' }}</span>
              <span>{{ divergencia.valorCadastro || 'Não informado' }}</span>
              <span>{{ percentual(divergencia.similaridade) }}</span>
            </div>
          </div>
          <dl class="ocr-extracted-fields">
            <div v-for="(valor, campo) in documento.camposExtraidos" :key="campo"><dt>{{ rotuloCampo(campo) }}</dt><dd>{{ valor }}</dd></div>
          </dl>
        </details>
      </article>
    </div>
  </section>
</template>
