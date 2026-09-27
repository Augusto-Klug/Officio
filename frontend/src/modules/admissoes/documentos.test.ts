import { describe, expect, it } from 'vitest'
import { agruparDocumentosAdmissao } from './documentos'
import type { AdmissaoDocumento } from './types/admissao'

function documento(tipoCodigo: string, ordem: number): AdmissaoDocumento {
  return {
    id: `${ordem}`,
    tipoDocumentoId: `tipo-${ordem}`,
    tipoCodigo,
    tipoDescricao: tipoCodigo,
    rotulo: tipoCodigo,
    obrigatorio: ordem <= 4,
    ordem,
    documentoId: null,
    nomeArquivo: null,
    mimeType: null,
    tamanhoBytes: null,
    qualidadeEstimada: null,
    enviadoEm: null,
    statusValidacao: 'PENDENTE_ENVIO',
    confiancaOcr: null,
    mensagemValidacao: null,
    camposExtraidos: {},
    divergencias: [],
  }
}

describe('agruparDocumentosAdmissao', () => {
  it('organiza o checklist nas categorias da ficha de admissao', () => {
    const grupos = agruparDocumentosAdmissao([
      documento('IDENTIDADE', 1),
      documento('COMPROVANTE_RESIDENCIA', 3),
      documento('CNH', 5),
      documento('COMPROVANTE_ESCOLARIDADE', 7),
    ])

    expect(grupos.map((grupo) => grupo.codigo)).toEqual(['identificacao', 'endereco', 'formacao'])
    expect(grupos[0]?.documentos.map((item) => item.tipoCodigo)).toEqual(['IDENTIFICACAO'])
    expect(grupos[0]?.documentos[0]?.alternativas?.map((item) => item.tipoCodigo)).toEqual(['IDENTIDADE', 'CNH'])
  })

  it('preserva tipos ainda nao categorizados', () => {
    const grupos = agruparDocumentosAdmissao([documento('DOCUMENTO_CUSTOMIZADO', 9)])

    expect(grupos).toHaveLength(1)
    expect(grupos[0]?.codigo).toBe('outros')
  })

  it('nao exibe o antigo item separado de CPF', () => {
    const grupos = agruparDocumentosAdmissao([
      documento('IDENTIDADE', 1),
      documento('CPF', 2),
    ])

    expect(grupos.flatMap((grupo) => grupo.documentos).map((item) => item.tipoCodigo))
      .toEqual(['IDENTIFICACAO'])
  })
})
