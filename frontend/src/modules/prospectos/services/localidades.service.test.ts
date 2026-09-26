import { describe, expect, it } from 'vitest'
import { normalizeEnderecoCep, normalizeEstados, normalizeMunicipios } from './localidades.service'

describe('servico de localidades', () => {
  it('normaliza estados e municipios retornados pelo backend', () => {
    expect(normalizeEstados([{ codigoIbge: '42', sigla: 'SC', nome: 'Santa Catarina' }])).toEqual([
      { codigoIbge: '42', sigla: 'SC', nome: 'Santa Catarina' },
    ])
    expect(normalizeMunicipios([{ codigoIbge: '4202404', nome: 'Blumenau', uf: 'SC' }])).toEqual([
      { codigoIbge: '4202404', nome: 'Blumenau', uf: 'SC' },
    ])
  })

  it('normaliza endereco consultado por CEP', () => {
    expect(normalizeEnderecoCep({
      cep: '89010000',
      logradouro: 'Rua XV de Novembro',
      complemento: null,
      bairro: 'Centro',
      cidade: 'Blumenau',
      uf: 'SC',
      municipioCodigoIbge: '4202404',
    })).toMatchObject({ cep: '89010000', cidade: 'Blumenau', municipioCodigoIbge: '4202404' })
  })
})
