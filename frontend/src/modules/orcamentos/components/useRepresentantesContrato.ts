import { computed, onScopeDispose, ref, type Ref } from 'vue'
import { normalizeError } from '../../../api/errors'
import { obterRepresentantesContrato } from '../services/orcamentos.service'
import { representanteValido, type RepresentantesContrato } from './representante'

export function useRepresentantesContrato(orcamentoId: () => string, conversaoAberta: Ref<boolean>) {
  const dados = ref<RepresentantesContrato>({ prospectoId: '', socios: [] })
  const selecionadoId = ref<string | null>(null)
  const carregando = ref(false)
  const erro = ref('')
  const cadastroAberto = ref(false)
  let controller: AbortController | undefined
  const pronto = computed(() => !carregando.value && !erro.value && representanteValido(dados.value.socios, selecionadoId.value))
  async function carregar() {
    controller?.abort()
    const atual = new AbortController(); controller = atual
    selecionadoId.value = null; carregando.value = true; erro.value = ''
    try {
      const resposta = await obterRepresentantesContrato(orcamentoId(), atual.signal)
      if (!atual.signal.aborted) dados.value = resposta
    } catch (cause) { if (!atual.signal.aborted) erro.value = normalizeError(cause).message }
    finally { if (controller === atual) carregando.value = false }
  }
  function editar() {
    if (!dados.value.prospectoId) return
    conversaoAberta.value = false; cadastroAberto.value = true
  }
  async function retornar(visible: boolean) {
    cadastroAberto.value = visible
    if (!visible) { conversaoAberta.value = true; await carregar() }
  }
  onScopeDispose(() => controller?.abort())
  return { dados, selecionadoId, carregando, erro, cadastroAberto, pronto, carregar, editar, retornar }
}
