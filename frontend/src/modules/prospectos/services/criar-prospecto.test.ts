import { afterEach, expect, it } from 'vitest'
import { AxiosHeaders } from 'axios'
import { http } from '../../../api/http'
import { clearSession, setSession } from '../../../stores/session'
import { criarProspecto } from './prospectos.service'
import { payloadProspecto, validarProspecto, type ProspectoFormulario } from '../types/criar-prospecto'

const form = (): ProspectoFormulario => ({ razaoSocial: ' Empresa ', cnpj: '', regimeTributario: null, socios: [] })
afterEach(() => { delete http.defaults.adapter; clearSession() })

it.each([null, '0', '1', '2', '3'])('envia regime %s no contrato numérico do backend com Bearer', async regime => {
  const dados = { ...form(), regimeTributario: regime }
  setSession({ token: 'token-teste', expiraEm: new Date(Date.now() + 60_000).toISOString(), usuario: { id: '1', nome: 'Maria', email: 'maria@test.local', perfil: 'Admin' } })
  http.defaults.adapter = async config => {
    expect(config.url).toBe('/v1/prospectos/Criar')
    expect(config.method).toBe('post')
    expect(config.headers.get('Authorization')).toBe('Bearer token-teste')
    expect(config.headers.get('Content-Type')).toBe('application/json')
    expect(JSON.parse(config.data)).toEqual({ razaoSocial: 'Empresa', cnpj: null, regimeTributario: regime === null ? null : Number(regime), socios: [] })
    return { data: { id: 'novo', razaoSocial: 'Empresa' }, status: 201, statusText: 'Created', headers: new AxiosHeaders(), config }
  }
  await criarProspecto(payloadProspecto(dados))
})

it.each(['12345678000190', '12.345.678/0001-90'])('envia CNPJ %s com os 18 caracteres exigidos', cnpj => {
  const dados = { ...form(), cnpj }
  expect(validarProspecto(dados)).toEqual({})
  expect(payloadProspecto(dados).cnpj).toBe('12.345.678/0001-90')
})

it('normaliza sócios e não exige participação total de 100%', () => {
  const dados = { ...form(), socios: [{ id: 4, nome: ' Maria ', cpf: '', participacao: '25,5' }] }
  expect(validarProspecto(dados)).toEqual({})
  expect(payloadProspecto(dados).socios).toEqual([{ nome: 'Maria', cpf: null, participacao: 25.5 }])
})

it.each(['', '-1', '101', 'texto', 'NaN', '10.123'])('rejeita participação inválida: %s', participacao => {
  expect(validarProspecto({ ...form(), socios: [{ id: 0, nome: 'Maria', cpf: '', participacao }] })).toHaveProperty('socio-0-participacao')
})

it('valida comprimento da razão social, CPF incompleto e regime desconhecido', () => {
  expect(validarProspecto({ ...form(), razaoSocial: 'a'.repeat(301), regimeTributario: '4', socios: [{ id: 0, nome: 'Maria', cpf: '123', participacao: '0' }] })).toMatchObject({
    razaoSocial: 'Use até 300 caracteres.', regimeTributario: 'Selecione um regime tributário válido.', 'socio-0-cpf': expect.any(String),
  })
})
