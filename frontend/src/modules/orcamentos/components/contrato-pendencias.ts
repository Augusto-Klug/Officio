export function extrairPendenciasContrato(erros: Record<string, string[]>): string[] {
  return [...new Set(Object.values(erros).flat().filter((erro) => Boolean(erro.trim())))]
}
