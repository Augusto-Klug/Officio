import { afterEach, describe, expect, it, vi } from 'vitest'
import { http } from '../../../api/http'
import { criarAdmissaoPortal, obterPdfAssinadoPortal } from './portal-cliente.service'

afterEach(() => vi.restoreAllMocks())

describe('portal do cliente', () => {
  it('cria uma admissao enviando somente empresa e email do funcionario', async () => {
    const post = vi.spyOn(http, 'post').mockResolvedValue({
      data: { id: 'adm-1', emailFuncionario: 'funcionario@example.com', linkFuncionarioEnviado: true },
    })

    await expect(criarAdmissaoPortal({ empresaId: 'empresa-1', emailFuncionario: 'funcionario@example.com' }))
      .resolves.toEqual({ id: 'adm-1', emailFuncionario: 'funcionario@example.com', linkFuncionarioEnviado: true })

    expect(post).toHaveBeenCalledWith('/v1/portal-cliente/CriarAdmissao', {
      empresaId: 'empresa-1', emailFuncionario: 'funcionario@example.com',
    })
  })

  it('obtém o PDF assinado por uma rota de portal com escopo de empresa', async () => {
    const get = vi.spyOn(http, 'get').mockResolvedValue({ data: { pdfUrl: 'https://storage.test/signed.pdf' } })

    await expect(obterPdfAssinadoPortal('empresa-1', 'contrato-1'))
      .resolves.toBe('https://storage.test/signed.pdf')

    expect(get).toHaveBeenCalledWith('/v1/portal-cliente/ObterPdfAssinado/empresa-1/contrato-1')
  })
})
