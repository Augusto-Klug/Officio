import { describe, expect, it } from 'vitest'
import { limparPayload, novoProspecto, validarProspecto } from './prospecto-form'

describe('validacao de endereco do prospecto', () => {
  function prospectoValido() {
    return { ...novoProspecto(), razaoSocial: 'Empresa', nomeContato: 'Contato', email: 'contato@example.com',
      telefone: '4733334444', cep: '89010000', logradouro: 'Rua das Flores', numero: '42', bairro: 'Centro',
      cidade: 'Blumenau', uf: 'SC', municipioCodigoIbge: '4202404' }
  }

  it('exige numero no endereco principal', () => {
    const payload = prospectoValido()

    expect(validarProspecto({ ...payload, numero: '' }).numero).toBeTruthy()
    expect(validarProspecto(payload).numero).toBeUndefined()
  })

  it('mantem os campos do socio opcionais, inclusive numero do endereco', () => {
    const payload = {
      ...prospectoValido(),
      socios: [{
        chave: 'socio-1', nome: '', cpf: null, participacao: null, profissao: null, tipoRelacaoId: null,
        endereco: { cep: '', logradouro: 'Rua das Flores', numero: '', complemento: '', bairro: '', cidade: '', uf: '', municipioCodigoIbge: '' },
      }],
    }

    expect(validarProspecto(payload)['socios.0.endereco.numero']).toBeUndefined()
    expect(validarProspecto(payload)).toEqual({})
  })

  it('mantem o endereco vazio do socio como opcional', () => {
    const payload = {
      ...prospectoValido(),
      socios: [{
        chave: 'socio-1', nome: '', cpf: null, participacao: null, profissao: null, tipoRelacaoId: null,
        endereco: { cep: '', logradouro: '', numero: '', complemento: '', bairro: '', cidade: '', uf: '', municipioCodigoIbge: '' },
      }],
    }

    expect(validarProspecto(payload)['socios.0.endereco.numero']).toBeUndefined()
    expect(limparPayload(payload).socios[0].endereco).toBeNull()
  })
})
