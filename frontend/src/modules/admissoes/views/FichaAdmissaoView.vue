<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import { useRoute } from 'vue-router'
import Checkbox from 'primevue/checkbox'
import InputText from 'primevue/inputtext'
import Message from 'primevue/message'
import Select from 'primevue/select'
import BaseButton from '../../../components/base/BaseButton.vue'
import StatusBadge from '../../../components/shared/StatusBadge.vue'
import { normalizeError } from '../../../api/errors'
import { formatDate } from '../../../utils/format'
import { atualizarFichaAdmissao, concluirFichaAdmissao, enviarDocumentoFichaAdmissao, obterFichaAdmissao, obterUrlDocumentoFichaAdmissao } from '../services/admissoes.service'
import type { AtualizarFichaAdmissaoPayload, FichaAdmissaoPublica } from '../types/admissao'
import { agruparDocumentosAdmissao } from '../documentos'

const route = useRoute()
const token = String(route.params.token || '')
const ficha = ref<FichaAdmissaoPublica | null>(null)
const form = ref<AtualizarFichaAdmissaoPayload>({ nomeFuncionario: '', dependentes: [] })
const loading = ref(true)
const saving = ref(false)
const actionId = ref('')
const error = ref('')
const success = ref('')
const finalizado = computed(() => ficha.value?.preenchimentoConcluido === true)
const gruposDocumentos = computed(() => agruparDocumentosAdmissao(ficha.value?.documentos ?? []))
const tipoIdentificacao = ref<Record<string, string>>({})
const sexos = ['Feminino', 'Masculino', 'Outro', 'Prefiro não informar']
const estadosCivis = ['Solteiro(a)', 'Casado(a)', 'União estável', 'Divorciado(a)', 'Viúvo(a)']
const tiposConta = ['Corrente', 'Poupança', 'Salário']
const tamanhoMaximoArquivo = 10 * 1024 * 1024

function preencher(dados: FichaAdmissaoPublica) {
  form.value = {
    nomeFuncionario: dados.nomeFuncionario, cpf: dados.cpf ?? '', rg: dados.rg ?? '',
    dataNascimento: dados.dataNascimento ?? '', sexo: dados.sexo ?? '', estadoCivil: dados.estadoCivil ?? '',
    escolaridade: dados.escolaridade ?? '', telefone: dados.telefone ?? '', email: dados.email ?? '',
    pis: dados.pis ?? '', banco: dados.banco ?? '', agencia: dados.agencia ?? '', conta: dados.conta ?? '',
    tipoConta: dados.tipoConta ?? '', utilizaValeTransporte: dados.utilizaValeTransporte ?? false,
    dependentes: dados.dependentes.map((item) => ({
      nome: item.nome,
      cpf: item.cpf ?? undefined,
      dataNascimento: item.dataNascimento ?? undefined,
      parentesco: item.parentesco ?? undefined,
      dependenteIr: item.dependenteIr ?? undefined,
      salarioFamilia: item.salarioFamilia ?? undefined,
    })),
  }
}

async function carregar() {
  loading.value = true
  error.value = ''
  try {
    ficha.value = await obterFichaAdmissao(token)
    preencher(ficha.value)
    for (const documento of gruposDocumentos.value.flatMap((grupo) => grupo.documentos)) {
      if (documento.alternativas && !tipoIdentificacao.value[documento.id])
        tipoIdentificacao.value[documento.id] = documento.alternativas.find((item) => item.documentoId)?.tipoCodigo ?? 'IDENTIDADE'
    }
  }
  catch (cause) { error.value = normalizeError(cause).message }
  finally { loading.value = false }
}

function adicionarDependente() {
  form.value.dependentes.push({ nome: '', dependenteIr: false, salarioFamilia: false })
}

function removerDependente(index: number) { form.value.dependentes.splice(index, 1) }

async function salvar() {
  saving.value = true
  error.value = ''
  try { await atualizarFichaAdmissao(token, form.value); success.value = 'Dados salvos.'; await carregar() }
  catch (cause) { error.value = normalizeError(cause).message }
  finally { saving.value = false }
}

async function concluir() {
  saving.value = true
  error.value = ''
  try {
    await atualizarFichaAdmissao(token, form.value)
    await concluirFichaAdmissao(token)
    success.value = 'Ficha enviada para conferência.'
    await carregar()
  } catch (cause) { error.value = normalizeError(cause).message }
  finally { saving.value = false }
}

