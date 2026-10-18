<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import Dialog from 'primevue/dialog'
import Message from 'primevue/message'
import Textarea from 'primevue/textarea'
import BaseButton from '../../../components/base/BaseButton.vue'
import StatusBadge from '../../../components/shared/StatusBadge.vue'
import { normalizeError } from '../../../api/errors'
import { formatDate } from '../../../utils/format'
import { aprovarDocumentoAdmissao, enviarDocumentoAdmissao, listarDocumentosAdmissao, obterUrlDocumentoAdmissao, removerDocumentoAdmissao, validarDocumentoAdmissao } from '../services/admissoes.service'
import type { AdmissaoDocumento } from '../types/admissao'
import { agruparDocumentosAdmissao } from '../documentos'

const props = defineProps<{ admissaoId: string; editable: boolean; canValidate: boolean }>()
const emit = defineEmits<{ atualizado: [] }>()
const documentos = ref<AdmissaoDocumento[]>([])
const loading = ref(false)
const actionId = ref('')
const error = ref('')
const success = ref('')
const revisaoAberta = ref(false)
const documentoEmRevisao = ref<AdmissaoDocumento | null>(null)
const justificativaRevisao = ref('')
const erroRevisao = ref('')
const gruposDocumentos = computed(() => agruparDocumentosAdmissao(documentos.value))
const tipoIdentificacao = ref<Record<string, string>>({})
const documentosExibidos = computed(() => gruposDocumentos.value.flatMap((grupo) => grupo.documentos))
const quantidadeEnviada = computed(() => documentosExibidos.value
  .filter((documento) => documentoAtivo(documento).documentoId).length)
const possuiPendentesObrigatorios = computed(() => gruposDocumentos.value
  .flatMap((grupo) => grupo.documentos)
  .some((documento) => documento.obrigatorio && !documentoAtivo(documento).documentoId))

const statusConfig: Record<string, { label: string; severity: 'info' | 'success' | 'warn' | 'danger' | 'secondary' }> = {
  PENDENTE_ENVIO: { label: 'Pendente', severity: 'warn' },
  ENVIADO: { label: 'Enviado', severity: 'info' },
  PENDENTE_VALIDACAO: { label: 'Validando', severity: 'info' },
  APROVADO: { label: 'Aprovado', severity: 'success' },
  REPROVADO: { label: 'Reprovado', severity: 'danger' },
  REVISAO_MANUAL: { label: 'Revisão manual', severity: 'warn' },
}

function status(documento: AdmissaoDocumento) {
  if (!documento.obrigatorio && documento.statusValidacao === 'PENDENTE_ENVIO')
    return { label: 'Opcional', severity: 'secondary' as const }
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
  validade: 'Validade',
  numero_titulo: 'Número do título',
  numero_reservista: 'Número do certificado',
  escolaridade: 'Escolaridade',
  instituicao: 'Instituição',
  curso: 'Curso',
  cep: 'CEP',
  endereco: 'Endereço',
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
  try {
    documentos.value = await listarDocumentosAdmissao(props.admissaoId)
    for (const documento of gruposDocumentos.value.flatMap((grupo) => grupo.documentos)) {
      if (documento.alternativas && !tipoIdentificacao.value[documento.id])
        tipoIdentificacao.value[documento.id] = documento.alternativas.find((item) => item.documentoId)?.tipoCodigo ?? 'IDENTIDADE'
    }
  }
  catch (cause) { error.value = normalizeError(cause).message }
  finally { loading.value = false }
}

async function executar(itemId: string, acao: () => Promise<void>, mensagem: string): Promise<boolean> {
  actionId.value = itemId
  error.value = ''
  success.value = ''
  try {
    await acao()
    await carregar()
    success.value = mensagem
    emit('atualizado')
    return true
  } catch (cause) {
    error.value = normalizeError(cause).message
    return false
  }
  finally { actionId.value = '' }
}

