import { describe, expect, it } from 'vitest'
import { formatDate, formatSaoPauloDateTime } from './format'

describe('formatDate', () => {
  it('preserva a data civil sem deslocamento de fuso horário', () => {
    expect(formatDate('2026-09-22')).toBe('22/09/2026')
  })

  it('trata valores ausentes ou inválidos', () => {
    expect(formatDate(null)).toBe('Não informado')
    expect(formatDate('data-invalida')).toBe('Não informado')
  })
})

describe('formatSaoPauloDateTime', () => {
  it('converte UTC para o fuso de Sao Paulo', () => {
    expect(formatSaoPauloDateTime('2026-09-19T14:30:00Z')).toContain('11:30')
  })

  it('trata timestamp legado sem indicador de fuso como UTC', () => {
    expect(formatSaoPauloDateTime('2026-09-19T14:30:00')).toContain('11:30')
  })

  it('trata valores invalidos', () => {
    expect(formatSaoPauloDateTime('data-invalida')).toBe('Não informado')
  })
})
