import { describe, expect, it } from 'vitest'
import { formatDate } from './format'

describe('formatDate', () => {
  it('preserva a data civil sem deslocamento de fuso horário', () => {
    expect(formatDate('2026-09-22')).toBe('22/09/2026')
  })

  it('trata valores ausentes ou inválidos', () => {
    expect(formatDate(null)).toBe('Não informado')
    expect(formatDate('data-invalida')).toBe('Não informado')
  })
})
