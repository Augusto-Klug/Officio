<script setup lang="ts">
import { computed, onBeforeUnmount, onMounted, ref, watch } from 'vue'
import Select from 'primevue/select'
import Message from 'primevue/message'
import BaseInput from '../base/BaseInput.vue'
import { consultarCep, listarEstados, listarMunicipios } from '../../modules/prospectos/services/localidades.service'
import type { EstadoLocalidade, MunicipioLocalidade } from '../../modules/prospectos/types/localidade'
import type { EnderecoCadastro } from './endereco'
import { formatarCep, somenteDigitos } from '../../utils/inputMasks'
import { normalizeError } from '../../api/errors'

const props = defineProps<{ prefix: string; disabled?: boolean }>()
const model = defineModel<EnderecoCadastro>({ required: true })
const estados = ref<EstadoLocalidade[]>([])
const municipios = ref<MunicipioLocalidade[]>([])
const loading = ref(false)
const consultando = ref(false)
const aviso = ref('')
const erroCep = ref('')
let cepController: AbortController | undefined
let municipioController: AbortController | undefined
let cepTimer: ReturnType<typeof setTimeout> | undefined
const cepConsultado = ref('')
const cepValido = ref(false)
const localidadeBloqueadaPeloCep = ref(false)
const controller = new AbortController()
const estadoOptions = computed(() => estados.value.map(e => ({ label: `${e.sigla} — ${e.nome}`, value: e.sigla })))
const municipioOptions = computed(() => municipios.value.map(m => ({ label: m.nome, value: m.codigoIbge })))

async function carregarMunicipios(uf: string) {
  municipioController?.abort()
  if (!uf) { municipios.value = []; return }
  const current = new AbortController(); municipioController = current; loading.value = true
  try { const items = await listarMunicipios(uf, current.signal); if (!current.signal.aborted) municipios.value = items }
  catch (cause) { if (!current.signal.aborted) aviso.value = normalizeError(cause).message }
  finally { if (municipioController === current) loading.value = false }
}
async function alterarUf(uf: string | null) {
  model.value.uf = uf || ''; model.value.cidade = ''; model.value.municipioCodigoIbge = ''; municipios.value = []
  await carregarMunicipios(model.value.uf)
}
function alterarMunicipio(codigo: string | null) {
  model.value.municipioCodigoIbge = codigo || ''
  model.value.cidade = municipios.value.find(m => m.codigoIbge === codigo)?.nome || ''
}
function alterarCep(value: string | undefined) {
  cepController?.abort(); consultando.value = false; erroCep.value = ''; aviso.value = ''
  cepValido.value = false; cepConsultado.value = ''; localidadeBloqueadaPeloCep.value = false
  model.value.cep = formatarCep(value)
}
function alterarNumero(value: string | undefined) {
  model.value.numero = somenteDigitos(value).slice(0, 30)
}
async function buscarCep(preencher = true) {
  cepController?.abort()
  const cep = somenteDigitos(model.value.cep)
  erroCep.value = ''; aviso.value = ''
  if (!cep) return true
  if (cep.length !== 8) { erroCep.value = 'Informe um CEP com 8 dígitos.'; return false }
  const current = new AbortController(); cepController = current; consultando.value = true
  try {
    const e = await consultarCep(cep, current.signal)
    if (current.signal.aborted) return false
    if (preencher) {
      if (e.logradouro) model.value.logradouro = e.logradouro
      if (e.bairro) model.value.bairro = e.bairro
      model.value.uf = e.uf; model.value.cidade = e.cidade; model.value.municipioCodigoIbge = e.municipioCodigoIbge
      await carregarMunicipios(e.uf)
      if (!current.signal.aborted) {
        cepValido.value = true; cepConsultado.value = cep; localidadeBloqueadaPeloCep.value = true
        aviso.value = 'Endereço localizado. Confira os dados e informe o número e o complemento, se houver.'
      }
    } else if ((model.value.municipioCodigoIbge && model.value.municipioCodigoIbge !== e.municipioCodigoIbge) || (model.value.uf && model.value.uf !== e.uf)) {
      erroCep.value = 'A cidade selecionada não corresponde ao CEP.'; return false
    }
    return !current.signal.aborted
  } catch (cause) {
    if (current.signal.aborted) return false
    const erro = normalizeError(cause)
    if (erro.status === 404) { cepValido.value = false; erroCep.value = 'CEP inválido.'; return false }
    cepValido.value = false
    aviso.value = 'Consulta de CEP indisponível. Complete o endereço manualmente.'
    return true
  } finally { if (cepController === current) consultando.value = false }
}
async function validar() {
  const cep = somenteDigitos(model.value.cep)
  if (!cep) return true
  if (cep.length !== 8) { erroCep.value = 'Informe um CEP com 8 dígitos.'; return false }
  if (cepValido.value && cepConsultado.value === cep) return true
  if (erroCep.value === 'CEP inválido.') return false
  return buscarCep(true)
}
defineExpose({ validar })
watch(() => somenteDigitos(model.value.cep), (cep) => {
  if (cepTimer) clearTimeout(cepTimer)
  cepController?.abort()
  consultando.value = false
  if (!cep) return
  if (cep.length === 8 && !props.disabled) cepTimer = setTimeout(() => void buscarCep(), 350)
}, { immediate: true })
onMounted(async () => {
  try { estados.value = await listarEstados(controller.signal); if (!controller.signal.aborted) await carregarMunicipios(model.value.uf) }
  catch (cause) { if (!controller.signal.aborted) aviso.value = normalizeError(cause).message }
})
onBeforeUnmount(() => { controller.abort(); cepController?.abort(); municipioController?.abort(); if (cepTimer) clearTimeout(cepTimer) })
</script>

