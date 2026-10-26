export interface RepresentanteDisponivel {
  id: string
  nome: string
  cpf: string | null
  tipoRelacao: string | null
  cadastroCompleto: boolean
  informacoesFaltantes: string[]
}
export interface RepresentantesContrato { prospectoId: string; socios: RepresentanteDisponivel[] }
export function representanteValido(socios: RepresentanteDisponivel[], id: string | null): boolean {
  return socios.some(s => s.id === id && s.cadastroCompleto)
}
