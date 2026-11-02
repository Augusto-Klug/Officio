<script setup lang="ts">
import InputText from 'primevue/inputtext'
defineOptions({ inheritAttrs: false })
const props = defineProps<{ id: string; label: string; error?: string; hint?: string; numericOnly?: boolean }>()
const model = defineModel<string>({ default: '' })

function bloquearEntradaNaoNumerica(event: InputEvent) {
  if (!props.numericOnly || event.inputType === 'insertFromPaste' || event.inputType === 'insertFromDrop') return
  if (event.data && /\D/.test(event.data)) event.preventDefault()
}

function filtrarEntradaNumerica(event: Event) {
  if (!props.numericOnly) return
  const input = event.target as HTMLInputElement
  const valor = input.value
  const posicao = input.selectionStart ?? valor.length
  const digitosAntesDoCursor = valor.slice(0, posicao).replace(/\D/g, '').length
  const digitos = valor.replace(/\D/g, '')

  if (valor !== digitos) {
    input.value = digitos
    input.setSelectionRange(digitosAntesDoCursor, digitosAntesDoCursor)
  }
  model.value = digitos
}
</script>

<template>
  <div class="field">
    <label :for="id">{{ label }}</label>
    <InputText :id="id" v-model="model" v-bind="$attrs" :invalid="!!error" :aria-invalid="!!error"
      :aria-describedby="error || hint ? `${id}-help` : undefined" @beforeinput="bloquearEntradaNaoNumerica" @input="filtrarEntradaNumerica" />
    <small v-if="error || hint" :id="`${id}-help`" :class="{ 'field-error': error }">{{ error || hint }}</small>
  </div>
</template>
