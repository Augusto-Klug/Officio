<script setup lang="ts">
import { computed, nextTick, onBeforeUnmount, reactive, ref, watch } from 'vue'
import Dialog from 'primevue/dialog'
import InputNumber from 'primevue/inputnumber'
import Message from 'primevue/message'
import Select from 'primevue/select'
import BaseButton from '../../../components/base/BaseButton.vue'
import BaseInput from '../../../components/base/BaseInput.vue'
import { normalizeError } from '../../../api/errors'
import { consultarCep, listarEstados, listarMunicipios } from '../services/localidades.service'
import { criarProspecto } from '../services/prospectos.service'
import type { EstadoLocalidade, MunicipioLocalidade } from '../types/localidade'
import { regimeTributarioOptions, type ProspectoCriado, type RegimeTributario } from '../types/prospecto'
import { formatarCep, formatarCnpj, formatarCpf, limparPayload, novoProspecto, somenteDigitos, validarProspecto, type ProspectoFormErrors } from './prospecto-form'

const props = defineProps<{ visible: boolean }>()
const emit = defineEmits<{
  'update:visible': [value: boolean]
  criado: [prospecto: ProspectoCriado]
}>()
const dialogVisible = computed({ get: () => props.visible, set: (value) => emit('update:visible', value) })
const form = reactive({
  razaoSocial: '', nomeFantasia: '', cnpj: '', inscricaoEstadual: '', inscricaoMunicipal: '', tipoEmpresa: '',
  regimeTributario: null as RegimeTributario | null, porteEmpresa: '', cnaePrincipal: '', dataAbertura: '',
  email: '', telefone: '', whatsapp: '', nomeContato: '', cep: '', logradouro: '', numero: '', bairro: '',
  complemento: '', cidade: '', uf: '', municipioCodigoIbge: '', origem: '',
  socios: [] as Array<{ nome: string; cpf: string; participacao: number }>,
})
const errors = ref<ProspectoFormErrors>({})
const submitError = ref('')
const loading = ref(false)
const estados = ref<EstadoLocalidade[]>([])
const municipios = ref<MunicipioLocalidade[]>([])
const estadosLoading = ref(false)
const municipiosLoading = ref(false)
const cepLoading = ref(false)
const enderecoAviso = ref('')
type CepStatus = 'vazio' | 'pendente' | 'valido' | 'invalido' | 'indisponivel'
const cepStatus = ref<CepStatus>('vazio')
const cepConsultado = ref('')
const localidadeBloqueadaPeloCep = ref(false)
let cepTimer: ReturnType<typeof setTimeout> | undefined
let cepController: AbortController | undefined
let municipiosController: AbortController | undefined
const participacaoTotal = computed(() => form.socios.reduce((total, socio) => total + (socio.participacao || 0), 0))
const estadoOptions = computed(() => estados.value.map((estado) => ({
  label: `${estado.sigla} - ${estado.nome}`,
  value: estado.sigla,
})))
const municipioOptions = computed(() => municipios.value.map((municipio) => ({
  label: municipio.nome,
  value: municipio.codigoIbge,
})))
const enderecoAvisoSeveridade = computed(() => cepStatus.value === 'indisponivel' ? 'error' : 'secondary')

function reset() {
  form.razaoSocial = ''
  form.nomeFantasia = ''
  form.cnpj = ''
  form.inscricaoEstadual = ''
  form.inscricaoMunicipal = ''
  form.tipoEmpresa = ''
  form.regimeTributario = null
  form.porteEmpresa = ''
  form.cnaePrincipal = ''
  form.dataAbertura = ''
  form.email = ''
  form.telefone = ''
  form.whatsapp = ''
  form.nomeContato = ''
  form.cep = ''
  form.logradouro = ''
  form.numero = ''
  form.complemento = ''
  form.bairro = ''
  form.cidade = ''
  form.uf = ''
  form.municipioCodigoIbge = ''
  form.origem = ''
  form.socios = []
  errors.value = {}
  submitError.value = ''
  enderecoAviso.value = ''
  cepStatus.value = 'vazio'
  cepConsultado.value = ''
  localidadeBloqueadaPeloCep.value = false
  municipios.value = []
  cancelarConsultasEndereco()
}

