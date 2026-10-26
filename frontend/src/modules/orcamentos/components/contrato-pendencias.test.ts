import { describe, expect, it } from 'vitest'
import { extrairPendenciasContrato } from './contrato-pendencias'

describe('pendências da geração do contrato', () => {
  it('apresenta as mensagens recebidas do backend sem duplicações', () => {
    expect(extrairPendenciasContrato({
      pendencias: ['CRC da CONTRATADA não informado.', 'Endereço incompleto.'],
      vigencia: ['Endereço incompleto.'],
    })).toEqual(['CRC da CONTRATADA não informado.', 'Endereço incompleto.'])
  })
})
