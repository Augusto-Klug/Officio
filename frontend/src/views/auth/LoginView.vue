<script setup lang="ts">
import { ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import Message from 'primevue/message'
import BaseCard from '../../components/base/BaseCard.vue'
import BaseInput from '../../components/base/BaseInput.vue'
import BaseButton from '../../components/base/BaseButton.vue'
import { login } from '../../api/auth.service'
import { setSession } from '../../stores/session'
import { safeRedirect } from '../../router'
import { normalizeError } from '../../api/errors'
const route = useRoute()
const router = useRouter()
const email = ref('')
const senha = ref('')
const loading = ref(false)
const error = ref('')
async function submit() {
  if (loading.value) return
  loading.value = true
  error.value = ''
  try {
    setSession(await login(email.value.trim(), senha.value))
    senha.value = ''
    await router.replace(safeRedirect(route.query.redirect))
  } catch (cause) { error.value = normalizeError(cause).message }
  finally { loading.value = false }
}
</script>
<template>
  <main class="login-page">
    <div class="login-content">
      <div class="login-brand"><img src="/favicon.svg" width="40" height="40" alt="" /><span translate="no">officio.</span></div>
      <BaseCard>
        <h1>Entre no seu escritório</h1><p class="login-description">Acesse o Officio com suas credenciais.</p>
        <Message v-if="route.query.expired && !error" severity="warn" :closable="false">Sua sessão expirou. Entre novamente.</Message>
        <Message v-if="error" severity="error" :closable="false" role="alert">{{ error }}</Message>
        <form class="login-form" @submit.prevent="submit" :aria-busy="loading">
          <BaseInput id="email" v-model="email" label="E-mail" type="email" name="email" autocomplete="username" :spellcheck="false" placeholder="voce@escritorio.com.br…" required :disabled="loading" />
          <BaseInput id="senha" v-model="senha" label="Senha" type="password" name="senha" autocomplete="current-password" required :disabled="loading" />
          <BaseButton type="submit" :label="loading ? 'Entrando…' : 'Entrar'" :loading="loading" :disabled="loading" />
          <RouterLink class="auth-link" to="/esqueci-senha">Esqueci minha senha</RouterLink>
        </form>
      </BaseCard>
      <p class="login-footer">Officio · Gestão contábil</p>
    </div>
  </main>
</template>