function documentoAtivo(documento: AdmissaoDocumento): AdmissaoDocumento {
  if (!documento.alternativas) return documento
  const tipo = tipoIdentificacao.value[documento.id] ?? 'IDENTIDADE'
  return {
    ...(documento.alternativas.find((item) => item.tipoCodigo === tipo) ?? documento.alternativas[0]!),
    obrigatorio: true,
  }
}

async function enviar(documento: AdmissaoDocumento, event: Event) {
  const input = event.target as HTMLInputElement
  const arquivos = Array.from(input.files ?? [])
  input.value = ''
  if (!arquivos.length) return
  if (arquivos.length > 10 || arquivos.reduce((total, arquivo) => total + arquivo.size, 0) > 10 * 1024 * 1024) {
    error.value = 'Selecione no máximo 10 arquivos, somando até 10 MB.'
    return
  }
  const alvo = documentoAtivo(documento)
  await executar(alvo.id, () => enviarDocumentoAdmissao(props.admissaoId, alvo.id, arquivos), 'Documento enviado.')
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
  documentoEmRevisao.value = documento
  justificativaRevisao.value = ''
  erroRevisao.value = ''
  revisaoAberta.value = true
}

async function confirmarRevisao() {
  const documento = documentoEmRevisao.value
  const justificativa = justificativaRevisao.value.trim()
  if (!documento) return
  if (!justificativa) {
    erroRevisao.value = 'Informe por que o documento foi conferido e aprovado.'
    return
  }

  erroRevisao.value = ''
  const aprovado = await executar(documento.id, () => aprovarDocumentoAdmissao(
    props.admissaoId, documento.id, justificativa), 'Documento aprovado manualmente.')
  if (aprovado) {
    revisaoAberta.value = false
    documentoEmRevisao.value = null
    justificativaRevisao.value = ''
  }
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
      <h3>Documentos <span>{{ quantidadeEnviada }}/{{ documentosExibidos.length }}</span></h3>
      <span v-if="possuiPendentesObrigatorios" class="document-pending-count">Obrigatórios pendentes</span>
    </div>
    <Message v-if="success" severity="success" :closable="true" @close="success = ''">{{ success }}</Message>
    <Message v-if="error" severity="error" :closable="true" @close="error = ''">{{ error }}</Message>
    <div v-if="loading" class="document-loading" aria-busy="true"><i class="pi pi-spinner loading-spinner" /><span>Carregando documentos...</span></div>
    <p v-else-if="!documentos.length" class="muted">Nenhum documento configurado para esta admissão.</p>
    <div v-else class="document-groups">
      <section v-for="grupo in gruposDocumentos" :key="grupo.codigo" class="document-group">
        <h4><i :class="grupo.icone" aria-hidden="true" />{{ grupo.titulo }}<span>{{ grupo.documentos.length }}</span></h4>
        <div class="document-checklist">
          <article v-for="documento in grupo.documentos" :key="documento.id" class="document-row">
            <div class="document-icon" aria-hidden="true"><i class="pi pi-file" /></div>
            <div class="document-copy">
              <div class="document-title"><strong>{{ documento.rotulo }}</strong><span v-if="documento.obrigatorio">Obrigatório</span></div>
              <div v-if="documento.alternativas" class="document-type-choice" role="group" aria-label="Tipo de documento de identificação">
                <label v-for="opcao in documento.alternativas" :key="opcao.id"><input v-model="tipoIdentificacao[documento.id]" type="radio" :name="`identificacao-${documento.id}`" :value="opcao.tipoCodigo" :disabled="Boolean(actionId)">{{ opcao.tipoCodigo === 'CNH' ? 'CNH' : 'Identidade (RG/CIN)' }}</label>
              </div>
              <span v-if="documentoAtivo(documento).nomeArquivo" class="document-file">{{ documentoAtivo(documento).nomeArquivo }} · {{ tamanho(documentoAtivo(documento).tamanhoBytes) }} · {{ formatDate(documentoAtivo(documento).enviadoEm) }}</span>
              <span v-else class="document-file">Arquivo não enviado</span>
              <small v-if="documentoAtivo(documento).mensagemValidacao">{{ documentoAtivo(documento).mensagemValidacao }}</small>
              <small v-if="documentoAtivo(documento).tipoDocumentoDetectado" class="document-detected-type">Tipo identificado: {{ documentoAtivo(documento).tipoDocumentoDetectado }}</small>
              <small v-if="documentoAtivo(documento).tipoDocumentoIncompativel" class="document-type-error">O arquivo não corresponde a este documento. Substitua-o pelo tipo solicitado; a revisão manual não pode ignorar essa incompatibilidade.</small>
            </div>
            <StatusBadge :label="status(documentoAtivo(documento)).label" :severity="status(documentoAtivo(documento)).severity" />
            <div class="document-actions">
              <BaseButton v-if="documentoAtivo(documento).documentoId" icon="pi pi-external-link" variant="text" title="Visualizar documento" aria-label="Visualizar documento" :loading="actionId === documentoAtivo(documento).id" @click="visualizar(documentoAtivo(documento))" />
              <BaseButton v-if="editable && canValidate && documentoAtivo(documento).documentoId && documentoAtivo(documento).statusValidacao !== 'APROVADO'" label="Validar" icon="pi pi-sparkles" variant="secondary" :loading="actionId === documentoAtivo(documento).id" @click="validar(documentoAtivo(documento))" />
              <BaseButton v-if="editable && canValidate && !documentoAtivo(documento).tipoDocumentoIncompativel && ['REVISAO_MANUAL', 'REPROVADO'].includes(documentoAtivo(documento).statusValidacao)" label="Revisão manual" icon="pi pi-eye" :loading="actionId === documentoAtivo(documento).id" @click="aprovar(documentoAtivo(documento))" />
              <label v-if="editable" class="document-upload" :for="`documento-${documentoAtivo(documento).id}`"><i :class="documentoAtivo(documento).documentoId ? 'pi pi-refresh' : 'pi pi-upload'" />{{ documentoAtivo(documento).documentoId ? 'Substituir' : 'Enviar' }}</label>
              <input :id="`documento-${documentoAtivo(documento).id}`" class="visually-hidden" type="file" multiple accept=".pdf,.jpg,.jpeg,.png,application/pdf,image/jpeg,image/png" :disabled="actionId === documentoAtivo(documento).id" @change="enviar(documento, $event)" />
              <BaseButton v-if="editable && documentoAtivo(documento).documentoId" icon="pi pi-trash" variant="text" title="Remover documento" aria-label="Remover documento" :loading="actionId === documentoAtivo(documento).id" @click="remover(documentoAtivo(documento))" />
            </div>
            <details v-if="possuiDetalhesOcr(documentoAtivo(documento))" class="ocr-review">
              <summary><span><i class="pi pi-sparkles" />Detalhes da validação</span><strong>Confiança {{ percentual(documentoAtivo(documento).confiancaOcr) }}</strong></summary>
              <div v-if="documentoAtivo(documento).divergencias.length" class="ocr-divergence-list">
                <div class="ocr-comparison ocr-comparison-heading" aria-hidden="true"><span>Campo</span><span>Documento</span><span>Cadastro</span><span>Similaridade</span></div>
                <div v-for="divergencia in documentoAtivo(documento).divergencias" :key="divergencia.campo" class="ocr-comparison">
                  <strong>{{ rotuloCampo(divergencia.campo) }}</strong>
                  <span>{{ divergencia.valorDocumento || 'Não identificado' }}</span>
                  <span>{{ divergencia.valorCadastro || 'Não informado' }}</span>
                  <span>{{ percentual(divergencia.similaridade) }}</span>
                </div>
              </div>
              <dl class="ocr-extracted-fields">
                <div v-for="(valor, campo) in documentoAtivo(documento).camposExtraidos" :key="campo"><dt>{{ rotuloCampo(campo) }}</dt><dd>{{ valor }}</dd></div>
              </dl>
            </details>
            <details v-if="documentoAtivo(documento).observacaoRevisao" class="manual-review-audit">
              <summary><i class="pi pi-user-edit" /> Motivo da revisão manual</summary>
              <p>{{ documentoAtivo(documento).observacaoRevisao }}</p>
              <small v-if="documentoAtivo(documento).revisadoPorNome || documentoAtivo(documento).revisadoEm">
                <span v-if="documentoAtivo(documento).revisadoPorNome">Revisado por {{ documentoAtivo(documento).revisadoPorNome }}</span>
                <span v-if="documentoAtivo(documento).revisadoEm"> · {{ formatDate(documentoAtivo(documento).revisadoEm ?? null) }}</span>
              </small>
            </details>
          </article>
        </div>
      </section>
    </div>
  </section>

  <Dialog v-model:visible="revisaoAberta" modal header="Revisar documento manualmente" class="workflow-dialog manual-review-dialog" :style="{ width: 'min(700px, 94vw)' }">
    <div v-if="documentoEmRevisao" class="manual-review-content">
      <div class="manual-review-heading">
        <div><strong>{{ documentoEmRevisao.rotulo }}</strong><span>{{ documentoEmRevisao.nomeArquivo || 'Arquivo enviado' }}</span></div>
        <StatusBadge :label="status(documentoEmRevisao).label" :severity="status(documentoEmRevisao).severity" />
      </div>
      <Message severity="warn" :closable="false">
        Compare o arquivo com os dados da admissão. A aprovação manual substitui a decisão automática e fica registrada com seu usuário e justificativa.
      </Message>
      <div class="manual-review-metadata">
        <span v-if="documentoEmRevisao.tipoDocumentoDetectado">Tipo identificado <strong>{{ documentoEmRevisao.tipoDocumentoDetectado }}</strong></span>
        <span>Confiança OCR <strong>{{ percentual(documentoEmRevisao.confiancaOcr) }}</strong></span>
        <span v-if="documentoEmRevisao.mensagemValidacao">Resultado <strong>{{ documentoEmRevisao.mensagemValidacao }}</strong></span>
      </div>
      <div v-if="documentoEmRevisao.divergencias.length" class="ocr-divergence-list">
        <div class="ocr-comparison ocr-comparison-heading" aria-hidden="true"><span>Campo</span><span>Documento</span><span>Cadastro</span><span>Similaridade</span></div>
        <div v-for="divergencia in documentoEmRevisao.divergencias" :key="divergencia.campo" class="ocr-comparison">
          <strong>{{ rotuloCampo(divergencia.campo) }}</strong>
          <span>{{ divergencia.valorDocumento || 'Não identificado' }}</span>
          <span>{{ divergencia.valorCadastro || 'Não informado' }}</span>
          <span>{{ percentual(divergencia.similaridade) }}</span>
        </div>
      </div>
      <dl v-if="Object.keys(documentoEmRevisao.camposExtraidos).length" class="ocr-extracted-fields">
        <div v-for="(valor, campo) in documentoEmRevisao.camposExtraidos" :key="campo"><dt>{{ rotuloCampo(campo) }}</dt><dd>{{ valor }}</dd></div>
      </dl>
      <div class="manual-review-attachment">
        <BaseButton label="Abrir arquivo" icon="pi pi-external-link" variant="secondary" :loading="actionId === documentoEmRevisao.id" @click="visualizar(documentoEmRevisao)" />
      </div>
      <label class="manual-review-justification" for="manual-review-justification">Justificativa da aprovação <span>Obrigatória</span></label>
      <Textarea id="manual-review-justification" v-model="justificativaRevisao" rows="3" auto-resize placeholder="Descreva o que foi conferido no documento." :disabled="Boolean(actionId)" />
      <Message v-if="erroRevisao" severity="error" :closable="false">{{ erroRevisao }}</Message>
    </div>
    <template #footer>
      <BaseButton label="Cancelar" variant="text" :disabled="Boolean(actionId)" @click="revisaoAberta = false" />
      <BaseButton label="Aprovar manualmente" icon="pi pi-check" :loading="actionId === documentoEmRevisao?.id" :disabled="!justificativaRevisao.trim()" @click="confirmarRevisao" />
    </template>
  </Dialog>
</template>
