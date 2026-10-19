import { afterEach, describe, expect, it, vi } from 'vitest'
import { http } from '../../../api/http'
import { listarEmpresasComAdmissoes, normalizeEmpresasComAdmissoes } from './admissoes.service'
import { aprovarDocumentoAdmissao, atualizarAdmissao, atualizarFichaAdmissao, atualizarFichaEmpresaAdmissao, cancelarAdmissao, concluirAdmissao, concluirFichaAdmissao, concluirFichaEmpresaAdmissao, criarAdmissao, enviarDocumentoAdmissao, enviarDocumentoFichaAdmissao, gerarLinkFichaAdmissao, gerarLinkFichaEmpresaAdmissao, listarAdmissoes, listarDocumentosAdmissao, normalizeAdmissao, normalizeAdmissoes, normalizeDocumentosAdmissao, normalizeFichaEmpresa, normalizeFichaFuncionario, obterAdmissao, obterFichaAdmissao, obterFichaEmpresaAdmissao, obterUrlDocumentoAdmissao, obterUrlDocumentoFichaAdmissao, removerDocumentoAdmissao, revogarLinkFichaAdmissao, revogarLinkFichaEmpresaAdmissao, validarDocumentoAdmissao } from './admissoes.service'

afterEach(() => vi.restoreAllMocks())

describe('agrupamento de empresas com admissoes', () => {
  it('normaliza e consulta o endpoint agrupado', async () => {
    const agrupamento = { empresaId: 'emp-1', empresaNome: 'Empresa Teste', cnpj: '11222333000144', quantidadeAdmissoes: 3 }
    expect(normalizeEmpresasComAdmissoes([agrupamento])).toEqual([agrupamento])

    const get = vi.spyOn(http, 'get').mockResolvedValue({ data: [agrupamento] })
    await expect(listarEmpresasComAdmissoes('Empresa')).resolves.toEqual([agrupamento])
    expect(get).toHaveBeenCalledWith('/v1/admissoes/ListarEmpresasComAdmissoes', {
      params: { busca: 'Empresa' }, signal: undefined,
    })
  })

  it('envia empresaId como escopo da lista detalhada', async () => {
    const get = vi.spyOn(http, 'get').mockResolvedValue({ data: [] })
    await listarAdmissoes({ empresaId: 'emp-1', busca: 'Ana' })
    expect(get).toHaveBeenCalledWith('/v1/admissoes/Listar', {
      params: { empresaId: 'emp-1', busca: 'Ana' }, signal: undefined,
    })
  })
})

