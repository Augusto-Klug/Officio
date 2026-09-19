<script setup lang="ts">
import Select from 'primevue/select'
defineOptions({ inheritAttrs: false })
defineProps<{ id: string; label: string; options: ReadonlyArray<{ label: string; value: string }>; placeholder?: string }>()
const model = defineModel<string | null>({ default: null })
</script>

<template>
  <div class="field">
    <label :for="id">{{ label }}</label>
    <Select :input-id="id" v-model="model" :options="[...options]" option-label="label" option-value="value"
      :placeholder="placeholder" show-clear v-bind="$attrs" />
  </div>
</template>