function close() { if (!loading.value) dialogVisible.value = false }
function addSocio() { form.socios.push({ nome: '', cpf: '', participacao: 0 }) }
function removeSocio(index: number) { form.socios.splice(index, 1); errors.value = {} }
function fieldError(name: string) { return errors.value[name] }

function cancelarConsultasEndereco() {
  if (cepTimer) clearTimeout(cepTimer)
  cepController?.abort()
  municipiosController?.abort()
  cepLoading.value = false
  municipiosLoading.value = false
}

async function carregarEstados() {
  if (estados.value.length || estadosLoading.value) return
  estadosLoading.value = true
  try {
    estados.value = await listarEstados()
  } catch (cause) {
    enderecoAviso.value = normalizeError(cause).message
  } finally {
    estadosLoading.value = false
  }
}

async function carregarMunicipios(uf: string, municipioSelecionado = '') {
  municipiosController?.abort()
  municipios.value = []
  if (!uf) return

  const controller = new AbortController()
  municipiosController = controller
  municipiosLoading.value = true
  try {
    municipios.value = await listarMunicipios(uf, controller.signal)
    if (municipioSelecionado && municipios.value.some((item) => item.codigoIbge === municipioSelecionado)) {
      form.municipioCodigoIbge = municipioSelecionado
      form.cidade = municipios.value.find((item) => item.codigoIbge === municipioSelecionado)?.nome || ''
    }
  } catch (cause) {
    if (!controller.signal.aborted) enderecoAviso.value = normalizeError(cause).message
  } finally {
    if (municipiosController === controller) municipiosLoading.value = false
  }
}

async function alterarUf(uf: string | null) {
  form.uf = uf || ''
  form.municipioCodigoIbge = ''
  form.cidade = ''
  enderecoAviso.value = ''
  await carregarMunicipios(form.uf)
}

function alterarMunicipio(codigoIbge: string | null) {
  form.municipioCodigoIbge = codigoIbge || ''
  form.cidade = municipios.value.find((item) => item.codigoIbge === codigoIbge)?.nome || ''
}

async function preencherEnderecoPorCep(cep: string) {
  if (cepTimer) {
    clearTimeout(cepTimer)
    cepTimer = undefined
  }
  cepController?.abort()
  const controller = new AbortController()
  cepController = controller
  cepLoading.value = true
  cepStatus.value = 'pendente'
  cepConsultado.value = cep
  enderecoAviso.value = ''
  if (errors.value.cep) {
    const { cep: _, ...outrosErros } = errors.value
    errors.value = outrosErros
  }
  try {
    const endereco = await consultarCep(cep, controller.signal)
    if (controller.signal.aborted) return

    form.logradouro = endereco.logradouro || ''
    form.bairro = endereco.bairro || ''
    form.uf = endereco.uf
    await carregarMunicipios(endereco.uf, endereco.municipioCodigoIbge)
    cepStatus.value = 'valido'
    localidadeBloqueadaPeloCep.value = true
    enderecoAviso.value = endereco.logradouro
      ? 'Endereço localizado. Estado e município foram definidos pelo CEP. Informe o número e o complemento, se houver.'
      : 'CEP localizado. Estado e município foram definidos pelo CEP. Complete o logradouro, o número e o complemento.'
  } catch (cause) {
    if (!controller.signal.aborted) {
      const erro = normalizeError(cause)
      if (erro.status === 404) {
        cepStatus.value = 'invalido'
        localidadeBloqueadaPeloCep.value = false
        errors.value = {
          ...errors.value,
          cep: 'CEP inválido. Informe um CEP válido para continuar o cadastro.',
        }
        enderecoAviso.value = ''
      } else {
        cepStatus.value = 'indisponivel'
        localidadeBloqueadaPeloCep.value = false
        enderecoAviso.value = erro.message
      }
    }
  } finally {
    if (cepController === controller) cepLoading.value = false
  }
}

watch(() => somenteDigitos(form.cep), (cep) => {
  if (cepTimer) clearTimeout(cepTimer)
  cepController?.abort()
  enderecoAviso.value = ''
  cepConsultado.value = ''
  cepStatus.value = cep ? 'pendente' : 'vazio'
  if (!cep) localidadeBloqueadaPeloCep.value = false
  if (errors.value.cep) {
    const { cep: _, ...outrosErros } = errors.value
    errors.value = outrosErros
  }
  if (cep.length === 8) cepTimer = setTimeout(() => preencherEnderecoPorCep(cep), 350)
})

