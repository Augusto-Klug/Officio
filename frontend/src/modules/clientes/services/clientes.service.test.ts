import { afterEach, describe, expect, it, vi } from 'vitest'
import { http } from '../../../api/http'
import { atualizarCliente, listarClientes, normalizarRegimeTributario, normalizeClienteDetalhe, normalizeClientes, vincularCodigoSci } from './clientes.service'
import { RegimeTributario, regimeTributarioOptions } from '../../prospectos/types/prospecto'

afterEach(() => vi.restoreAllMocks())

describe('contrato HTTP de clientes', () => {
  it.each(regimeTributarioOptions)('normaliza o regime tributário %s pelo nome persistido, número ou rótulo', (opcao) => {
    const enumName = RegimeTributario[opcao.value]
    expect(normalizarRegimeTributario(enumName)).toBe(opcao.value)
    expect(normalizarRegimeTributario(String(opcao.value))).toBe(opcao.value)
    expect(normalizarRegimeTributario(opcao.label)).toBe(opcao.value)
  })

  it('retorna nulo quando o regime tributário não é reconhecido', () => {
    expect(normalizarRegimeTributario('RegimeDesconhecido')).toBeNull()
    expect(normalizarRegimeTributario(null)).toBeNull()
  })

  it('normaliza listagem e aceita propriedades PascalCase', () => {
    expect(normalizeClientes([{ Id: 'c1', EmpresaId: 'e1', RazaoSocial: 'Empresa Ltda.', Ativo: true, QuantidadeSocios: 2, QuantidadeContratos: 1, QuantidadeDocumentos: 3 }])).toEqual([
      expect.objectContaining({ id: 'c1', razaoSocial: 'Empresa Ltda.', ativo: true, quantidadeSocios: 2, quantidadeContratos: 1, quantidadeDocumentos: 3 }),
    ])
  })

  it('normaliza detalhe e coleções relacionadas', () => {
    const result = normalizeClienteDetalhe({
      id: 'c1', empresaId: 'e1', razaoSocial: 'Empresa', ativo: true,
      socios: [{ id: 's1', nome: 'Ana', participacao: 100, ativo: true, vigenteDesde: '2026-01-01T00:00:00Z' }],
      contratos: [{ id: 'ct1', numero: 'CTR-1', status: 'Ativo', valorTotal: 500, dataInicio: '2026-09-01' }],
      documentos: [{ id: 'd1', nomeArquivo: 'contrato.pdf', tipo: 'Contrato', legivel: true, qualidadeEstimada: 0.98 }],
    })
    expect(result.socios[0]?.nome).toBe('Ana')
    expect(result.contratos[0]?.numero).toBe('CTR-1')
    expect(result.documentos[0]?.legivel).toBe(true)
  })

  it('usa filtros e rota pública do controller', async () => {
    const get = vi.spyOn(http, 'get').mockResolvedValue({ data: [] })
    await listarClientes({ busca: 'Empresa', ativo: true })
    expect(get).toHaveBeenCalledWith('/v1/clientes/Listar', { params: { busca: 'Empresa', ativo: true }, signal: undefined })
  })

  it('vincula código SCI no endpoint correto', async () => {
    const patch = vi.spyOn(http, 'patch').mockResolvedValue({ data: undefined })
    await vincularCodigoSci('c1', 'SCI-42')
    expect(patch).toHaveBeenCalledWith('/v1/clientes/VincularCodigoSci/c1', { sciCodigo: 'SCI-42' })
  })

  it('sends company and versioned partner data in the update payload', async () => {
    const put = vi.spyOn(http, 'put').mockResolvedValue({ data: undefined })
    await atualizarCliente('c1', {
      razaoSocial: 'Updated Company', nomeFantasia: null, cnpj: '11222333000144', cpf: null,
      regimeTributario: 0, email: null, telefone: null, whatsapp: null, inscricaoEstadual: null,
      inscricaoMunicipal: null, tipoEmpresa: null, observacoes: null,
      endereco: { cep: '', logradouro: '', numero: '', complemento: '', bairro: '', cidade: '', uf: '', municipioCodigoIbge: '' },
      socios: [{ id: 's1', nome: 'Ana', cpf: '12345678900', participacao: 65, endereco: { municipioCodigoIbge: '4202404' } }],
    })
    expect(put).toHaveBeenCalledWith('/v1/clientes/Atualizar/c1', expect.objectContaining({
      cnpj: '11222333000144', socios: [expect.objectContaining({ id: 's1', participacao: 65 })],
      municipioCodigoIbge: '',
    }))
  })
})
