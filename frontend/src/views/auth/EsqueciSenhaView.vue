<script setup lang="ts">
import { ref } from 'vue'
import Message from 'primevue/message'
import BaseButton from '../../components/base/BaseButton.vue'
import BaseCard from '../../components/base/BaseCard.vue'
import BaseInput from '../../components/base/BaseInput.vue'
import { solicitarRedefinicaoSenha } from '../../api/auth.service'
import { normalizeError } from '../../api/errors'

const email = ref('')
const loading = ref(false)
const error = ref('')
const sent = ref(false)

async function submit() {
  if (loading.value) return
  loading.value = true
  error.value = ''
  try { await solicitarRedefinicaoSenha(email.value); sent.value = true }
  catch (cause) { error.value = normalizeError(cause).message }
  finally { loading.value = false }
}
</script>

<template>
  <main class="login-page">
    <div class="login-content">
      <RouterLink to="/login" class="login-brand" aria-label="Voltar para o login"><img src="/favicon.svg" width="40" height="40" alt="" /><span translate="no">officio.</span></RouterLink>
      <BaseCard>
        <template v-if="sent">
          <div class="auth-result-icon"><i class="pi pi-envelope" aria-hidden="true" /></div>
          <h1>Confira seu e-mail</h1>
          <p class="login-description">Se houver uma conta para <strong>{{ email.trim() }}</strong>, enviaremos um link para redefinir a senha.</p>
          <RouterLink class="auth-link" to="/login">Voltar para entrar</RouterLink>
        </template>
        <template v-else>
          <h1>Recupere sua senha</h1>
          <p class="login-description">Informe o e-mail usado no Officio para receber as instruções.</p>
          <Message v-if="error" severity="error" :closable="false" role="alert">{{ error }}</Message>
          <form class="login-form" @submit.prevent="submit" :aria-busy="loading">
            <BaseInput id="recovery-email" v-model="email" label="E-mail" type="email" autocomplete="email" required :disabled="loading" />
            <BaseButton type="submit" label="Enviar instruções" icon="pi pi-send" :loading="loading" :disabled="loading" />
            <RouterLink class="auth-link" to="/login">Voltar para entrar</RouterLink>
          </form>
        </template>
      </BaseCard>
    </div>
  </main>
</template>
