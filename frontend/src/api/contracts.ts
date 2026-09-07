// Variações conhecidas do backend ficam restritas à fronteira HTTP.
export function record(value: unknown): Record<string, unknown> {
  if (!value || typeof value !== 'object' || Array.isArray(value)) {
    throw new Error('A API retornou uma resposta em formato inesperado.')
  }
  return value as Record<string, unknown>
}

export function field(value: Record<string, unknown>, ...names: string[]): unknown {
  for (const name of names) {
    if (value[name] !== undefined) return value[name]
    const pascal = name[0]!.toUpperCase() + name.slice(1)
    if (value[pascal] !== undefined) return value[pascal]
  }
}

export function requiredString(value: unknown): string {
  if (typeof value !== 'string' || !value.trim()) throw new Error('A API retornou um campo obrigatório inválido.')
  return value
}

export function optionalString(value: unknown): string | null {
  return typeof value === 'string' && value.trim() ? value : null
}
