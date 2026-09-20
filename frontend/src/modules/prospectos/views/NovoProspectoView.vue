<script setup lang="ts">
import { nextTick, reactive, ref } from 'vue'
import { useRouter } from 'vue-router'
import Message from 'primevue/message'
import BaseInput from '../../../components/base/BaseInput.vue'
import BaseSelect from '../../../components/base/BaseSelect.vue'
import BaseButton from '../../../components/base/BaseButton.vue'
import BaseCard from '../../../components/base/BaseCard.vue'
import PageHeader from '../../../components/layout/PageHeader.vue'
import { normalizeError } from '../../../api/errors'
import { criarProspecto } from '../services/prospectos.service'
import { confirmarCadastro } from '../state/confirmacao'
import { payloadProspecto, regimesTributarios, validarProspecto, type ProspectoFormulario } from '../types/criar-prospecto'

const router = useRouter()
const form = reactive<ProspectoFormulario>({ razaoSocial: '', cnpj: '', regimeTributario: null, socios: [] })
const erros = ref<Record<string, string>>({})
const mensagem = ref('')
const loading = ref(false)
const salvo = ref(false)
const formulario = ref<HTMLFormElement>()
const alerta = ref<HTMLDivElement>()
const adicionar = ref<HTMLDivElement>()
let proximoId = 0

async function adicionarSocio() {
  if (loading.value || salvo.value) return
  const id = proximoId++
  form.socios.push({ id, nome: '', cpf: '', participacao: '' })
  await nextTick()
  formulario.value?.querySelector<HTMLInputElement>(`#socio-${id}-nome`)?.focus()
}
async function removerSocio(id: number) {
  if (loading.value || salvo.value) return
  form.socios = form.socios.filter(socio => socio.id !== id)
  for (const campo of ['nome', 'cpf', 'participacao']) delete erros.value[`socio-${id}-${campo}`]
  await nextTick()
  adicionar.value?.querySelector<HTMLButtonElement>('button')?.focus()
}
async function voltar() {
  await router.replace({ name: 'prospectos' })
}
async function submit() {
  if (loading.value || salvo.value) return
  mensagem.value = ''
  erros.value = validarProspecto(form)
  if (Object.keys(erros.value).length) {
    mensagem.value = 'Revise os campos indicados antes de cadastrar.'
    await nextTick()
    formulario.value?.querySelector<HTMLElement>('[aria-invalid="true"]')?.focus()
    return
  }
  loading.value = true
  try {
    await criarProspecto(payloadProspecto(form))
    salvo.value = true
    confirmarCadastro()
  } catch (cause) {
    mensagem.value = normalizeError(cause).message
  } finally {
    loading.value = false
  }
  if (salvo.value) {
    try { await voltar() } catch { /* O sucesso permanece visível e impede um novo POST. */ }
  } else {
    await nextTick()
    alerta.value?.focus()
  }
}
</script>

