import { describe, expect, it } from 'vitest'
import {
  naturezaCobrancaOptions,
  rotuloStatusOrcamento,
  secaoOptions,
  tipoOptions,
} from './orcamento'

describe('rótulos comerciais apresentados no orçamento', () => {
  it('apresenta os novos rótulos sem alterar os valores dos tipos', () => {
    expect(tipoOptions).toEqual([
      { label: 'Serviço / Escopo', value: 1 },
      { label: 'Item com valor', value: 2 },
      { label: 'Condição comercial', value: 3 },
    ])
  })

  it('preserva as seções e naturezas de cobrança atuais', () => {
    expect(secaoOptions.map(({ label }) => label)).toEqual([
      'Serviços inclusos',
      'Honorários',
      'Serviços separados',
      'Criação de empresa',
    ])
    expect(naturezaCobrancaOptions.map(({ label }) => label)).toEqual([
      'Não recorrente',
      'Honorário mensal',
      'Por folha',
    ])
  })

  it('exibe o status Substituido com rótulo localizado', () => {
    expect(rotuloStatusOrcamento('Substituido')).toBe('Substituído')
  })
})
