import type { AdmissaoDocumento } from './types/admissao'

export interface GrupoDocumentosAdmissao {
  codigo: string
  titulo: string
  icone: string
  documentos: AdmissaoDocumento[]
}

const categorias: ReadonlyArray<{
  codigo: string
  titulo: string
  icone: string
  tipos: readonly string[]
}> = [
  {
    codigo: 'identificacao',
    titulo: 'Identificação',
    icone: 'pi pi-id-card',
    tipos: ['IDENTIFICACAO', 'TITULO_ELEITOR', 'CARTEIRA_TRABALHO', 'CERTIFICADO_RESERVISTA'],
  },
  {
    codigo: 'endereco',
    titulo: 'Endereço',
    icone: 'pi pi-map-marker',
    tipos: ['COMPROVANTE_RESIDENCIA'],
  },
  {
    codigo: 'formacao',
    titulo: 'Formação',
    icone: 'pi pi-graduation-cap',
    tipos: ['COMPROVANTE_ESCOLARIDADE'],
  },
]

export function agruparDocumentosAdmissao(documentos: AdmissaoDocumento[]): GrupoDocumentosAdmissao[] {
  documentos = documentos.filter((documento) => documento.tipoCodigo !== 'CPF')
  const identificacoes = documentos.filter((documento) => ['IDENTIDADE', 'CNH'].includes(documento.tipoCodigo))
  const identificacaoAtual = identificacoes.find((documento) => documento.documentoId) ?? identificacoes[0]
  documentos = documentos.filter((documento) => !['IDENTIDADE', 'CNH'].includes(documento.tipoCodigo))
  if (identificacaoAtual) {
    documentos.push({
      ...identificacaoAtual,
      tipoCodigo: 'IDENTIFICACAO',
      rotulo: 'Identidade (RG/CIN) ou CNH',
      obrigatorio: true,
      alternativas: identificacoes,
    })
  }
  const grupos: GrupoDocumentosAdmissao[] = categorias.map((categoria) => ({
    codigo: categoria.codigo,
    titulo: categoria.titulo,
    icone: categoria.icone,
    documentos: documentos.filter((documento) => categoria.tipos.includes(documento.tipoCodigo)),
  }))

  const tiposConhecidos = new Set<string>(categorias.flatMap((categoria) => [...categoria.tipos]))
  const outros = documentos.filter((documento) => !tiposConhecidos.has(documento.tipoCodigo))
  if (outros.length) {
    grupos.push({
      codigo: 'outros',
      titulo: 'Outros documentos',
      icone: 'pi pi-file',
      documentos: outros,
    })
  }

  return grupos.filter((grupo) => grupo.documentos.length > 0)
}
