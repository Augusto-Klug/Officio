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
import { formatarCpf, formatarTelefone, somenteDigitos } from '../../../utils/inputMasks'
import { atualizarFichaAdmissao, concluirFichaAdmissao, enviarDocumentoFichaAdmissao, obterFichaAdmissao, obterUrlDocumentoFichaAdmissao } from '../services/admissoes.service'
import type { AtualizarFichaFuncionarioPayload, FichaFuncionarioPublica } from '../types/admissao'
import { agruparDocumentosAdmissao } from '../documentos'
import { escolaridadesAdmissao, estadosCivisAdmissao, etniasAdmissao, opcoesComValorAtual, sexosAdmissao, tiposContaAdmissao } from '../types/opcoesAdmissao'

const route = useRoute()
const token = String(route.params.token || '')
const ficha = ref<FichaFuncionarioPublica | null>(null)
const form = ref<AtualizarFichaFuncionarioPayload>({ nomeFuncionario: '', dependentes: [] })
const identidade = ref({ nomeFuncionario: '', cpf: '', rg: '', dataNascimento: '', sexo: '', etnia: '' })
const loading = ref(true)
const saving = ref(false)
const actionId = ref('')
const error = ref('')
const success = ref('')
const finalizado = computed(() => ficha.value?.preenchimentoConcluido === true)
const gruposDocumentos = computed(() => agruparDocumentosAdmissao(ficha.value?.documentos ?? []))
const tipoIdentificacao = ref<Record<string, string>>({})
const tamanhoMaximoArquivo = 10 * 1024 * 1024

