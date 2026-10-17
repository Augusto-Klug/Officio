import { afterEach, describe, expect, it, vi } from 'vitest'
import { http } from '../../../api/http'
import { aceitarOrcamento, atualizarOrcamento, converterOrcamento, criarOrcamento, listarCatalogoOrcamento, listarContratosAtivosParaConversao, normalizeCatalogoOrcamento, normalizeOrcamentos, obterOrcamento } from './orcamentos.service'

afterEach(() => vi.restoreAllMocks())

describe('contrato HTTP de orçamentos', () => {
  it('normalizes the service catalog and includes inactive items on request', async () => {
    const catalogItem = { id: 'catalog-1', descricao: 'Monthly accounting', secao: 'Honorarios', tipo: 'ItemMonetario', naturezaCobranca: 'HonorarioMensal', valorReferencia: 500, ativo: false }
    const get = vi.spyOn(http, 'get').mockResolvedValue({ data: [catalogItem] })

    expect(normalizeCatalogoOrcamento([catalogItem])).toEqual([catalogItem])
    await expect(listarCatalogoOrcamento(true)).resolves.toEqual([catalogItem])
    expect(get).toHaveBeenCalledWith('/v1/orcamentos/Catalogo', { params: { incluirInativos: true }, signal: undefined })
  })

  it('normaliza a lista e seus itens', () => {
    const result = normalizeOrcamentos([{
      id: 'orc-1', prospectoId: 'pro-1', titulo: 'Proposta mensal', status: 'Rascunho',
      valorTotal: 500, diaVencimento: 10, observacoes: null, motivoRecusa: null,
      itens: [{ id: 'item-1', itemCatalogoOrcamentoId: null, descricao: 'Honorários', secao: 'Honorarios', tipo: 'ItemMonetario', naturezaCobranca: 'HonorarioMensal', quantidade: 1, valorUnitario: 500, valorTotal: 500, ordem: 1 }],
    }])

    expect(result[0]).toMatchObject({ id: 'orc-1', valorTotal: 500, itens: [{ descricao: 'Honorários', naturezaCobranca: 'HonorarioMensal' }] })
  })

  it('rejeita resposta estruturalmente inválida', () => {
    expect(() => normalizeOrcamentos({ data: [] })).toThrow('formato inesperado')
    expect(() => normalizeOrcamentos([{ id: 'orc-1', itens: null }])).toThrow()
  })

  it('obtém um orçamento por id', async () => {
    const resposta = {
      id: 'orc-1', prospectoId: 'pro-1', titulo: 'Proposta mensal', status: 'Rascunho',
      valorTotal: 500, diaVencimento: 10, observacoes: null, motivoRecusa: null,
      itens: [{ id: 'item-1', itemCatalogoOrcamentoId: null, descricao: 'Honorários', secao: 'Honorarios', tipo: 'ItemMonetario', naturezaCobranca: 'HonorarioMensal', quantidade: 1, valorUnitario: 500, valorTotal: 500, ordem: 1 }],
    }
    const get = vi.spyOn(http, 'get').mockResolvedValue({ data: resposta })

    await expect(obterOrcamento('orc-1')).resolves.toMatchObject({ id: 'orc-1', itens: [{ descricao: 'Honorários' }] })
    expect(get).toHaveBeenCalledWith('/v1/orcamentos/ObterPorId/orc-1')
  })

  it('usa os endpoints de criação, aceite e conversão', async () => {
    const post = vi.spyOn(http, 'post')
      .mockResolvedValueOnce({ data: 'orc-1' })
      .mockResolvedValueOnce({ data: undefined })
      .mockResolvedValueOnce({ data: 'con-1' })
    const payload = { prospectoId: 'pro-1', titulo: 'Proposta', diaVencimento: 10, itens: [{ descricao: 'Honorários', secao: 2, tipo: 2, naturezaCobranca: 2, quantidade: 1, valorUnitario: 500, ordem: 1 }] }
    await expect(criarOrcamento(payload)).resolves.toBe('orc-1')
    await aceitarOrcamento('orc-1')
    await expect(converterOrcamento('orc-1', { dataInicio: '2026-09-19', confirmarSubstituicaoContratoAtivo: true })).resolves.toBe('con-1')
    expect(post).toHaveBeenNthCalledWith(1, '/v1/orcamentos/Criar', payload)
    expect(post).toHaveBeenNthCalledWith(2, '/v1/orcamentos/Aceitar/orc-1')
    expect(post).toHaveBeenNthCalledWith(3, '/v1/orcamentos/ConverterParaContrato/orc-1', { dataInicio: '2026-09-19', confirmarSubstituicaoContratoAtivo: true })
  })

  it('consulta contratos ativos da empresa antes de converter um orcamento', async () => {
    const get = vi.spyOn(http, 'get').mockResolvedValue({ data: [{
      id: 'con-1', numero: 'CTR-2026-0001', dataInicio: '2026-01-01T00:00:00', dataFim: null,
    }] })

    await expect(listarContratosAtivosParaConversao('orc-1')).resolves.toEqual([{
      id: 'con-1', numero: 'CTR-2026-0001', dataInicio: '2026-01-01T00:00:00', dataFim: null,
    }])
    expect(get).toHaveBeenCalledWith('/v1/orcamentos/ObterContratosAtivosParaConversao/orc-1')
  })

  it('atualiza um orçamento editável', async () => {
    const put = vi.spyOn(http, 'put').mockResolvedValue({ data: 'orc-1' })
    const payload = {
      titulo: 'Proposta revisada',
      diaVencimento: 15,
      observacoes: 'Nova condição',
      itens: [{ descricao: 'Honorários', secao: 2, tipo: 2, naturezaCobranca: 2, quantidade: 1, valorUnitario: 650, ordem: 1 }],
    }

    await expect(atualizarOrcamento('orc-1', payload)).resolves.toBe('orc-1')
    expect(put).toHaveBeenCalledWith('/v1/orcamentos/Atualizar/orc-1', payload)
  })
})
