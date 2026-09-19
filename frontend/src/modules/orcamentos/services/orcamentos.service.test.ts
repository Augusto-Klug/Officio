import { afterEach, describe, expect, it, vi } from 'vitest'
import { http } from '../../../api/http'
import { aceitarOrcamento, atualizarOrcamento, converterOrcamento, criarOrcamento, normalizeOrcamentos, obterOrcamento } from './orcamentos.service'

afterEach(() => vi.restoreAllMocks())

describe('contrato HTTP de orçamentos', () => {
  it('normaliza a lista e seus itens', () => {
    const result = normalizeOrcamentos([{
      id: 'orc-1', prospectoId: 'pro-1', titulo: 'Proposta mensal', status: 'Rascunho',
      valorTotal: 500, diaVencimento: 10, observacoes: null, motivoRecusa: null,
      itens: [{ id: 'item-1', itemCatalogoOrcamentoId: null, descricao: 'Honorários', secao: 'Honorarios', tipo: 'ItemMonetario', quantidade: 1, valorUnitario: 500, valorTotal: 500, ordem: 1 }],
    }])

    expect(result[0]).toMatchObject({ id: 'orc-1', valorTotal: 500, itens: [{ descricao: 'Honorários' }] })
  })

  it('rejeita resposta estruturalmente inválida', () => {
    expect(() => normalizeOrcamentos({ data: [] })).toThrow('formato inesperado')
    expect(() => normalizeOrcamentos([{ id: 'orc-1', itens: null }])).toThrow()
  })

  it('obtém um orçamento por id', async () => {
    const resposta = {
      id: 'orc-1', prospectoId: 'pro-1', titulo: 'Proposta mensal', status: 'Rascunho',
      valorTotal: 500, diaVencimento: 10, observacoes: null, motivoRecusa: null,
      itens: [{ id: 'item-1', itemCatalogoOrcamentoId: null, descricao: 'Honorários', secao: 'Honorarios', tipo: 'ItemMonetario', quantidade: 1, valorUnitario: 500, valorTotal: 500, ordem: 1 }],
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
    const payload = { prospectoId: 'pro-1', titulo: 'Proposta', diaVencimento: 10, itens: [{ descricao: 'Honorários', secao: 2, tipo: 2, quantidade: 1, valorUnitario: 500, ordem: 1 }] }
    await expect(criarOrcamento(payload)).resolves.toBe('orc-1')
    await aceitarOrcamento('orc-1')
    await expect(converterOrcamento('orc-1', { dataInicio: '2026-09-19', renovacaoAutomatica: true })).resolves.toBe('con-1')
    expect(post).toHaveBeenNthCalledWith(1, '/v1/orcamentos/Criar', payload)
    expect(post).toHaveBeenNthCalledWith(2, '/v1/orcamentos/Aceitar/orc-1')
    expect(post).toHaveBeenNthCalledWith(3, '/v1/orcamentos/ConverterParaContrato/orc-1', { dataInicio: '2026-09-19', renovacaoAutomatica: true })
  })

  it('atualiza um orçamento editável', async () => {
    const put = vi.spyOn(http, 'put').mockResolvedValue({ data: 'orc-1' })
    const payload = {
      titulo: 'Proposta revisada',
      diaVencimento: 15,
      observacoes: 'Nova condição',
      itens: [{ descricao: 'Honorários', secao: 2, tipo: 2, quantidade: 1, valorUnitario: 650, ordem: 1 }],
    }

    await expect(atualizarOrcamento('orc-1', payload)).resolves.toBe('orc-1')
    expect(put).toHaveBeenCalledWith('/v1/orcamentos/Atualizar/orc-1', payload)
  })
})
