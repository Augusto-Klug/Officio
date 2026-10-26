import { describe, expect, it } from 'vitest'
import { formatarCep, formatarCnpj, formatarCpf, formatarCrc, formatarDocumentoFiscal, formatarPis, formatarTelefone } from './inputMasks'

describe('mascaras de campos brasileiros', () => {
  it('formata e limita CPF e CNPJ', () => {
    expect(formatarCpf('12345678901-extra')).toBe('123.456.789-01')
    expect(formatarCnpj('12345678000199-extra')).toBe('12.345.678/0001-99')
  })

  it('formata e limita CEP', () => {
    expect(formatarCep('89010000')).toBe('89010-000')
    expect(formatarCep('890100001')).toBe('89010-000')
  })

  it('formata telefone fixo e celular e limita a 11 digitos', () => {
    expect(formatarTelefone('4733334444')).toBe('(47) 3333-4444')
    expect(formatarTelefone('47999999999')).toBe('(47) 99999-9999')
    expect(formatarTelefone('47999999999123')).toBe('(47) 99999-9999')
  })

  it('formata PIS/PASEP e limita a 11 digitos', () => {
    expect(formatarPis('12345678901')).toBe('123.45678.90-1')
    expect(formatarPis('12345678901234')).toBe('123.45678.90-1')
  })

  it('formata CRC e limita a 8 digitos e 2 letras de UF', () => {
    expect(formatarCrc('1sc123456/s-7')).toBe('1SC123456/S-7')
    expect(formatarCrc('1SC2345678')).toBe('1SC234567/S-8')
    expect(formatarCrc('1SC123')).toBe('1SC123')
  })

  it('identifica CPF ou CNPJ para exibicao e preserva outros documentos', () => {
    expect(formatarDocumentoFiscal('12345678901')).toBe('123.456.789-01')
    expect(formatarDocumentoFiscal('12345678000199')).toBe('12.345.678/0001-99')
    expect(formatarDocumentoFiscal('documento legado')).toBe('documento legado')
  })
})