async function handleShow() {
  errors.value = {}
  submitError.value = ''
  enderecoAviso.value = ''
  await carregarEstados()
}

onBeforeUnmount(cancelarConsultasEndereco)

async function focusFirstInvalid() {
  await nextTick()
  const first = Object.keys(errors.value)[0]
  if (first) document.getElementById(first.replaceAll('.', '-'))?.focus()
}

function montarPayload() {
  return limparPayload({
    ...novoProspecto(),
    razaoSocial: form.razaoSocial,
    nomeFantasia: form.nomeFantasia,
    cnpj: form.cnpj,
    inscricaoEstadual: form.inscricaoEstadual,
    inscricaoMunicipal: form.inscricaoMunicipal,
    tipoEmpresa: form.tipoEmpresa,
    regimeTributario: form.regimeTributario,
    porteEmpresa: form.porteEmpresa,
    cnaePrincipal: form.cnaePrincipal,
    dataAbertura: form.dataAbertura,
    email: form.email,
    telefone: form.telefone,
    whatsapp: form.whatsapp,
    nomeContato: form.nomeContato,
    cep: form.cep,
    logradouro: form.logradouro,
    numero: form.numero,
    complemento: form.complemento,
    bairro: form.bairro,
    cidade: form.cidade,
    uf: form.uf,
    municipioCodigoIbge: form.municipioCodigoIbge,
    origem: form.origem,
    socios: form.socios.map((socio) => ({ ...socio, cpf: socio.cpf })),
  })
}

async function submit() {
  submitError.value = ''
  let payload = montarPayload()
  errors.value = validarProspecto(payload)
  if (Object.keys(errors.value).length) { await focusFirstInvalid(); return }

  const cepAtual = somenteDigitos(form.cep)
  if (cepAtual && (cepStatus.value !== 'valido' || cepConsultado.value !== cepAtual)) {
    await preencherEnderecoPorCep(cepAtual)
  }

  if (cepStatus.value === 'invalido') {
    errors.value = validarProspecto(payload, true)
    await focusFirstInvalid()
    return
  }

  if (cepAtual && cepStatus.value !== 'valido' && cepStatus.value !== 'indisponivel') {
    submitError.value = 'Não foi possível validar o CEP. Tente novamente em instantes.'
    return
  }

  payload = montarPayload()
  loading.value = true
  try {
    const prospecto = await criarProspecto(payload)
    emit('criado', prospecto)
    reset()
    dialogVisible.value = false
  } catch (cause) {
    const error = normalizeError(cause)
    submitError.value = error.message
    await focusFirstInvalid()
  } finally { loading.value = false }
}
</script>