<template>
  <PageHeader title="Novo prospecto" description="Cadastre uma oportunidade comercial para o seu escritório.">
    <BaseButton label="Voltar para prospectos" icon="pi pi-arrow-left" variant="text" :disabled="loading" @click="voltar" />
  </PageHeader>

  <BaseCard class="prospect-form-card">
    <form ref="formulario" novalidate :aria-busy="loading" @submit.prevent="submit">
      <div v-if="mensagem" ref="alerta" tabindex="-1" class="form-message">
        <Message severity="error" :closable="false" role="alert">{{ mensagem }}</Message>
      </div>
      <Message v-if="salvo" severity="success" :closable="false" role="status" class="form-message">Prospecto cadastrado com sucesso. <RouterLink to="/prospectos">Ir para a listagem</RouterLink></Message>
      <p class="form-intro">Campos marcados com * são obrigatórios.</p>
      <fieldset :disabled="loading || salvo" class="form-section">
        <legend>Dados do prospecto</legend>
        <div class="form-grid">
          <BaseInput id="razaoSocial" v-model="form.razaoSocial" class="full-width" name="razaoSocial" label="Razão social *" required maxlength="300" autocomplete="organization" :error="erros.razaoSocial" />
          <BaseInput id="cnpj" v-model="form.cnpj" name="cnpj" label="CNPJ (opcional)" inputmode="numeric" maxlength="18" placeholder="00.000.000/0000-00" hint="Informe com ou sem pontuação." :error="erros.cnpj" />
          <div>
            <BaseSelect id="regimeTributario" v-model="form.regimeTributario" label="Regime tributário (opcional)" :options="regimesTributarios" placeholder="Não informado" :disabled="loading || salvo" :aria-invalid="!!erros.regimeTributario" :aria-describedby="erros.regimeTributario ? 'regime-erro' : undefined" />
            <small v-if="erros.regimeTributario" id="regime-erro" class="field-error">{{ erros.regimeTributario }}</small>
          </div>
        </div>
      </fieldset>

      <section class="partners-section" aria-labelledby="socios-titulo">
        <h2 id="socios-titulo">Sócios</h2>
        <p class="section-description">Opcional. Adicione os sócios que deseja informar neste cadastro.</p>
        <p v-if="!form.socios.length" class="partners-empty">Nenhum sócio adicionado.</p>
        <fieldset v-for="(socio, index) in form.socios" :key="socio.id" :disabled="loading || salvo" class="partner">
          <legend>Sócio {{ index + 1 }}</legend>
          <div class="partner-fields">
            <BaseInput :id="`socio-${socio.id}-nome`" v-model="socio.nome" label="Nome *" required autocomplete="off" :error="erros[`socio-${socio.id}-nome`]" />
            <BaseInput :id="`socio-${socio.id}-cpf`" v-model="socio.cpf" label="CPF (opcional)" inputmode="numeric" maxlength="14" placeholder="000.000.000-00" :error="erros[`socio-${socio.id}-cpf`]" />
            <BaseInput :id="`socio-${socio.id}-participacao`" v-model="socio.participacao" label="Participação (%) *" inputmode="decimal" required placeholder="Ex.: 50" :error="erros[`socio-${socio.id}-participacao`]" />
          </div>
          <BaseButton label="Remover sócio" :aria-label="`Remover sócio ${index + 1}`" icon="pi pi-trash" variant="text" :disabled="loading || salvo" @click="removerSocio(socio.id)" />
        </fieldset>
        <div ref="adicionar"><BaseButton label="Adicionar sócio" icon="pi pi-plus" variant="secondary" :disabled="loading || salvo" @click="adicionarSocio" /></div>
      </section>

      <div class="form-actions">
        <BaseButton label="Cancelar" variant="secondary" :disabled="loading" @click="voltar" />
        <BaseButton type="submit" :label="loading ? 'Cadastrando…' : 'Cadastrar prospecto'" :loading="loading" :disabled="loading || salvo" />
      </div>
    </form>
  </BaseCard>
</template>

<style scoped>
.prospect-form-card { max-width: 960px; }
.form-intro, .section-description, .partners-empty { color: var(--officio-muted); font-size: 14px; line-height: 1.6; }
.form-intro, .form-message { margin-bottom: var(--space-6); }
.form-section, .partner { border: 0; padding: 0; margin: 0; min-width: 0; }
legend { font-size: 16px; font-weight: 650; padding: 0; margin-bottom: var(--space-6); }
.form-grid { display: grid; grid-template-columns: repeat(2, minmax(0, 1fr)); gap: var(--space-6); }
.full-width { grid-column: 1 / -1; }
.partners-section { border-top: 1px solid var(--officio-border); margin-top: var(--space-8); padding-top: var(--space-8); }
.section-description { margin: var(--space-2) 0 var(--space-6); }
.partner { border-bottom: 1px solid var(--officio-border); padding-bottom: var(--space-6); margin-bottom: var(--space-6); }
.partner legend { font-size: 14px; margin-bottom: var(--space-4); }
.partner-fields { display: grid; grid-template-columns: 2fr 1.4fr 1fr; gap: var(--space-4); margin-bottom: var(--space-2); }
.form-actions { display: flex; justify-content: flex-end; gap: var(--space-3); border-top: 1px solid var(--officio-border); margin-top: var(--space-8); padding-top: var(--space-6); }
@media (max-width: 760px) {
  .form-grid, .partner-fields { grid-template-columns: minmax(0, 1fr); }
  .form-actions { flex-wrap: wrap; }
  .form-actions > * { flex: 1; }
}
</style>
