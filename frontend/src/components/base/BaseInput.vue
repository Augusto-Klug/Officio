<script setup lang="ts">
import InputText from 'primevue/inputtext'
defineOptions({ inheritAttrs: false })
defineProps<{ id: string; label: string; error?: string; hint?: string }>()
const model = defineModel<string>({ default: '' })
</script>

<template>
  <div class="field">
    <label :for="id">{{ label }}</label>
    <InputText :id="id" v-model="model" v-bind="$attrs" :invalid="!!error" :aria-invalid="!!error"
      :aria-describedby="error || hint ? `${id}-help` : undefined" />
    <small v-if="error || hint" :id="`${id}-help`" :class="{ 'field-error': error }">{{ error || hint }}</small>
  </div>
</template>
