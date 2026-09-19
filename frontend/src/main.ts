import { createApp } from 'vue'
import PrimeVue from 'primevue/config'
import Aura from '@primeuix/themes/aura'
import { definePreset } from '@primeuix/themes'
import App from './App.vue'
import { router } from './router'
import 'primeicons/primeicons.css'
import './assets/styles.css'

const Officio = definePreset(Aura, {
  semantic: {
    primary: { 50: '#f0f8f4', 100: '#daeee3', 200: '#b7dcc8', 300: '#88c4a7', 400: '#55a783', 500: '#328663', 600: '#256d50', 700: '#205740', 800: '#1b4635', 900: '#183d35', 950: '#09251e' },
  },
})

createApp(App).use(PrimeVue, {
  theme: { preset: Officio, options: { darkModeSelector: false } },
  locale: {
    emptyMessage: 'Nenhum resultado encontrado', emptySearchMessage: 'Nenhum resultado encontrado',
    searchMessage: '{0} resultados disponíveis', selectionMessage: '{0} itens selecionados', emptySelectionMessage: 'Nenhum item selecionado',
    aria: { firstPageLabel: 'Primeira página', lastPageLabel: 'Última página', nextPageLabel: 'Próxima página', previousPageLabel: 'Página anterior', pageLabel: 'Página {page}', close: 'Fechar', selectLabel: 'Selecionar', unselectLabel: 'Desmarcar' },
  },
}).use(router).mount('#app')
