import { afterEach, describe, expect, it, vi } from 'vitest'
import { http } from '../../../api/http'
import { enviarContratoParaAssinatura, gerarPdfContrato, normalizeContratos, obterPdfAssinado } from './contratos.service'

afterEach(() => vi.restoreAllMocks())

describe('contrato HTTP de contratos', () => {
  const contract = {
    id: 'con-1', numero: 'CTR-2026-0001', prospectoId: 'pro-1', clienteId: null,
    orcamentoId: 'orc-1', razaoSocial: 'Empresa Teste', status: 'AguardandoAssinatura',
    valorTotal: 500, diaVencimento: 10, dataInicio: '2026-09-19T00:00:00', dataFim: null,
    renovacaoAutomatica: true, clausulasExtras: null, pdfGerado: true,
    pdfAssinadoDisponivel: false, assinadoEm: null,
  }

  it('normaliza os indicadores usados pelas ações da tela', () => {
    expect(normalizeContratos([contract])[0]).toMatchObject({
      id: 'con-1', status: 'AguardandoAssinatura', pdfGerado: true, pdfAssinadoDisponivel: false,
    })
  })

  it('aceita PascalCase na fronteira e rejeita indicadores inválidos', () => {
    const pascal = Object.fromEntries(Object.entries(contract).map(([key, value]) => [key[0]!.toUpperCase() + key.slice(1), value]))
    expect(normalizeContratos([pascal])[0]?.numero).toBe('CTR-2026-0001')
    expect(() => normalizeContratos([{ ...contract, pdfGerado: 'sim' }])).toThrow('indicador inválido')
  })

  it('normaliza envio digital e fallback manual', async () => {
    const post = vi.spyOn(http, 'post')
      .mockResolvedValueOnce({ data: { metodo: 'digital_api', assinaturaExternaId: 'ext-1', signatarios: [{ email: 'ana@empresa.com', link: 'https://assinatura.test/1' }] } })
      .mockResolvedValueOnce({ data: { metodo: 'manual_upload', message: 'Use o PDF.', pdfUrl: 'https://arquivos.test/manual.pdf' } })
    const payload = { signatarios: [{ nome: 'Ana', email: 'ana@empresa.com', tipo: 'cliente' }] }
    await expect(enviarContratoParaAssinatura('con-1', payload)).resolves.toMatchObject({ envelopeId: 'ext-1', signatarios: [{ email: 'ana@empresa.com' }] })
    await expect(enviarContratoParaAssinatura('con-1', payload)).resolves.toMatchObject({ metodo: 'manual_upload', mensagem: 'Use o PDF.', pdfUrl: 'https://arquivos.test/manual.pdf' })
  })

  it('obtém os links dos PDFs gerado e assinado', async () => {
    const post = vi.spyOn(http, 'post').mockResolvedValue({ data: { pdfUrl: 'https://arquivos.test/gerado.pdf' } })
    const get = vi.spyOn(http, 'get').mockResolvedValue({ data: { pdfUrl: 'https://arquivos.test/assinado.pdf' } })
    await expect(gerarPdfContrato('con-1')).resolves.toEqual({ pdfUrl: 'https://arquivos.test/gerado.pdf' })
    await expect(obterPdfAssinado('con-1')).resolves.toBe('https://arquivos.test/assinado.pdf')
    expect(post).toHaveBeenCalledWith('/v1/contratos/GerarPdf/con-1')
    expect(get).toHaveBeenCalledWith('/v1/contratos/ObterPdfAssinado/con-1')
  })
})