async function enviarDocumento(itemId: string, event: Event) {
  const input = event.target as HTMLInputElement
  const arquivos = Array.from(input.files ?? [])
  input.value = ''
  if (!arquivos.length) return
  if (arquivos.length > 10 || arquivos.reduce((total, arquivo) => total + arquivo.size, 0) > tamanhoMaximoArquivo) {
    error.value = 'Selecione no máximo 10 arquivos, somando até 10 MB.'
    return
  }
  actionId.value = itemId
  error.value = ''
  try { await enviarDocumentoFichaAdmissao(token, itemId, arquivos); success.value = 'Documento enviado.'; await carregar() }
  catch (cause) { error.value = normalizeError(cause).message }
  finally { actionId.value = '' }
}

function documentoAtivo(documento: NonNullable<FichaAdmissaoPublica['documentos'][number]>): FichaAdmissaoPublica['documentos'][number] {
  if (!documento.alternativas) return documento
  const tipo = tipoIdentificacao.value[documento.id] ?? 'IDENTIDADE'
  return documento.alternativas.find((item) => item.tipoCodigo === tipo) ?? documento.alternativas[0]!
}

async function visualizar(itemId: string) {
  actionId.value = itemId
  try { const resultado = await obterUrlDocumentoFichaAdmissao(token, itemId); window.open(resultado.url, '_blank', 'noopener,noreferrer') }
  catch (cause) { error.value = normalizeError(cause).message }
  finally { actionId.value = '' }
}

onMounted(() => void carregar())
</script>

