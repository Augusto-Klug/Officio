<script setup lang="ts">
import { onBeforeUnmount, ref } from 'vue'
import Message from 'primevue/message'
import BaseInput from '../../../components/base/BaseInput.vue'
import BaseButton from '../../../components/base/BaseButton.vue'
import BaseCard from '../../../components/base/BaseCard.vue'
import PageHeader from '../../../components/layout/PageHeader.vue'
import EnderecoCadastro from '../../../components/shared/EnderecoCadastro.vue'
import { normalizeError } from '../../../api/errors'
import { formatarCnpj, formatarCpf, formatarTelefone } from '../../../utils/inputMasks'
import { obterDadosContabilidade, atualizarDadosContabilidade } from '../contabilidade.service'
import { novoEndereco, prepararDados, validarDados, temEndereco, type DadosContabilidade } from '../contabilidade-form'

const form = ref<DadosContabilidade | null>(null)
const endereco = ref(novoEndereco())
const enderecoTecnico = ref(novoEndereco())
const tecnico = ref({ nome: '', cpf: '', profissao: '', crc: '' })
const possuiTecnico = ref(false)
const loading = ref(true)
const saving = ref(false)
const error = ref('')
const success = ref('')
const enderecoComponente = ref<InstanceType<typeof EnderecoCadastro>>()
const enderecoTecnicoComponente = ref<InstanceType<typeof EnderecoCadastro>>()
let controller: AbortController | undefined

function preencher(dados: DadosContabilidade) {
  form.value = { ...dados, cnpj: formatarCnpj(dados.cnpj), telefone: formatarTelefone(dados.telefone) }
  endereco.value = dados.endereco || novoEndereco()
  possuiTecnico.value = !!dados.responsavelTecnico
  const r = dados.responsavelTecnico
  tecnico.value = r ? { nome: r.nome, cpf: formatarCpf(r.cpf), profissao: r.profissao, crc: r.crc } : { nome: '', cpf: '', profissao: '', crc: '' }
  enderecoTecnico.value = r?.endereco || novoEndereco()
}
async function carregar() {
  controller?.abort(); const current = new AbortController(); controller = current; loading.value = true; error.value = ''
  try { const dados = await obterDadosContabilidade(current.signal); if (!current.signal.aborted) preencher(dados) }
  catch (cause) { if (!current.signal.aborted) error.value = normalizeError(cause).message }
  finally { if (controller === current) loading.value = false }
}
async function salvar() {
  if (!form.value || saving.value) return
  saving.value = true; error.value = ''; success.value = ''
  try {
    const temTecnico = possuiTecnico.value || Object.values(tecnico.value).some(v => v.trim()) || temEndereco(enderecoTecnico.value)
    const dados = prepararDados({ ...form.value, endereco: endereco.value,
      responsavelTecnico: temTecnico ? { ...tecnico.value, endereco: enderecoTecnico.value } : null })
    error.value = validarDados(dados)
    if (error.value) return
    if (!await enderecoComponente.value?.validar()) { error.value = 'Confira o CEP da contabilidade.'; return }
    if (temTecnico && !await enderecoTecnicoComponente.value?.validar()) { error.value = 'Confira o CEP do responsável técnico.'; return }
    preencher(await atualizarDadosContabilidade(dados))
    success.value = 'Dados da contabilidade atualizados.'
  } catch (cause) { error.value = normalizeError(cause).message }
  finally { saving.value = false }
}
void carregar()
onBeforeUnmount(() => controller?.abort())
</script>

<template>
  <PageHeader title="Dados da contabilidade" description="Mantenha o cadastro do escritório e de seu responsável técnico atualizado." />
  <Message v-if="error" severity="error" :closable="false" class="page-message">{{ error }}</Message>
  <Message v-if="success" severity="success" class="page-message" role="status">{{ success }}</Message>
  <div v-if="loading" class="list-state" aria-busy="true"><i class="pi pi-spinner loading-spinner" aria-hidden="true" /><p>Carregando dados da contabilidade…</p></div>
  <BaseButton v-else-if="!form" label="Tentar novamente" icon="pi pi-refresh" variant="secondary" @click="carregar" />
  <form v-else class="accounting-form" @submit.prevent="salvar">
    <fieldset :disabled="saving">
      <BaseCard class="accounting-section">
        <h2>Dados gerais</h2>
        <div class="form-grid two-columns">
          <BaseInput id="contabilidade-razao" v-model="form.razaoSocial" label="Razão social" maxlength="300" required />
          <BaseInput id="contabilidade-fantasia" v-model="form.nomeFantasia" label="Nome fantasia" maxlength="300" />
          <BaseInput id="contabilidade-cnpj" :model-value="form.cnpj" label="CNPJ" inputmode="numeric" maxlength="18" required @update:model-value="form.cnpj = formatarCnpj($event)" />
          <BaseInput id="contabilidade-crc" v-model="form.crc" label="CRC da contabilidade" maxlength="50" />
          <BaseInput id="contabilidade-email" v-model="form.email" label="E-mail" type="email" maxlength="250" autocomplete="email" />
          <BaseInput id="contabilidade-telefone" :model-value="form.telefone" label="Telefone" inputmode="tel" maxlength="15" autocomplete="tel" @update:model-value="form.telefone = formatarTelefone($event)" />
        </div>
      </BaseCard>
      <BaseCard class="accounting-section"><h2>Endereço da contabilidade</h2><EnderecoCadastro ref="enderecoComponente" v-model="endereco" prefix="contabilidade" :disabled="saving" /></BaseCard>
      <BaseCard class="accounting-section">
        <h2>Responsável técnico</h2><p class="section-description">Para cadastrar, informe nome, CPF, profissão e CRC.</p>
        <div class="form-grid two-columns">
          <BaseInput id="tecnico-nome" v-model="tecnico.nome" label="Nome" maxlength="250" autocomplete="name" />
          <BaseInput id="tecnico-cpf" :model-value="tecnico.cpf" label="CPF" inputmode="numeric" maxlength="14" @update:model-value="tecnico.cpf = formatarCpf($event)" />
          <BaseInput id="tecnico-profissao" v-model="tecnico.profissao" label="Profissão" maxlength="150" />
          <BaseInput id="tecnico-crc" v-model="tecnico.crc" label="CRC do responsável técnico" maxlength="50" />
        </div>
        <h3>Endereço do responsável técnico</h3><EnderecoCadastro ref="enderecoTecnicoComponente" v-model="enderecoTecnico" prefix="tecnico" :disabled="saving" />
      </BaseCard>
    </fieldset>
    <div class="accounting-actions"><BaseButton label="Salvar alterações" icon="pi pi-check" type="submit" :loading="saving" /></div>
  </form>
</template>

<style scoped>
.accounting-form { max-width: 980px; }
fieldset { border: 0; padding: 0; margin: 0; min-width: 0; display: grid; gap: 1.5rem; }
.accounting-section h2 { margin: 0 0 1.25rem; font-size: 1.125rem; }
.accounting-section h3 { margin: 2rem 0 1rem; font-size: 1rem; }
.section-description { margin: -0.5rem 0 1.25rem; color: var(--officio-muted); }
.accounting-actions { display: flex; justify-content: flex-end; padding: 1.5rem 0; }
</style>
