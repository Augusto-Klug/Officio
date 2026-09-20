import { createRouter, createWebHistory } from 'vue-router'
import { accessToken, defaultAuthenticatedRoute, hasRole } from '../stores/session'
import { onUnauthorized } from '../api/http'

export const router = createRouter({
  history: createWebHistory(),
  routes: [
    { path: '/login', name: 'login', component: () => import('../views/auth/LoginView.vue'), meta: { title: 'Entrar' } },
    { path: '/ativar-conta', name: 'ativar-conta', component: () => import('../views/auth/AtivarContaView.vue'), meta: { title: 'Ativar conta' } },
    { path: '/esqueci-senha', name: 'esqueci-senha', component: () => import('../views/auth/EsqueciSenhaView.vue'), meta: { title: 'Recuperar senha' } },
    { path: '/redefinir-senha', name: 'redefinir-senha', component: () => import('../views/auth/RedefinirSenhaView.vue'), meta: { title: 'Redefinir senha' } },
    { path: '/ficha-admissao/:token', name: 'ficha-admissao', component: () => import('../modules/admissoes/views/FichaAdmissaoView.vue'), meta: { title: 'Ficha de admissão' } },
    {
      path: '/', component: () => import('../components/layout/AppLayout.vue'), meta: { requiresAuth: true },
      children: [
        { path: '', redirect: () => defaultAuthenticatedRoute() },
        { path: 'dashboard', name: 'dashboard', component: () => import('../modules/dashboard/views/DashboardView.vue'), meta: { title: 'Visao geral' } },
        { path: 'prospectos', name: 'prospectos', component: () => import('../modules/prospectos/views/ProspectosView.vue'), meta: { title: 'Prospectos' } },
        { path: 'prospectos/:prospectoId/orcamentos', name: 'orcamentos-prospecto', component: () => import('../modules/orcamentos/views/OrcamentosView.vue'), meta: { title: 'Orçamentos' } },
        { path: 'clientes', name: 'clientes', component: () => import('../modules/clientes/views/ClientesView.vue'), meta: { title: 'Clientes' } },
        { path: 'admissoes', name: 'admissoes', component: () => import('../modules/admissoes/views/AdmissoesView.vue'), meta: { title: 'Admissões' } },
        { path: 'contratos', name: 'contratos', component: () => import('../modules/contratos/views/ContratosView.vue'), meta: { title: 'Contratos' } },
        { path: 'faturamentos', name: 'faturamentos', component: () => import('../modules/faturamentos/views/FaturamentosView.vue'), meta: { title: 'Faturamento mensal' } },
        { path: 'acessos', name: 'acessos', component: () => import('../modules/acessos/views/AcessosView.vue'), meta: { title: 'Usuários e acessos', roles: ['Admin'] } },
        { path: 'admin/empresas', name: 'admin-empresas', component: () => import('../modules/administracao/views/EmpresasView.vue'), meta: { title: 'Empresas', roles: ['PlatformAdmin'] } },
      ],
    },
    { path: '/:pathMatch(.*)*', redirect: () => defaultAuthenticatedRoute() },
  ],
})

router.beforeEach((to) => {
  const token = accessToken()
  if (to.meta.requiresAuth && !token) return { name: 'login', query: { redirect: to.fullPath } }
  const roles = to.meta.roles as string[] | undefined
  if (token && roles?.length && !roles.some(hasRole)) return defaultAuthenticatedRoute()
  if (to.name === 'login' && token) return defaultAuthenticatedRoute()
})
router.afterEach((to) => { document.title = `${String(to.meta.title || 'Officio')} | Officio` })
onUnauthorized(() => {
  if (router.currentRoute.value.name !== 'login') {
    void router.replace({ name: 'login', query: { expired: '1', redirect: router.currentRoute.value.fullPath } })
  }
})

export function safeRedirect(value: unknown): string {
  // Restringe o retorno pós-login às áreas autenticadas conhecidas.
  if (typeof value !== 'string' || !/^\/(?:dashboard|prospectos|clientes|admissoes|contratos|faturamentos|acessos|admin\/empresas)(?:\/|\?|$)/.test(value)) return defaultAuthenticatedRoute()
  if (value.startsWith('/admin/empresas') && !hasRole('PlatformAdmin')) return defaultAuthenticatedRoute()
  if (value.startsWith('/acessos') && !hasRole('Admin')) return defaultAuthenticatedRoute()
  return value
}
