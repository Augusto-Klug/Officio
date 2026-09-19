<script setup lang="ts">
import { computed, nextTick, reactive, ref } from 'vue'
import Dialog from 'primevue/dialog'
import InputNumber from 'primevue/inputnumber'
import Message from 'primevue/message'
import Select from 'primevue/select'
import BaseButton from '../../../components/base/BaseButton.vue'
import BaseInput from '../../../components/base/BaseInput.vue'
import { normalizeError } from '../../../api/errors'
import { criarProspecto } from '../services/prospectos.service'
import { regimeTributarioOptions, type ProspectoCriado, type RegimeTributario } from '../types/prospecto'
import { formatarCnpj, formatarCpf, limparPayload, novoProspecto, validarProspecto, type ProspectoFormErrors } from './prospecto-form'

const props = defineProps<{ visible: boolean }>()
const emit = defineEmits<{
  'update:visible': [value: boolean]
  criado: [prospecto: ProspectoCriado]
}>()
const dialogVisible = computed({ get: () => props.visible, set: (value) => emit('update:visible', value) })
const form = reactive({ razaoSocial: '', cnpj: '', regimeTributario: null as RegimeTributario | null, socios: [] as Array<{ nome: string; cpf: string; participacao: number }> })
const errors = ref<ProspectoFormErrors>({})
const submitError = ref('')
const loading = ref(false)
const participacaoTotal = computed(() => form.socios.reduce((total, socio) => total + (socio.participacao || 0), 0))

function reset() {
  form.razaoSocial = ''
  form.cnpj = ''
  form.regimeTributario = null
  form.socios = []
  errors.value = {}
  submitError.value = ''
}

function close() { if (!loading.value) dialogVisible.value = false }
function addSocio() { form.socios.push({ nome: '', cpf: '', participacao: 0 }) }
function removeSocio(index: number) { form.socios.splice(index, 1); errors.value = {} }
function fieldError(name: string) { return errors.value[name] }

async function focusFirstInvalid() {
  await nextTick()
  const first = Object.keys(errors.value)[0]
  if (first) document.getElementById(first.replaceAll('.', '-'))?.focus()
}

async function submit() {
  submitError.value = ''
  const payload = limparPayload({
    ...novoProspecto(),
    razaoSocial: form.razaoSocial,
    cnpj: form.cnpj,
    regimeTributario: form.regimeTributario,
    socios: form.socios.map((socio) => ({ ...socio, cpf: socio.cpf })),
  })
  errors.value = validarProspecto(payload)
  if (Object.keys(errors.value).length) { await focusFirstInvalid(); return }
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
    :draggable="false" aria-describedby="prospect-dialog-description" @show="errors = {}; submitError = ''">
    <p id="prospect-dialog-description" class="dialog-description">Cadastre os dados comerciais usados na proposta e na futura conversão em cliente.</p>
    <form id="prospect-form" class="prospect-form" novalidate @submit.prevent="submit">
      <Message v-if="submitError" severity="error" :closable="false" role="alert">{{ submitError }}</Message>
      <div class="prospect-company-grid">
        <BaseInput id="razaoSocial" v-model="form.razaoSocial" label="Razão social *" name="razaoSocial" autocomplete="organization" maxlength="300" :error="fieldError('razaoSocial')" class="company-name-field" />
        <BaseInput id="cnpj" :model-value="form.cnpj" label="CNPJ" name="cnpj" inputmode="numeric" autocomplete="off" placeholder="00.000.000/0000-00" maxlength="18" :error="fieldError('cnpj')" @update:model-value="form.cnpj = formatarCnpj($event)" />
        <div class="field"><label for="regimeTributario">Regime tributário</label><Select input-id="regimeTributario" v-model="form.regimeTributario" :options="[...regimeTributarioOptions]" option-label="label" option-value="value" placeholder="Selecione" show-clear /></div>
      </div>
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
    <template #footer><BaseButton label="Cancelar" variant="text" :disabled="loading" @click="close" /><BaseButton label="Cadastrar prospecto" icon="pi pi-check" type="submit" form="prospect-form" :loading="loading" /></template>
  </Dialog>
</template>

<style scoped>
.dialog-description { margin: 0 0 20px; color: var(--officio-muted); font-size: 14px; }
.prospect-form { display: grid; gap: 24px; }
.prospect-company-grid { display: grid; grid-template-columns: repeat(2, minmax(0, 1fr)); gap: 16px; }
.company-name-field { grid-column: 1 / -1; }
.partners-section { display: grid; gap: 16px; padding-top: 20px; border-top: 1px solid var(--officio-border); }
.partners-list { display: grid; gap: 12px; }
.partner-row { display: grid; grid-template-columns: minmax(160px, 1.4fr) minmax(150px, 1fr) 140px 40px; gap: 12px; align-items: end; margin: 0; padding: 14px; border: 1px solid var(--officio-border); border-radius: 6px; }
.partner-row legend { padding: 0 4px; font-size: 12px; font-weight: 600; }
.partner-row :deep(.p-inputnumber), .partner-row :deep(.p-inputnumber-input) { width: 100%; }
.remove-partner { width: 40px; }
.empty-partners { margin: 0; color: var(--officio-muted); font-size: 13px; }
@media (max-width: 680px) { .prospect-company-grid, .partner-row { grid-template-columns: 1fr; } .company-name-field { grid-column: auto; } .remove-partner { justify-self: end; } }
</style>
