const dateFormat = new Intl.DateTimeFormat('pt-BR')
const currencyFormat = new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' })
export function formatDate(value: string | null): string {
  if (!value || Number.isNaN(Date.parse(value))) return 'Não informado'
  return dateFormat.format(new Date(value))
}
export function formatCnpj(value: string | null): string {
  if (!value) return 'CNPJ não informado'
  return value.replace(/^(\d{2})(\d{3})(\d{3})(\d{4})(\d{2})$/, '$1.$2.$3/$4-$5')
}
export function formatCurrency(value: number): string {
  return currencyFormat.format(value)
}