function preencher(dados: FichaFuncionarioPublica) {
  identidade.value = {
    nomeFuncionario: dados.nomeFuncionario,
    cpf: formatarCpf(dados.cpf),
    rg: dados.rg ?? '',
    dataNascimento: dados.dataNascimento ?? '',
    sexo: dados.sexo ?? '',
    etnia: dados.etnia ?? '',
  }
  form.value = {
    nomeFuncionario: dados.nomeFuncionario,
    cpf: somenteDigitos(dados.cpf) || undefined,
    rg: dados.rg ?? '',
    etnia: dados.etnia ?? '',
    dataNascimento: dados.dataNascimento ?? undefined,
    sexo: dados.sexo ?? undefined,
    estadoCivil: dados.estadoCivil ?? '',
    escolaridade: dados.escolaridade ?? '',
    telefone: formatarTelefone(dados.telefone),
    email: dados.email ?? '',
    banco: dados.banco ?? '',
    agencia: dados.agencia ?? '',
    conta: dados.conta ?? '',
    tipoConta: dados.tipoConta ?? '',
    dependentes: dados.dependentes.map((item) => ({
      nome: item.nome,
      cpf: formatarCpf(item.cpf) || undefined,
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
  } catch (cause) { error.value = normalizeError(cause).message }
  finally { loading.value = false }
}

function adicionarDependente() {
  form.value.dependentes.push({ nome: '', dependenteIr: false, salarioFamilia: false })
}

function removerDependente(index: number) { form.value.dependentes.splice(index, 1) }

async function salvar() {
  saving.value = true
  error.value = ''
  try { await atualizarFichaAdmissao(token, prepararPayload()); success.value = 'Dados salvos.'; await carregar() }
  catch (cause) { error.value = normalizeError(cause).message }
  finally { saving.value = false }
}

async function concluir() {
  saving.value = true
  error.value = ''
  try {
    await atualizarFichaAdmissao(token, prepararPayload())
    await concluirFichaAdmissao(token)
    success.value = 'Ficha enviada para conferência.'
    await carregar()
  } catch (cause) { error.value = normalizeError(cause).message }
  finally { saving.value = false }
}

function prepararPayload(): AtualizarFichaFuncionarioPayload {
  return {
    ...form.value,
    ...identidade.value,
    telefone: somenteDigitos(form.value.telefone) || undefined,
    dependentes: form.value.dependentes.map((item) => ({
      ...item,
      cpf: somenteDigitos(item.cpf) || undefined,
    })),
  }
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
  try {
    await enviarDocumentoFichaAdmissao(token, itemId, arquivos)
    success.value = 'Documento enviado.'
    try {
      const fichaAtualizada = await obterFichaAdmissao(token)
      if (ficha.value) ficha.value.documentos = fichaAtualizada.documentos
    } catch {
      error.value = 'Documento enviado, mas não foi possível atualizar a lista. Recarregue a ficha para confirmar.'
    }
  }
  catch (cause) { error.value = normalizeError(cause).message }
  finally { actionId.value = '' }
}

function documentoAtivo(documento: AdmissaoDocumentoFicha): AdmissaoDocumentoFicha {
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

function exibir(valor: string | null | undefined): string { return valor?.trim() || 'Não informado' }
type AdmissaoDocumentoFicha = NonNullable<FichaFuncionarioPublica['documentos'][number]>

onMounted(() => void carregar())
</script>

<template>
  <main class="public-admission-page">
    <header class="public-admission-header"><a href="/" class="brand">Officio<span class="brand-dot">.</span></a><span>Ficha do admitido</span></header>
    <div v-if="loading" class="list-state" aria-busy="true"><i class="pi pi-spinner loading-spinner" /><p>Carregando ficha...</p></div>
    <div v-else-if="!ficha" class="public-admission-state"><i class="pi pi-link" /><h1>Link indisponível</h1><p>{{ error || 'Este link não está mais disponível.' }}</p></div>
    <form v-else class="public-admission-form" @submit.prevent="salvar">
      <div class="public-admission-title"><div><h1>{{ ficha.nomeFuncionario }}</h1><p>{{ ficha.empresaNome }}</p></div><StatusBadge :label="finalizado ? 'Enviada' : 'Em preenchimento'" :severity="finalizado ? 'success' : 'warn'" /></div>
      <Message v-if="success" severity="success" :closable="true" @close="success = ''">{{ success }}</Message>
      <Message v-if="error" severity="error" :closable="true" @close="error = ''">{{ error }}</Message>
      <div v-if="finalizado" class="public-admission-complete"><i class="pi pi-check-circle" /><div><h2>Ficha enviada</h2><p>O empregador já pode preencher os dados de contratação. A contabilidade revisará os documentos depois que as duas fichas forem enviadas.</p></div></div>

      <fieldset :disabled="finalizado || saving">
        <section class="public-form-section"><h2>Identificação</h2><div class="form-grid three-columns">
          <div class="field field-span-2"><label for="public-name">Nome completo</label><InputText id="public-name" v-model="identidade.nomeFuncionario" required :disabled="finalizado || saving" /></div>
          <div class="field"><label for="public-cpf">CPF</label><InputText id="public-cpf" :model-value="identidade.cpf" inputmode="numeric" maxlength="14" placeholder="000.000.000-00" required :disabled="finalizado || saving" @update:model-value="identidade.cpf = formatarCpf($event)" /></div>
          <div class="field"><label for="public-rg">RG</label><InputText id="public-rg" v-model="identidade.rg" maxlength="30" :disabled="finalizado || saving" /></div>
          <div class="field"><label for="public-birth">Data de nascimento</label><InputText id="public-birth" v-model="identidade.dataNascimento" type="date" required :disabled="finalizado || saving" /></div>
          <div class="field"><label for="public-sex">Sexo</label><Select input-id="public-sex" v-model="identidade.sexo" :options="opcoesComValorAtual(identidade.sexo, sexosAdmissao)" :disabled="finalizado || saving" placeholder="Selecione" required /></div>
          <div class="field"><label for="public-ethnicity">Etnia</label><Select input-id="public-ethnicity" v-model="identidade.etnia" :options="etniasAdmissao" :disabled="finalizado || saving" placeholder="Selecione" /></div>
        </div></section>

        <section class="public-form-section"><h2>Dados pessoais e contato</h2><div class="form-grid three-columns">
          <div class="field"><label for="public-civil">Estado civil</label><Select input-id="public-civil" v-model="form.estadoCivil" :options="opcoesComValorAtual(form.estadoCivil, estadosCivisAdmissao)" :disabled="finalizado || saving" placeholder="Selecione" /></div>
          <div class="field"><label for="public-school">Escolaridade</label><Select input-id="public-school" v-model="form.escolaridade" :options="opcoesComValorAtual(form.escolaridade, escolaridadesAdmissao)" :disabled="finalizado || saving" placeholder="Selecione" /></div>
          <div class="field"><label for="public-phone">Telefone</label><InputText id="public-phone" :model-value="form.telefone" inputmode="tel" autocomplete="tel" maxlength="15" placeholder="(00) 00000-0000" :disabled="finalizado || saving" @update:model-value="form.telefone = formatarTelefone($event)" /></div>
          <div class="field field-span-2"><label for="public-email">E-mail</label><InputText id="public-email" v-model="form.email" type="email" :disabled="finalizado || saving" /></div>
        </div></section>

        <section class="public-form-section"><h2>Dados bancários</h2><div class="form-grid four-columns">
          <div class="field"><label for="employee-bank">Banco</label><InputText id="employee-bank" v-model="form.banco" autocomplete="off" :disabled="finalizado || saving" /></div>
          <div class="field"><label for="employee-branch">Agência</label><InputText id="employee-branch" v-model="form.agencia" autocomplete="off" :disabled="finalizado || saving" /></div>
          <div class="field"><label for="employee-account">Conta</label><InputText id="employee-account" v-model="form.conta" autocomplete="off" :disabled="finalizado || saving" /></div>
          <div class="field"><label for="employee-account-type">Tipo de conta</label><Select input-id="employee-account-type" v-model="form.tipoConta" :options="tiposContaAdmissao" :disabled="finalizado || saving" placeholder="Selecione" /></div>
        </div></section>

        <section class="public-form-section"><div class="public-section-heading"><h2>Dependentes</h2><BaseButton label="Adicionar" icon="pi pi-plus" variant="secondary" type="button" :disabled="finalizado || saving" @click="adicionarDependente" /></div>
          <div v-if="form.dependentes.length" class="public-dependent-list"><div v-for="(dependente, index) in form.dependentes" :key="index" class="public-dependent-row">
            <div class="field"><label :for="`dep-name-${index}`">Nome</label><InputText :id="`dep-name-${index}`" v-model="dependente.nome" required :disabled="finalizado || saving" /></div>
            <div class="field"><label :for="`dep-cpf-${index}`">CPF</label><InputText :id="`dep-cpf-${index}`" :model-value="dependente.cpf" inputmode="numeric" maxlength="14" placeholder="000.000.000-00" :disabled="finalizado || saving" @update:model-value="dependente.cpf = formatarCpf($event)" /></div>
            <div class="field"><label :for="`dep-birth-${index}`">Nascimento</label><InputText :id="`dep-birth-${index}`" v-model="dependente.dataNascimento" type="date" :disabled="finalizado || saving" /></div>
            <div class="field"><label :for="`dep-relation-${index}`">Parentesco</label><InputText :id="`dep-relation-${index}`" v-model="dependente.parentesco" :disabled="finalizado || saving" /></div>
            <label class="check-field"><Checkbox v-model="dependente.dependenteIr" binary :disabled="finalizado || saving" />Dependente no IR</label>
            <label class="check-field"><Checkbox v-model="dependente.salarioFamilia" binary :disabled="finalizado || saving" />Salário-família</label>
            <BaseButton icon="pi pi-trash" variant="text" title="Remover dependente" aria-label="Remover dependente" type="button" :disabled="finalizado || saving" @click="removerDependente(index)" />
          </div></div><p v-else class="muted">Nenhum dependente informado.</p>
        </section>

        <section class="public-form-section"><h2>Documentos</h2><div class="public-document-groups"><section v-for="grupo in gruposDocumentos" :key="grupo.codigo" class="public-document-group"><h3><i :class="grupo.icone" aria-hidden="true" />{{ grupo.titulo }}<span>{{ grupo.documentos.length }}</span></h3><div class="public-document-list"><div v-for="documento in grupo.documentos" :key="documento.id" class="public-document-row">
          <div><strong>{{ documento.rotulo }}</strong><div v-if="documento.alternativas" class="document-type-choice"><label v-for="opcao in documento.alternativas" :key="opcao.id"><input v-model="tipoIdentificacao[documento.id]" type="radio" :name="`public-identificacao-${documento.id}`" :value="opcao.tipoCodigo" :disabled="finalizado || Boolean(actionId)">{{ opcao.tipoCodigo === 'CNH' ? 'CNH' : 'Identidade (RG/CIN)' }}</label></div><span>{{ documentoAtivo(documento).nomeArquivo || (documento.obrigatorio ? 'Obrigatório' : 'Opcional') }}</span></div>
          <StatusBadge :label="documentoAtivo(documento).documentoId ? 'Enviado' : documento.obrigatorio ? 'Pendente' : 'Opcional'" :severity="documentoAtivo(documento).documentoId ? 'success' : documento.obrigatorio ? 'warn' : 'secondary'" />
          <div class="document-actions"><BaseButton v-if="documentoAtivo(documento).documentoId" icon="pi pi-eye" variant="text" title="Visualizar documento" aria-label="Visualizar documento" :loading="actionId === documentoAtivo(documento).id" :disabled="Boolean(actionId)" @click="visualizar(documentoAtivo(documento).id)" /><label v-if="!finalizado" class="document-upload" :class="{ disabled: Boolean(actionId) }" :for="`public-doc-${documentoAtivo(documento).id}`"><i :class="actionId === documentoAtivo(documento).id ? 'pi pi-spinner loading-spinner' : documentoAtivo(documento).documentoId ? 'pi pi-refresh' : 'pi pi-upload'" />{{ documentoAtivo(documento).documentoId ? 'Substituir' : 'Enviar' }}</label><input :id="`public-doc-${documentoAtivo(documento).id}`" class="visually-hidden" type="file" multiple accept=".pdf,.jpg,.jpeg,.png" :disabled="finalizado || Boolean(actionId)" @change="enviarDocumento(documentoAtivo(documento).id, $event)" /></div>
        </div></div></section></div></section>
      </fieldset>

      <footer v-if="!finalizado" class="public-form-actions"><span>Link válido até {{ formatDate(ficha.expiraEm) }}</span><div><BaseButton label="Salvar" icon="pi pi-save" variant="secondary" :loading="saving" type="submit" /><BaseButton label="Enviar para conferência" icon="pi pi-check" :loading="saving" type="button" @click="concluir" /></div></footer>
    </form>
  </main>
</template>
