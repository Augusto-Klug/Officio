<script setup lang="ts">
import { computed, ref } from 'vue'
import { useRoute } from 'vue-router'
import Message from 'primevue/message'
import BaseButton from '../../components/base/BaseButton.vue'
import BaseCard from '../../components/base/BaseCard.vue'
import BaseInput from '../../components/base/BaseInput.vue'
import { redefinirSenha } from '../../api/auth.service'
import { normalizeError } from '../../api/errors'

const route = useRoute()
const token = computed(() => typeof route.query.token === 'string' ? route.query.token : '')
const senha = ref('')
const confirmarSenha = ref('')
const loading = ref(false)
const error = ref('')
const completed = ref(false)

async function submit() {
  if (loading.value) return
  if (senha.value !== confirmarSenha.value) { error.value = 'As senhas não coincidem.'; return }
  loading.value = true
  error.value = ''
  try { await redefinirSenha(token.value, senha.value); completed.value = true }
  catch (cause) { error.value = normalizeError(cause).message }
  finally { loading.value = false }
}
</script>

<template>
  <main class="login-page"><div class="login-content">
    <div class="login-brand"><img src="/favicon.svg" width="40" height="40" alt="" /><span translate="no">officio.</span></div>
    <BaseCard>
      <template v-if="completed">
        <div class="auth-result-icon"><i class="pi pi-lock-open" aria-hidden="true" /></div><h1>Senha redefinida</h1>
        <p class="login-description">Use sua nova senha no próximo acesso.</p><RouterLink class="auth-primary-link" to="/login">Entrar no Officio</RouterLink>
      </template>
      <template v-else-if="!token">
        <Message severity="error" :closable="false">Este link está incompleto. Solicite uma nova redefinição de senha.</Message>
        <RouterLink class="auth-link" to="/esqueci-senha">Solicitar novo link</RouterLink>
      </template>
      <template v-else>
        <h1>Defina uma nova senha</h1><p class="login-description">A nova senha substituirá a credencial anterior.</p>
        <Message v-if="error" severity="error" :closable="false" role="alert">{{ error }}</Message>
        <form class="login-form" @submit.prevent="submit" :aria-busy="loading">
          <BaseInput id="reset-password" v-model="senha" label="Nova senha" type="password" autocomplete="new-password" hint="Use pelo menos 8 caracteres." required :disabled="loading" />
          <BaseInput id="reset-confirm" v-model="confirmarSenha" label="Confirme a nova senha" type="password" autocomplete="new-password" required :disabled="loading" />
          <BaseButton type="submit" label="Redefinir senha" icon="pi pi-check" :loading="loading" :disabled="loading" />
        </form>
      </template>
    </BaseCard>
  </div></main>
</template>
