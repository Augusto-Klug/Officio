import { createRouter, createWebHistory } from 'vue-router'
import { accessToken } from '../stores/session'
import { onUnauthorized } from '../api/http'

export const router = createRouter({
  history: createWebHistory(),
  routes: [
    { path: '/login', name: 'login', component: () => import('../views/auth/LoginView.vue'), meta: { title: 'Entrar' } },
    {
      path: '/', component: () => import('../components/layout/AppLayout.vue'), meta: { requiresAuth: true },
      children: [
        { path: '', redirect: '/prospectos' },
        { path: 'prospectos', name: 'prospectos', component: () => import('../modules/prospectos/views/ProspectosView.vue'), meta: { title: 'Prospectos' } },
      ],
    },
    { path: '/:pathMatch(.*)*', redirect: '/prospectos' },
  ],
})

router.beforeEach((to) => {
  const token = accessToken()
  if (to.meta.requiresAuth && !token) return { name: 'login', query: { redirect: to.fullPath } }
  if (to.name === 'login' && token) return { name: 'prospectos' }
})
router.afterEach((to) => { document.title = `${String(to.meta.title || 'Officio')} | Officio` })
onUnauthorized(() => {
  if (router.currentRoute.value.name !== 'login') {
    void router.replace({ name: 'login', query: { expired: '1', redirect: router.currentRoute.value.fullPath } })
  }
})

export function safeRedirect(value: unknown): string {
  // Nesta entrega, a única rota interna de retorno é a listagem.
  return typeof value === 'string' && /^\/prospectos(?:\?|$)/.test(value) ? value : '/prospectos'
}