<template>
  <Dialog v-model:visible="dialogVisible" modal header="Cadastrar prospecto" class="prospect-dialog"
    :style="{ width: 'min(760px, calc(100vw - 24px))' }" :closable="!loading" :close-on-escape="!loading"
    :draggable="false" aria-describedby="prospect-dialog-description" @show="handleShow">
    <p id="prospect-dialog-description" class="dialog-description">Cadastre os dados comerciais usados na proposta e na futura conversão em cliente.</p>
    <form id="prospect-form" class="prospect-form" novalidate @submit.prevent="submit">
      <Message v-if="submitError" severity="error" :closable="false" role="alert">{{ submitError }}</Message>
      <div class="prospect-company-grid">
        <BaseInput id="razaoSocial" v-model="form.razaoSocial" label="Razão social *" name="razaoSocial" autocomplete="organization" maxlength="300" :error="fieldError('razaoSocial')" class="company-name-field" />
        <BaseInput id="cnpj" :model-value="form.cnpj" label="CNPJ" name="cnpj" inputmode="numeric" autocomplete="off" placeholder="00.000.000/0000-00" maxlength="18" :error="fieldError('cnpj')" @update:model-value="form.cnpj = formatarCnpj($event)" />
        <div class="field"><label for="regimeTributario">Regime tributário</label><Select input-id="regimeTributario" v-model="form.regimeTributario" :options="[...regimeTributarioOptions]" option-label="label" option-value="value" placeholder="Selecione" show-clear /></div>
      </div>
      <section class="form-section" aria-labelledby="company-details-heading">
        <div class="form-section-heading"><div><h3 id="company-details-heading">Dados da empresa</h3></div></div>
        <div class="prospect-company-grid">
          <BaseInput id="nomeFantasia" v-model="form.nomeFantasia" label="Nome fantasia" name="nomeFantasia" maxlength="300" />
          <BaseInput id="tipoEmpresa" v-model="form.tipoEmpresa" label="Tipo de empresa" name="tipoEmpresa" maxlength="100" placeholder="Ex.: Sociedade limitada" />
          <BaseInput id="inscricaoEstadual" v-model="form.inscricaoEstadual" label="Inscricao estadual" name="inscricaoEstadual" maxlength="30" />
          <BaseInput id="inscricaoMunicipal" v-model="form.inscricaoMunicipal" label="Inscricao municipal" name="inscricaoMunicipal" maxlength="30" />
          <BaseInput id="porteEmpresa" v-model="form.porteEmpresa" label="Porte da empresa" name="porteEmpresa" maxlength="100" />
          <BaseInput id="cnaePrincipal" v-model="form.cnaePrincipal" label="CNAE principal" name="cnaePrincipal" inputmode="numeric" maxlength="7" :error="fieldError('cnaePrincipal')" />
          <BaseInput id="dataAbertura" v-model="form.dataAbertura" label="Data de abertura" name="dataAbertura" type="date" />
          <BaseInput id="origem" v-model="form.origem" label="Origem" name="origem" maxlength="100" />
        </div>
      </section>

      <section class="form-section" aria-labelledby="contact-heading">
        <div class="form-section-heading"><div><h3 id="contact-heading">Contato</h3></div></div>
        <div class="prospect-company-grid">
          <BaseInput id="nomeContato" v-model="form.nomeContato" label="Nome do contato" name="nomeContato" maxlength="250" />
          <BaseInput id="email" v-model="form.email" label="E-mail" name="email" type="email" autocomplete="email" maxlength="250" />
          <BaseInput id="telefone" v-model="form.telefone" label="Telefone" name="telefone" inputmode="tel" autocomplete="tel" maxlength="30" />
          <BaseInput id="whatsapp" v-model="form.whatsapp" label="WhatsApp" name="whatsapp" inputmode="tel" maxlength="30" />
        </div>
      </section>

      <section class="form-section" aria-labelledby="address-heading">
        <div class="form-section-heading"><div><h3 id="address-heading">Endereco</h3></div></div>
        <Message v-if="enderecoAviso" :severity="enderecoAvisoSeveridade" :closable="false" aria-live="polite">{{ enderecoAviso }}</Message>
        <div class="prospect-address-grid">
          <BaseInput id="cep" :model-value="form.cep" label="CEP" name="cep" inputmode="numeric" autocomplete="postal-code" placeholder="00000-000" maxlength="9" :error="fieldError('cep')" class="address-cep" @update:model-value="form.cep = formatarCep($event)" />
          <span v-if="cepLoading" class="address-loading" role="status">Consultando CEP...</span>
          <BaseInput id="logradouro" v-model="form.logradouro" label="Logradouro" name="logradouro" maxlength="250" class="address-street" />
          <BaseInput id="numero" v-model="form.numero" label="Numero" name="numero" maxlength="30" />
          <BaseInput id="complemento" v-model="form.complemento" label="Complemento" name="complemento" maxlength="150" />
          <BaseInput id="bairro" v-model="form.bairro" label="Bairro" name="bairro" maxlength="150" />
          <div class="field">
            <label for="uf">UF</label>
            <Select input-id="uf" :model-value="form.uf || null" :options="estadoOptions" option-label="label" option-value="value" filter placeholder="Selecione" :loading="estadosLoading" :disabled="localidadeBloqueadaPeloCep" :invalid="!!fieldError('uf')" @update:model-value="alterarUf" />
            <small v-if="fieldError('uf')" class="field-error">{{ fieldError('uf') }}</small>
          </div>
          <div class="field address-city">
            <label for="municipioCodigoIbge">Municipio</label>
            <Select input-id="municipioCodigoIbge" :model-value="form.municipioCodigoIbge || null" :options="municipioOptions" option-label="label" option-value="value" filter placeholder="Selecione" :loading="municipiosLoading" :disabled="!form.uf || municipiosLoading || localidadeBloqueadaPeloCep" :invalid="!!fieldError('municipioCodigoIbge')" @update:model-value="alterarMunicipio" />
            <small v-if="fieldError('municipioCodigoIbge')" class="field-error">{{ fieldError('municipioCodigoIbge') }}</small>
          </div>
        </div>
      </section>

      <section class="partners-section" aria-labelledby="partners-heading">
        <div class="form-section-heading"><div><h3 id="partners-heading">Sócios</h3><p>Participação informada: {{ participacaoTotal }}%</p></div><BaseButton label="Adicionar sócio" icon="pi pi-plus" variant="secondary" @click="addSocio" /></div>
        <Message v-if="fieldError('socios')" severity="error" :closable="false">{{ fieldError('socios') }}</Message>
        <div v-if="form.socios.length" class="partners-list">
          <fieldset v-for="(socio, index) in form.socios" :key="index" class="partner-row">
            <legend>Sócio {{ index + 1 }}</legend>
            <BaseInput :id="`socios-${index}-nome`" v-model="socio.nome" label="Nome *" :error="fieldError(`socios.${index}.nome`)" />
            <BaseInput :id="`socios-${index}-cpf`" :model-value="socio.cpf" label="CPF" inputmode="numeric" placeholder="000.000.000-00" maxlength="14" :error="fieldError(`socios.${index}.cpf`)" @update:model-value="socio.cpf = formatarCpf($event)" />
            <div class="field"><label :for="`socios-${index}-participacao`">Participação (%)</label><InputNumber :input-id="`socios-${index}-participacao`" v-model="socio.participacao" :min="0" :max="100" :max-fraction-digits="2" suffix="%" :invalid="!!fieldError(`socios.${index}.participacao`)" /><small v-if="fieldError(`socios.${index}.participacao`)" class="field-error">{{ fieldError(`socios.${index}.participacao`) }}</small></div>
            <BaseButton icon="pi pi-trash" variant="text" :aria-label="`Remover sócio ${index + 1}`" class="remove-partner" @click="removeSocio(index)" />
          </fieldset>
        </div>
        <p v-else class="empty-partners">Nenhum sócio adicionado.</p>
      </section>
    </form>
    <template #footer><BaseButton label="Cancelar" variant="text" :disabled="loading" @click="close" /><BaseButton label="Cadastrar prospecto" icon="pi pi-check" type="submit" form="prospect-form" :loading="loading" :disabled="loading || cepLoading || cepStatus === 'pendente' || cepStatus === 'invalido'" /></template>
  </Dialog>
