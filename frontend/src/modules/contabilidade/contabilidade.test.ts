import { afterEach, describe, expect, it, vi } from 'vitest'
import { http } from '../../api/http'
import { atualizarDadosContabilidade, normalizeContabilidade, obterDadosContabilidade } from './contabilidade.service'
import { novoEndereco, prepararDados, validarDados, type DadosContabilidade } from './contabilidade-form'

afterEach(() => vi.restoreAllMocks())
const dados = (): DadosContabilidade => ({ razaoSocial: ' Escritório ', nomeFantasia: '', cnpj: '11.222.333/0001-44',
  crc: '', email: 'contato@teste.test', telefone: '(11) 99999-8888', endereco: null, responsavelTecnico: null })

describe('dados da contabilidade', () => {
  it('normaliza cadastro legado sem endereços e responsável', () => {
    expect(normalizeContabilidade({ RazaoSocial: 'Escritório', Cnpj: '11222333000144', Email: null }))
      .toMatchObject({ email: '', crc: '', telefone: '', endereco: null, responsavelTecnico: null })
  })
  it('preserva os dois endereços e a qualificação técnica na leitura', () => {
    const endereco = { ...novoEndereco(), logradouro: 'Rua A', cidade: 'São Paulo', uf: 'SP', municipioCodigoIbge: '3550308' }
    const d = { ...dados(), endereco, responsavelTecnico: { nome: 'Ana', cpf: '12345678901', profissao: 'Contadora', crc: 'SP-1', endereco: { ...endereco, numero: '10' } } }
    expect(normalizeContabilidade(d)).toEqual(d)
  })
  it('usa endpoints sem identificadores de parceiro', async () => {
    const get = vi.spyOn(http, 'get').mockResolvedValue({ data: dados() })
    const put = vi.spyOn(http, 'put').mockResolvedValue({ data: dados() })
    const signal = new AbortController().signal
    await obterDadosContabilidade(signal)
    await atualizarDadosContabilidade(dados())
    expect(get).toHaveBeenCalledWith('/v1/contabilidade', { signal })
    expect(put).toHaveBeenCalledWith('/v1/contabilidade', dados())
  })
  it('normaliza documentos e telefone e não cria endereços vazios', () => {
    const d = prepararDados({ ...dados(), endereco: novoEndereco(), responsavelTecnico: {
      nome: ' Ana ', cpf: '123.456.789-01', profissao: ' Contadora ', crc: ' SP-1 ', endereco: novoEndereco() } })
    expect(d).toMatchObject({ razaoSocial: 'Escritório', cnpj: '11222333000144', telefone: '11999998888', endereco: null,
      responsavelTecnico: { nome: 'Ana', cpf: '12345678901', profissao: 'Contadora', crc: 'SP-1', endereco: null } })
  })
  it('permite salvar cadastro sem responsável e valida CNPJ e e-mail', () => {
    expect(validarDados(dados())).toBe('')
    expect(validarDados({ ...dados(), cnpj: '123' })).toContain('CNPJ')
    expect(validarDados({ ...dados(), email: 'email inválido' })).toContain('e-mail')
  })
  it('impede responsável parcial e CPF incompleto', () => {
    const r = { nome: 'Ana', cpf: '123', profissao: 'Contadora', crc: '', endereco: null }
    expect(validarDados({ ...dados(), responsavelTecnico: r })).toContain('CRC')
    expect(validarDados({ ...dados(), responsavelTecnico: { ...r, crc: 'SP-1' } })).toContain('CPF')
  })
  it('valida endereço incompleto em cada bloco', () => {
    expect(validarDados({ ...dados(), endereco: { ...novoEndereco(), bairro: 'Centro' } })).toContain('Contabilidade')
    expect(validarDados({ ...dados(), responsavelTecnico: { nome: 'Ana', cpf: '12345678901', profissao: 'Contadora', crc: 'SP-1',
      endereco: { ...novoEndereco(), cep: '123' } } })).toContain('Responsável técnico')
  })
})
