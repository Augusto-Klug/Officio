import { describe, expect, it, vi, afterEach } from 'vitest'
import { createSSRApp, effectScope, ref } from 'vue'
import { renderToString } from '@vue/server-renderer'
import PrimeVue from 'primevue/config'
import RepresentanteSelecao from './RepresentanteSelecao.vue'
import { useRepresentantesContrato } from './useRepresentantesContrato'
import { representanteValido, type RepresentanteDisponivel } from './representante'
import { http } from '../../../api/http'

const completo = (id = 's1'): RepresentanteDisponivel => ({ id, nome: `Pessoa ${id}`, cpf: '11789299900',
  tipoRelacao: 'Sócio', cadastroCompleto: true, informacoesFaltantes: [] })
const incompleto: RepresentanteDisponivel = { ...completo('s2'), nome: '', cadastroCompleto: false, informacoesFaltantes: ['Nome', 'Profissão'] }
afterEach(() => vi.restoreAllMocks())
async function render(socios: RepresentanteDisponivel[], id: string | null = null) {
  return renderToString(createSSRApp(RepresentanteSelecao, { socios, loading: false, error: '', modelValue: id }).use(PrimeVue))
}

describe('seleção do representante', () => {
  it('lista nome, CPF, tipo, completude e informações faltantes', async () => {
    const html = await render([completo(), incompleto])
    expect(html).toContain('Pessoa s1'); expect(html).toContain('117.892.999-00')
    expect(html).toContain('Sócio'); expect(html).toContain('Cadastro completo')
    expect(html).toContain('Cadastro incompleto'); expect(html).toContain('Ver pend'); expect(html).toContain('Nome'); expect(html).toContain('Adicionar sócio'); expect(html).not.toContain('Nome,')
    expect(html).toContain('Completar cadastro')
    expect(html).toMatch(/type="radio"[^>]*disabled/)
  })
  it('explica ausência de sócios e oferece cadastro sem selecionar automaticamente', async () => {
    const html = await render([])
    expect(html).toContain('Nenhum sócio cadastrado'); expect(html).toContain('Cadastrar sócio')
    expect(representanteValido([], null)).toBe(false)
  })
  it('utiliza grupo único de radios e mantém somente um marcado', async () => {
    const html = await render([completo(), completo('s3')], 's3')
    expect(html.match(/name="representante"/g)).toHaveLength(2)
    expect(html.match(/ checked(?:="[^"]*")?/g)).toHaveLength(1)
  })
  it('bloqueia incompleto, exige decisão explícita e retorna à edição do mesmo prospecto', async () => {
    const get = vi.spyOn(http, 'get').mockResolvedValue({ data: { prospectoId: 'p1', socios: [completo(), incompleto] } })
    const scope = effectScope(); const aberto = ref(true)
    const fluxo = scope.run(() => useRepresentantesContrato(() => 'o1', aberto))!
    await fluxo.carregar()
    expect(get).toHaveBeenCalledWith('/v1/orcamentos/ObterRepresentantes/o1', { signal: expect.any(AbortSignal) })
    expect(fluxo.pronto.value).toBe(false)
    fluxo.selecionadoId.value = 's2'; expect(fluxo.pronto.value).toBe(false)
    fluxo.selecionadoId.value = 's1'; expect(fluxo.pronto.value).toBe(true)
    fluxo.editar()
    expect(fluxo.cadastroAberto.value).toBe(true); expect(aberto.value).toBe(false)
    expect(fluxo.dados.value.prospectoId).toBe('p1')
    get.mockResolvedValue({ data: { prospectoId: 'p1', socios: [completo(), completo('s2')] } })
    await fluxo.retornar(false)
    expect(aberto.value).toBe(true); expect(fluxo.cadastroAberto.value).toBe(false)
    expect(fluxo.dados.value.socios[1]!.cadastroCompleto).toBe(true)
    expect(fluxo.selecionadoId.value).toBeNull()
    fluxo.selecionadoId.value = 's2'; expect(fluxo.pronto.value).toBe(true)
    scope.stop()
  })
  it('trata falha de carregamento sem liberar a conversão', async () => {
    vi.spyOn(http, 'get').mockRejectedValue(new Error('Falha ao carregar'))
    const scope = effectScope(); const fluxo = scope.run(() => useRepresentantesContrato(() => 'o1', ref(true)))!
    await fluxo.carregar(); expect(fluxo.erro.value).toBeTruthy(); expect(fluxo.pronto.value).toBe(false)
    scope.stop()
  })
})
