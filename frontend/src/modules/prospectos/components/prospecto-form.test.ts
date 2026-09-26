import { describe, expect, it } from 'vitest'
import { formatarCep, formatarCnpj, formatarCpf, limparPayload, novoProspecto, validarProspecto } from './prospecto-form'

describe('formulário de prospecto', () => {
  it('aplica máscaras sem aceitar caracteres excedentes', () => {
    expect(formatarCnpj('12345678000190abc')).toBe('12.345.678/0001-90')
    expect(formatarCpf('12345678909')).toBe('123.456.789-09')
    expect(formatarCep('89010000')).toBe('89010-000')
  })

  it('valida os campos coerentes com o contrato do backend', () => {
    const payload = novoProspecto()
    payload.cnpj = '123'
    payload.socios = [{ nome: '', cpf: '123', participacao: 101 }]

    expect(validarProspecto(payload)).toMatchObject({
      razaoSocial: expect.any(String), cnpj: expect.any(String),
      'socios.0.nome': expect.any(String), 'socios.0.cpf': expect.any(String), 'socios.0.participacao': expect.any(String),
    })
  })

  it('impede participação societária total acima de 100%', () => {
    const payload = novoProspecto()
    payload.razaoSocial = 'Empresa'
    payload.socios = [
      { nome: 'Sócio A', cpf: null, participacao: 60 },
      { nome: 'Sócio B', cpf: null, participacao: 50 },
    ]
    expect(validarProspecto(payload).socios).toBeTruthy()
  })

  it('remove espaços e converte opcionais vazios em nulo', () => {
    const payload = novoProspecto()
    payload.razaoSocial = '  Empresa  '
    payload.cnpj = '   '
    expect(limparPayload(payload)).toMatchObject({ razaoSocial: 'Empresa', cnpj: null, regimeTributario: null, socios: [] })
  })

  it('remove máscaras dos documentos antes de enviar ao backend', () => {
    const payload = novoProspecto()
    payload.razaoSocial = 'Empresa'
    payload.cnpj = '12.345.678/0001-90'
    payload.socios = [{ nome: 'Ana', cpf: '123.456.789-09', participacao: 100 }]
    expect(limparPayload(payload)).toMatchObject({
      cnpj: '12345678000190',
      socios: [{ nome: 'Ana', cpf: '12345678909', participacao: 100 }],
    })
  })

  it('exige UF e município quando o endereço for iniciado', () => {
    const payload = novoProspecto()
    payload.razaoSocial = 'Empresa'
    payload.cep = '89010000'

    expect(validarProspecto(payload)).toMatchObject({
      uf: expect.any(String),
      municipioCodigoIbge: expect.any(String),
    })
  })

  it('impede o cadastro quando a consulta confirmar que o CEP nao existe', () => {
    const payload = novoProspecto()
    payload.razaoSocial = 'Empresa'
    payload.cep = '00000000'

    expect(validarProspecto(payload, true).cep).toBe(
      'CEP inválido. Informe um CEP válido para continuar o cadastro.',
    )
  })

  it('preserva complemento e código IBGE no payload normalizado', () => {
    const payload = novoProspecto()
    payload.razaoSocial = 'Empresa'
    payload.complemento = ' Sala 12 '
    payload.municipioCodigoIbge = '4202404'

    expect(limparPayload(payload)).toMatchObject({
      complemento: 'Sala 12',
      municipioCodigoIbge: '4202404',
    })
  })
})
