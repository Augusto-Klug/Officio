import { describe, expect, it } from 'vitest'
import { obterItensObrigatoriosAusentes } from './validacao-itens-orcamento'

describe('itens obrigatórios do orçamento', () => {
  it('retorna as duas naturezas quando nenhum serviço foi selecionado', () => {
    expect(obterItensObrigatoriosAusentes([
      { descricao: '', secao: 2, tipo: 2, naturezaCobranca: 2, quantidade: 1, valorUnitario: 0, ordem: 1 },
    ])).toEqual(['Honorário mensal', 'Preço por folha'])
  })

  it('retorna apenas a natureza obrigatória que ainda não foi selecionada', () => {
    expect(obterItensObrigatoriosAusentes([
      { itemCatalogoOrcamentoId: 'honorario', descricao: 'Honorários', secao: 2, tipo: 2, naturezaCobranca: 2, quantidade: 1, valorUnitario: 500, ordem: 1 },
    ])).toEqual(['Preço por folha'])
  })

  it('aceita o orçamento quando as duas naturezas estão presentes', () => {
    expect(obterItensObrigatoriosAusentes([
      { itemCatalogoOrcamentoId: 'honorario', descricao: 'Honorários', secao: 2, tipo: 2, naturezaCobranca: 2, quantidade: 1, valorUnitario: 500, ordem: 1 },
      { itemCatalogoOrcamentoId: 'folha', descricao: 'Folha', secao: 2, tipo: 2, naturezaCobranca: 3, quantidade: 1, valorUnitario: 12, ordem: 2 },
    ])).toEqual([])
  })
})
