<script setup lang="ts">
import Button from 'primevue/button'
withDefaults(defineProps<{ label?: string; variant?: 'primary' | 'secondary' | 'text'; loading?: boolean; disabled?: boolean; type?: 'button' | 'submit' | 'reset'; icon?: string }>(), { variant: 'primary', type: 'button' })
</script>

<template>
  <Button :label="label" :type="type" :loading="loading" :disabled="disabled" :icon="icon"
    :severity="variant === 'primary' ? undefined : 'secondary'" :text="variant === 'text'" class="base-button" />
</template>