</template>

<style scoped>
.dialog-description { margin: 0 0 20px; color: var(--officio-muted); font-size: 14px; }
.prospect-form { display: grid; gap: 24px; }
.prospect-company-grid { display: grid; grid-template-columns: repeat(2, minmax(0, 1fr)); gap: 16px; }
.company-name-field { grid-column: 1 / -1; }
.form-section { display: grid; gap: 16px; padding-top: 20px; border-top: 1px solid var(--officio-border); }
.form-section-heading h3 { margin: 0; font-size: 16px; }
.prospect-address-grid { display: grid; grid-template-columns: minmax(140px, .7fr) minmax(180px, 1fr) minmax(180px, 1fr); gap: 16px; align-items: end; }
.address-cep { max-width: 180px; }
.address-loading { align-self: center; color: var(--officio-muted); font-size: 13px; }
.address-street { grid-column: span 2; }
.address-city { grid-column: span 2; }
.field-error { color: var(--p-red-600); font-size: 12px; }
.partners-section { display: grid; gap: 16px; padding-top: 20px; border-top: 1px solid var(--officio-border); }
.partners-list { display: grid; gap: 12px; }
.partner-row { display: grid; grid-template-columns: minmax(160px, 1.4fr) minmax(150px, 1fr) 140px 40px; gap: 12px; align-items: end; margin: 0; padding: 14px; border: 1px solid var(--officio-border); border-radius: 6px; }
.partner-row legend { padding: 0 4px; font-size: 12px; font-weight: 600; }
.partner-row :deep(.p-inputnumber), .partner-row :deep(.p-inputnumber-input) { width: 100%; }
.remove-partner { width: 40px; }
.empty-partners { margin: 0; color: var(--officio-muted); font-size: 13px; }
@media (max-width: 680px) { .prospect-company-grid, .prospect-address-grid, .partner-row { grid-template-columns: 1fr; } .company-name-field, .address-street { grid-column: auto; } .remove-partner { justify-self: end; } }
</style>