<template>
  <main class="public-admission-page">
    <header class="public-admission-header"><a href="/" class="brand">Officio<span class="brand-dot">.</span></a><span>Ficha de admissão</span></header>
    <div v-if="loading" class="list-state" aria-busy="true"><i class="pi pi-spinner loading-spinner" /><p>Carregando ficha...</p></div>
    <div v-else-if="!ficha" class="public-admission-state"><i class="pi pi-link" /><h1>Link indisponível</h1><p>{{ error || 'Este link não está mais disponível.' }}</p></div>
    <form v-else class="public-admission-form" @submit.prevent="salvar">
      <div class="public-admission-title"><div><h1>{{ ficha.nomeFuncionario }}</h1><p>{{ ficha.empresaNome }}</p></div><StatusBadge :label="finalizado ? 'Enviada' : 'Em preenchimento'" :severity="finalizado ? 'success' : 'warn'" /></div>
      <Message v-if="success" severity="success" :closable="true" @close="success = ''">{{ success }}</Message>
      <Message v-if="error" severity="error" :closable="true" @close="error = ''">{{ error }}</Message>
      <div v-if="finalizado" class="public-admission-complete"><i class="pi pi-check-circle" /><div><h2>Ficha enviada</h2><p>Os dados estão aguardando conferência pela contabilidade.</p></div></div>

      <fieldset :disabled="finalizado || saving">
        <section class="public-form-section"><h2>Dados pessoais</h2><div class="form-grid three-columns">
          <div class="field field-span-2"><label for="public-name">Nome completo</label><InputText id="public-name" v-model="form.nomeFuncionario" required /></div>
          <div class="field"><label for="public-cpf">CPF</label><InputText id="public-cpf" v-model="form.cpf" inputmode="numeric" required /></div>
          <div class="field"><label for="public-rg">RG</label><InputText id="public-rg" v-model="form.rg" /></div>
          <div class="field"><label for="public-birth">Data de nascimento</label><input id="public-birth" v-model="form.dataNascimento" class="native-input" type="date" required /></div>
          <div class="field"><label for="public-sex">Sexo</label><Select input-id="public-sex" v-model="form.sexo" :options="sexos" /></div>
          <div class="field"><label for="public-civil">Estado civil</label><Select input-id="public-civil" v-model="form.estadoCivil" :options="estadosCivis" /></div>
          <div class="field field-span-2"><label for="public-school">Escolaridade</label><InputText id="public-school" v-model="form.escolaridade" /></div>
        </div></section>

        <section class="public-form-section"><h2>Contato e vínculo</h2><div class="form-grid three-columns">
          <div class="field"><label for="public-phone">Telefone</label><InputText id="public-phone" v-model="form.telefone" /></div>
          <div class="field field-span-2"><label for="public-email">E-mail</label><InputText id="public-email" v-model="form.email" type="email" /></div>
          <div class="field"><label>Função</label><InputText :model-value="ficha.funcao || 'Não informada'" disabled /></div>
          <div class="field"><label>Admissão prevista</label><InputText :model-value="formatDate(ficha.dataAdmissaoPrevista)" disabled /></div>
          <label class="check-field"><Checkbox v-model="form.utilizaValeTransporte" binary />Utilizarei vale-transporte</label>
        </div></section>

        <section class="public-form-section"><h2>Dados bancários</h2><div class="form-grid five-columns">
          <div class="field"><label for="public-pis">PIS/PASEP</label><InputText id="public-pis" v-model="form.pis" /></div>
          <div class="field"><label for="public-bank">Banco</label><InputText id="public-bank" v-model="form.banco" /></div>
          <div class="field"><label for="public-agency">Agência</label><InputText id="public-agency" v-model="form.agencia" /></div>
          <div class="field"><label for="public-account">Conta</label><InputText id="public-account" v-model="form.conta" /></div>
          <div class="field"><label for="public-account-type">Tipo de conta</label><Select input-id="public-account-type" v-model="form.tipoConta" :options="tiposConta" /></div>
        </div></section>

        <section class="public-form-section"><div class="public-section-heading"><h2>Dependentes</h2><BaseButton label="Adicionar" icon="pi pi-plus" variant="secondary" @click="adicionarDependente" /></div>
          <div v-if="form.dependentes.length" class="public-dependent-list"><div v-for="(dependente, index) in form.dependentes" :key="index" class="public-dependent-row">
            <div class="field"><label :for="`dep-name-${index}`">Nome</label><InputText :id="`dep-name-${index}`" v-model="dependente.nome" required /></div>
            <div class="field"><label :for="`dep-cpf-${index}`">CPF</label><InputText :id="`dep-cpf-${index}`" v-model="dependente.cpf" /></div>
            <div class="field"><label :for="`dep-birth-${index}`">Nascimento</label><input :id="`dep-birth-${index}`" v-model="dependente.dataNascimento" class="native-input" type="date" /></div>
            <div class="field"><label :for="`dep-relation-${index}`">Parentesco</label><InputText :id="`dep-relation-${index}`" v-model="dependente.parentesco" /></div>
            <label class="check-field"><Checkbox v-model="dependente.dependenteIr" binary />Dependente no IR</label>
            <label class="check-field"><Checkbox v-model="dependente.salarioFamilia" binary />Salário-família</label>
            <BaseButton icon="pi pi-trash" variant="text" title="Remover dependente" aria-label="Remover dependente" @click="removerDependente(index)" />
          </div></div><p v-else class="muted">Nenhum dependente informado.</p>
        </section>

        <section class="public-form-section"><h2>Documentos</h2><div class="public-document-groups"><section v-for="grupo in gruposDocumentos" :key="grupo.codigo" class="public-document-group"><h3><i :class="grupo.icone" aria-hidden="true" />{{ grupo.titulo }}<span>{{ grupo.documentos.length }}</span></h3><div class="public-document-list"><div v-for="documento in grupo.documentos" :key="documento.id" class="public-document-row">
          <div><strong>{{ documento.rotulo }}</strong><div v-if="documento.alternativas" class="document-type-choice"><label v-for="opcao in documento.alternativas" :key="opcao.id"><input v-model="tipoIdentificacao[documento.id]" type="radio" :name="`public-identificacao-${documento.id}`" :value="opcao.tipoCodigo" :disabled="Boolean(actionId)">{{ opcao.tipoCodigo === 'CNH' ? 'CNH' : 'Identidade (RG/CIN)' }}</label></div><span>{{ documentoAtivo(documento).nomeArquivo || (documento.obrigatorio ? 'Obrigatório' : 'Opcional') }}</span></div>
          <StatusBadge :label="documentoAtivo(documento).documentoId ? 'Enviado' : documento.obrigatorio ? 'Pendente' : 'Opcional'" :severity="documentoAtivo(documento).documentoId ? 'success' : documento.obrigatorio ? 'warn' : 'secondary'" />
          <div class="document-actions"><BaseButton v-if="documentoAtivo(documento).documentoId" icon="pi pi-eye" variant="text" title="Visualizar documento" aria-label="Visualizar documento" :loading="actionId === documentoAtivo(documento).id" :disabled="Boolean(actionId)" @click="visualizar(documentoAtivo(documento).id)" /><label class="document-upload" :class="{ disabled: Boolean(actionId) }" :for="`public-doc-${documentoAtivo(documento).id}`"><i :class="actionId === documentoAtivo(documento).id ? 'pi pi-spinner loading-spinner' : documentoAtivo(documento).documentoId ? 'pi pi-refresh' : 'pi pi-upload'" />{{ documentoAtivo(documento).documentoId ? 'Substituir' : 'Enviar' }}</label><input :id="`public-doc-${documentoAtivo(documento).id}`" class="visually-hidden" type="file" multiple accept=".pdf,.jpg,.jpeg,.png" :disabled="Boolean(actionId)" @change="enviarDocumento(documentoAtivo(documento).id, $event)" /></div>
        </div></div></section></div></section>
      </fieldset>

      <footer v-if="!finalizado" class="public-form-actions"><span>Link válido até {{ formatDate(ficha.expiraEm) }}</span><div><BaseButton label="Salvar" icon="pi pi-save" variant="secondary" :loading="saving" type="submit" /><BaseButton label="Enviar para conferência" icon="pi pi-check" :loading="saving" @click="concluir" /></div></footer>
    </form>
  </main>
</template>
