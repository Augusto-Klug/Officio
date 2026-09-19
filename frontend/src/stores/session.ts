import { computed, readonly, ref } from 'vue'

export interface Session {
  token: string
  expiraEm: string
  usuario: { id: string; nome: string; email: string; perfil: string }
}

const key = 'officio.session'
function restore(): Session | null {
  try {
    const value = JSON.parse(sessionStorage.getItem(key) || 'null') as Session | null
    if (value && typeof value.token === 'string' && value.usuario?.nome && Date.parse(value.expiraEm) > Date.now()) return value
    sessionStorage.removeItem(key)
  } catch { /* Armazenamento indisponível ou sessão inválida: iniciar sem autenticação. */ }
  return null
}

const current = ref<Session | null>(restore())
export const session = readonly(current)
export const authenticated = computed(() => !!current.value)

export function setSession(value: Session) {
  current.value = value
  try { sessionStorage.setItem(key, JSON.stringify(value)) } catch { /* A sessão continua em memória. */ }
}

export function clearSession() {
  current.value = null
  try { sessionStorage.removeItem(key) } catch { /* Sem persistência disponível. */ }
}

export function accessToken(): string | null {
  if (current.value && Date.parse(current.value.expiraEm) <= Date.now()) clearSession()
  return current.value?.token || null
}