const resumo = {
  id: 'adm-1', empresaId: 'emp-1', empresaNome: 'Empresa Teste', nomeFuncionario: 'Ana Silva',
  cpf: '12345678901', status: 'RASCUNHO', statusDescricao: 'Rascunho',
  dataAdmissaoPrevista: '2026-09-22', funcao: 'Analista', salario: 4500, criadoEm: '2026-09-19T10:00:00Z',
  preenchimentoFuncionarioConcluido: false, preenchimentoEmpresaConcluido: false,
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
  it('normaliza documentos da admissao', () => {
    const documentos = normalizeDocumentosAdmissao([{
      id: 'item-1', tipoDocumentoId: 'tipo-1', tipoCodigo: 'IDENTIDADE', tipoDescricao: 'Identidade',
      rotulo: 'Documento de identidade', obrigatorio: true, ordem: 1, documentoId: null,
      nomeArquivo: null, mimeType: null, tamanhoBytes: null, qualidadeEstimada: null, enviadoEm: null,
      statusValidacao: 'PENDENTE_ENVIO', confiancaOcr: null, mensagemValidacao: null,
      camposExtraidos: { cpf: '12345678901' },
      divergencias: [{ campo: 'cpf', valorDocumento: '12345678901', valorCadastro: '10987654321', similaridade: 0.4 }],
      statusProcessamentoOcr: 'CONCLUIDO', categoriaResultadoOcr: 'OCR_CONCLUIDO', modeloOcr: 'prebuilt-idDocument',
      statusHttpOcr: 200, codigoErroOcr: null, idRequisicaoOcr: 'request-123',
      confiancasCamposOcr: { cpf: 0.98, nome: 0.72 },
    }])

    expect(documentos[0]).toMatchObject({
      id: 'item-1', obrigatorio: true, statusValidacao: 'PENDENTE_ENVIO',
      camposExtraidos: { cpf: '12345678901' }, divergencias: [{ campo: 'cpf', similaridade: 0.4 }],
      statusProcessamentoOcr: 'CONCLUIDO', categoriaResultadoOcr: 'OCR_CONCLUIDO',
      modeloOcr: 'prebuilt-idDocument', statusHttpOcr: 200, idRequisicaoOcr: 'request-123',
      confiancasCamposOcr: { cpf: 0.98, nome: 0.72 },
    })
  })

  it('usa os endpoints do fluxo de documentos', async () => {
    const documento = {
      id: 'item-1', tipoDocumentoId: 'tipo-1', tipoCodigo: 'CPF', tipoDescricao: 'CPF', rotulo: 'CPF',
      obrigatorio: true, ordem: 2, documentoId: null, nomeArquivo: null, mimeType: null, tamanhoBytes: null,
      qualidadeEstimada: null, enviadoEm: null, statusValidacao: 'PENDENTE_ENVIO', confiancaOcr: null,
      mensagemValidacao: null,
    }
    const get = vi.spyOn(http, 'get')
      .mockResolvedValueOnce({ data: [documento] })
      .mockResolvedValueOnce({ data: { url: 'https://storage/documento', expiraEm: '2026-09-19T12:00:00Z' } })
    const post = vi.spyOn(http, 'post')
      .mockResolvedValueOnce({ data: undefined })
      .mockResolvedValueOnce({ data: { valido: true, confiancaOcr: 0.98, fallbackManual: false, mensagem: null, divergencias: [] } })
      .mockResolvedValueOnce({ data: undefined })
    const remove = vi.spyOn(http, 'delete').mockResolvedValue({ data: undefined })
    const arquivo = new File(['pdf'], 'cpf.pdf', { type: 'application/pdf' })
    const verso = new File(['pdf-verso'], 'cpf-verso.pdf', { type: 'application/pdf' })

    await expect(listarDocumentosAdmissao('adm-1')).resolves.toHaveLength(1)
    await enviarDocumentoAdmissao('adm-1', 'item-1', [arquivo, verso])
    await expect(obterUrlDocumentoAdmissao('adm-1', 'item-1')).resolves.toMatchObject({ url: 'https://storage/documento' })
    await expect(validarDocumentoAdmissao('adm-1', 'item-1')).resolves.toMatchObject({ valido: true })
    await aprovarDocumentoAdmissao('adm-1', 'item-1', 'Conferido')
    await removerDocumentoAdmissao('adm-1', 'item-1')

    expect(post.mock.calls[0]?.[0]).toBe('/v1/admissoes/EnviarDocumento/adm-1/itens/item-1')
    expect((post.mock.calls[0]?.[1] as FormData).getAll('arquivos')).toEqual([arquivo, verso])
    expect(post).toHaveBeenNthCalledWith(2, '/v1/admissoes/ValidarDocumento/adm-1/itens/item-1')
    expect(post).toHaveBeenNthCalledWith(3, '/v1/admissoes/AprovarDocumento/adm-1/itens/item-1', { observacao: 'Conferido' })
    expect(remove).toHaveBeenCalledWith('/v1/admissoes/RemoverDocumento/adm-1/itens/item-1')
  })

  it('gera e revoga o link da ficha autenticada', async () => {
    const post = vi.spyOn(http, 'post')
      .mockResolvedValueOnce({ data: { token: 'token-1', expiraEm: '2026-09-26T10:00:00Z' } })
      .mockResolvedValueOnce({ data: undefined })

    await expect(gerarLinkFichaAdmissao('adm-1')).resolves.toEqual({ token: 'token-1', expiraEm: '2026-09-26T10:00:00Z' })
    await revogarLinkFichaAdmissao('adm-1')

    expect(post).toHaveBeenNthCalledWith(1, '/v1/admissoes/GerarLinkFicha/adm-1')
    expect(post).toHaveBeenNthCalledWith(2, '/v1/admissoes/RevogarLinkFicha/adm-1')
  })

  it('usa acesso anonimo e campos limitados no fluxo do admitido', async () => {
    const ficha = {
      ...resumo, expiraEm: '2026-09-26T10:00:00Z', preenchimentoConcluido: false,
      rg: null, dataNascimento: null, sexo: null, estadoCivil: null, escolaridade: null,
      telefone: null, email: null, pis: null, banco: null, agencia: null, conta: null,
      tipoConta: null, utilizaValeTransporte: false, comissao: null, contratoExperienciaDias: null,
      horarioTrabalho: null, dependentes: [], documentos: [],
    }
    const get = vi.spyOn(http, 'get')
      .mockResolvedValueOnce({ data: ficha })
      .mockResolvedValueOnce({ data: { url: 'https://storage/documento', expiraEm: '2026-09-19T12:00:00Z' } })
    const put = vi.spyOn(http, 'put').mockResolvedValue({ data: undefined })
    const post = vi.spyOn(http, 'post').mockResolvedValue({ data: undefined })
    const payload = {
      nomeFuncionario: 'Ana Silva', dataNascimento: '1995-05-10', sexo: 'Feminino',
      estadoCivil: 'Solteira', escolaridade: 'Superior', telefone: '47999999999',
      email: 'ana@example.com', dependentes: [],
    }
    const arquivo = new File(['pdf'], 'cpf.pdf', { type: 'application/pdf' })

    await expect(obterFichaAdmissao('tok/en+')).resolves.toMatchObject({ id: 'adm-1' })
    expect(normalizeFichaFuncionario(ficha)).toMatchObject({ empresaNome: 'Empresa Teste' })
    await atualizarFichaAdmissao('tok/en+', payload)
    await concluirFichaAdmissao('tok/en+')
    await enviarDocumentoFichaAdmissao('tok/en+', 'item-1', [arquivo])
    await obterUrlDocumentoFichaAdmissao('tok/en+', 'item-1')

    const token = 'tok%2Fen%2B'
    expect(get).toHaveBeenNthCalledWith(1, `/v1/admissoes-publicas/Obter/${token}`, { skipAuth: true })
    expect(put).toHaveBeenCalledWith(`/v1/admissoes-publicas/Atualizar/${token}`, payload, { skipAuth: true })
    expect(post).toHaveBeenNthCalledWith(1, `/v1/admissoes-publicas/Concluir/${token}`, undefined, { skipAuth: true })
    expect(post.mock.calls[1]?.[0]).toBe(`/v1/admissoes-publicas/EnviarDocumento/${token}/itens/item-1`)
    expect(post.mock.calls[1]?.[2]).toEqual({ skipAuth: true })
    expect(get).toHaveBeenNthCalledWith(2, `/v1/admissoes-publicas/ObterUrlDocumento/${token}/itens/item-1`, { skipAuth: true })
  })

  it('usa endpoints anônimos separados para a etapa do empregador', async () => {
    const ficha = {
      id: 'adm-1', empresaNome: 'Empresa Teste', nomeFuncionario: 'Ana Silva', cpf: '12345678901',
      rg: '1234567', dataNascimento: '1995-05-10', sexo: 'Feminino', estadoCivil: 'Solteira',
      escolaridade: 'Superior', telefone: '47999999999', email: 'ana@example.com', banco: null,
      agencia: null, conta: null, tipoConta: null, pis: null, dataAdmissaoPrevista: null, salario: null,
      comissao: null, funcao: null, contratoExperienciaDias: null, utilizaValeTransporte: null,
      horarioTrabalho: null, preenchimentoConcluido: false, expiraEm: '2026-09-26T10:00:00Z',
    }
    const get = vi.spyOn(http, 'get').mockResolvedValue({ data: ficha })
    const put = vi.spyOn(http, 'put').mockResolvedValue({ data: undefined })
    const post = vi.spyOn(http, 'post')
      .mockResolvedValueOnce({ data: { token: 'employer-token', expiraEm: ficha.expiraEm } })
      .mockResolvedValueOnce({ data: undefined })
      .mockResolvedValueOnce({ data: undefined })
      .mockResolvedValueOnce({ data: undefined })

    await expect(gerarLinkFichaEmpresaAdmissao('adm-1')).resolves.toEqual({ token: 'employer-token', expiraEm: ficha.expiraEm })
    await revogarLinkFichaEmpresaAdmissao('adm-1')
    await expect(obterFichaEmpresaAdmissao('emp/token')).resolves.toMatchObject({ id: 'adm-1' })
    expect(normalizeFichaEmpresa(ficha)).toMatchObject({ nomeFuncionario: 'Ana Silva' })
    const payload = {
      pis: '12345678901', funcao: 'Analista', banco: 'Banco Teste', agencia: '0001',
      conta: '12345-6', tipoConta: 'Corrente',
    }
    await atualizarFichaEmpresaAdmissao('emp/token', payload)
    await concluirFichaEmpresaAdmissao('emp/token')

    expect(post).toHaveBeenNthCalledWith(1, '/v1/admissoes/GerarLinkFichaEmpresa/adm-1')
    expect(post).toHaveBeenNthCalledWith(2, '/v1/admissoes/RevogarLinkFichaEmpresa/adm-1')
    const token = 'emp%2Ftoken'
    expect(get).toHaveBeenCalledWith(`/v1/admissoes-publicas/ObterEmpresa/${token}`, { skipAuth: true })
    expect(put).toHaveBeenCalledWith(`/v1/admissoes-publicas/AtualizarEmpresa/${token}`, payload, { skipAuth: true })
    expect(post).toHaveBeenNthCalledWith(3, `/v1/admissoes-publicas/ConcluirEmpresa/${token}`, undefined, { skipAuth: true })
  })
})
