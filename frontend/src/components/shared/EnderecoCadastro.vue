<script setup lang="ts">
import { computed, onBeforeUnmount, onMounted, ref } from 'vue'
import Select from 'primevue/select'
import Message from 'primevue/message'
import BaseInput from '../base/BaseInput.vue'
import BaseButton from '../base/BaseButton.vue'
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
  model.value.cep = formatarCep(value)
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
      if (!current.signal.aborted) aviso.value = 'Endereço localizado. Confira os dados e informe o número e o complemento, se houver.'
    } else if ((model.value.municipioCodigoIbge && model.value.municipioCodigoIbge !== e.municipioCodigoIbge) || (model.value.uf && model.value.uf !== e.uf)) {
      erroCep.value = 'A cidade selecionada não corresponde ao CEP.'; return false
    }
    return !current.signal.aborted
  } catch (cause) {
    if (current.signal.aborted) return false
    const erro = normalizeError(cause)
    if (erro.status === 404) { erroCep.value = 'CEP inválido.'; return false }
    aviso.value = 'Consulta de CEP indisponível. Complete o endereço manualmente.'
    return true
  } finally { if (cepController === current) consultando.value = false }
}
defineExpose({ validar: () => buscarCep(false) })
onMounted(async () => {
  try { estados.value = await listarEstados(controller.signal); if (!controller.signal.aborted) await carregarMunicipios(model.value.uf) }
  catch (cause) { if (!controller.signal.aborted) aviso.value = normalizeError(cause).message }
})
onBeforeUnmount(() => { controller.abort(); cepController?.abort(); municipioController?.abort() })
</script>

<template>
  <div class="address-fields">
    <div class="form-grid two-columns">
      <BaseInput :id="`${props.prefix}-cep`" :model-value="formatarCep(model.cep)" label="CEP" inputmode="numeric" maxlength="9" autocomplete="postal-code" :error="erroCep" :disabled="disabled" @update:model-value="alterarCep" />
      <div class="address-lookup"><BaseButton label="Buscar CEP" icon="pi pi-search" variant="secondary" :loading="consultando" :disabled="disabled || !model.cep" @click="buscarCep()" /></div>
    </div>
    <Message v-if="aviso" severity="secondary" :closable="false">{{ aviso }}</Message>
    <BaseInput :id="`${props.prefix}-logradouro`" v-model="model.logradouro" label="Logradouro" maxlength="250" autocomplete="address-line1" :disabled="disabled" />
    <div class="form-grid two-columns">
      <BaseInput :id="`${props.prefix}-numero`" v-model="model.numero" label="Número" maxlength="30" :disabled="disabled" />
      <BaseInput :id="`${props.prefix}-complemento`" v-model="model.complemento" label="Complemento" maxlength="150" autocomplete="address-line2" :disabled="disabled" />
    </div>
    <BaseInput :id="`${props.prefix}-bairro`" v-model="model.bairro" label="Bairro" maxlength="150" :disabled="disabled" />
    <div class="form-grid two-columns">
      <div class="field"><label :for="`${props.prefix}-uf`">UF</label><Select :input-id="`${props.prefix}-uf`" :model-value="model.uf || null" :options="estadoOptions" option-label="label" option-value="value" filter placeholder="Selecione" :disabled="disabled || consultando" @update:model-value="alterarUf" /></div>
      <div class="field"><label :for="`${props.prefix}-cidade`">Cidade</label><Select :input-id="`${props.prefix}-cidade`" :model-value="model.municipioCodigoIbge || null" :options="municipioOptions" option-label="label" option-value="value" filter placeholder="Selecione" :loading="loading" :disabled="disabled || !model.uf || loading || consultando" @update:model-value="alterarMunicipio" /></div>
    </div>
  </div>
</template>

<style scoped>
.address-fields { display: grid; gap: 1rem; }
.address-lookup { display: flex; align-items: end; padding-bottom: 1px; }
</style>