<template>
  <div class="address-fields">
    <div class="form-grid two-columns">
      <BaseInput :id="`${props.prefix}-cep`" :model-value="formatarCep(model.cep)" label="CEP" numeric-only inputmode="numeric" maxlength="9" autocomplete="postal-code" :error="erroCep" :disabled="disabled" @update:model-value="alterarCep" />
      <span v-if="consultando" class="address-loading" role="status">Consultando CEP...</span>
    </div>
    <Message v-if="aviso" severity="secondary" :closable="false">{{ aviso }}</Message>
    <BaseInput :id="`${props.prefix}-logradouro`" v-model="model.logradouro" label="Logradouro" maxlength="250" autocomplete="address-line1" :disabled="disabled" />
    <div class="form-grid two-columns">
      <BaseInput :id="`${props.prefix}-numero`" :model-value="model.numero" label="Número" numeric-only inputmode="numeric" maxlength="30" :disabled="disabled" @update:model-value="alterarNumero" />
      <BaseInput :id="`${props.prefix}-complemento`" v-model="model.complemento" label="Complemento" maxlength="150" autocomplete="address-line2" :disabled="disabled" />
    </div>
    <BaseInput :id="`${props.prefix}-bairro`" v-model="model.bairro" label="Bairro" maxlength="150" :disabled="disabled" />
    <div class="form-grid two-columns">
      <div class="field"><label :for="`${props.prefix}-uf`">UF</label><Select :input-id="`${props.prefix}-uf`" :model-value="model.uf || null" :options="estadoOptions" option-label="label" option-value="value" filter placeholder="Selecione" :disabled="disabled || consultando || localidadeBloqueadaPeloCep" @update:model-value="alterarUf" /></div>
      <div class="field"><label :for="`${props.prefix}-cidade`">Cidade</label><Select :input-id="`${props.prefix}-cidade`" :model-value="model.municipioCodigoIbge || null" :options="municipioOptions" option-label="label" option-value="value" filter placeholder="Selecione" :loading="loading" :disabled="disabled || !model.uf || loading || consultando || localidadeBloqueadaPeloCep" @update:model-value="alterarMunicipio" /></div>
    </div>
  </div>
</template>

<style scoped>
.address-fields { display: grid; gap: 1rem; }
.address-loading { align-self: center; color: var(--officio-muted); font-size: 13px; }
</style>
