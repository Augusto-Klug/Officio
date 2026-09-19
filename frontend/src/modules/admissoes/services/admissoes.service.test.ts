import { afterEach, describe, expect, it, vi } from 'vitest'
import { http } from '../../../api/http'
import { atualizarAdmissao, cancelarAdmissao, concluirAdmissao, criarAdmissao, listarAdmissoes, normalizeAdmissao, normalizeAdmissoes, obterAdmissao } from './admissoes.service'

afterEach(() => vi.restoreAllMocks())

const resumo = {
  id: 'adm-1', empresaId: 'emp-1', empresaNome: 'Empresa Teste', nomeFuncionario: 'Ana Silva',
  cpf: '12345678901', status: 'RASCUNHO', statusDescricao: 'Rascunho',
  dataAdmissaoPrevista: '2026-09-22', funcao: 'Analista', salario: 4500, criadoEm: '2026-09-19T10:00:00Z',
}

describe('contrato HTTP de admissões', () => {
  it('normaliza a lista de admissões', () => {
    expect(normalizeAdmissoes([resumo])).toEqual([resumo])
  })

  it('normaliza o detalhe e os dependentes', () => {
    const detalhe = normalizeAdmissao({
      ...resumo, rg: null, dataNascimento: null, sexo: null, estadoCivil: null, escolaridade: null,
      telefone: null, email: null, pis: null, banco: null, agencia: null, conta: null, tipoConta: null,
      comissao: null, contratoExperienciaDias: 90, utilizaValeTransporte: true, horarioTrabalho: null,
      observacoes: null, atualizadoEm: '2026-09-19T11:00:00Z',
      dependentes: [{ id: 'dep-1', nome: 'João Silva', cpf: null, dataNascimento: null, parentesco: 'Filho', dependenteIr: true, salarioFamilia: false }],
    })

    expect(detalhe).toMatchObject({ id: 'adm-1', contratoExperienciaDias: 90, dependentes: [{ nome: 'João Silva', dependenteIr: true }] })
  })

  it('rejeita respostas estruturalmente inválidas', () => {
    expect(() => normalizeAdmissoes({ data: [] })).toThrow('formato inesperado')
    expect(() => normalizeAdmissao({ ...resumo, dependentes: null })).toThrow('dependentes')
  })

  it('lista e obtém admissões pelos endpoints esperados', async () => {
    const get = vi.spyOn(http, 'get')
      .mockResolvedValueOnce({ data: [resumo] })
      .mockResolvedValueOnce({ data: { ...resumo, rg: null, dataNascimento: null, sexo: null, estadoCivil: null, escolaridade: null, telefone: null, email: null, pis: null, banco: null, agencia: null, conta: null, tipoConta: null, comissao: null, contratoExperienciaDias: null, utilizaValeTransporte: null, horarioTrabalho: null, observacoes: null, atualizadoEm: '2026-09-19T11:00:00Z', dependentes: [] } })

    await listarAdmissoes({ clienteId: 'cli-1', status: 'RASCUNHO', busca: 'Ana' })
    await expect(obterAdmissao('adm-1')).resolves.toMatchObject({ id: 'adm-1' })
    expect(get).toHaveBeenNthCalledWith(1, '/v1/admissoes/Listar', { params: { clienteId: 'cli-1', status: 'RASCUNHO', busca: 'Ana' }, signal: undefined })
    expect(get).toHaveBeenNthCalledWith(2, '/v1/admissoes/ObterPorId/adm-1')
  })

  it('cria, atualiza, conclui e cancela admissões', async () => {
    const post = vi.spyOn(http, 'post')
      .mockResolvedValueOnce({ data: 'adm-1' })
      .mockResolvedValueOnce({ data: undefined })
      .mockResolvedValueOnce({ data: undefined })
    const put = vi.spyOn(http, 'put').mockResolvedValue({ data: 'adm-1' })
    const dados = { nomeFuncionario: 'Ana Silva', cpf: '12345678901', funcao: 'Analista', dataAdmissaoPrevista: '2026-09-22', dependentes: [] }

    await expect(criarAdmissao({ clienteId: 'cli-1', dados })).resolves.toBe('adm-1')
    await expect(atualizarAdmissao('adm-1', dados)).resolves.toBe('adm-1')
    await concluirAdmissao('adm-1')
    await cancelarAdmissao('adm-2')

    expect(post).toHaveBeenNthCalledWith(1, '/v1/admissoes/CriarManual', { clienteId: 'cli-1', dados })
    expect(put).toHaveBeenCalledWith('/v1/admissoes/Atualizar/adm-1', { dados })
    expect(post).toHaveBeenNthCalledWith(2, '/v1/admissoes/Concluir/adm-1')
    expect(post).toHaveBeenNthCalledWith(3, '/v1/admissoes/Cancelar/adm-2')
  })
})
