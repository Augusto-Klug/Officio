// @vitest-environment jsdom
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { createApp, nextTick, type App } from 'vue'
import PrimeVue from 'primevue/config'
import { createMemoryHistory, createRouter, RouterView, type Router } from 'vue-router'
import { AxiosError, AxiosHeaders } from 'axios'
import NovoProspectoView from './NovoProspectoView.vue'
import ProspectosView from './ProspectosView.vue'
import { http } from '../../../api/http'
import { consumirConfirmacaoCadastro } from '../state/confirmacao'

let app: App
let root: HTMLDivElement
let router: Router
const settle = async () => { await new Promise(resolve => setTimeout(resolve, 0)); await nextTick() }
function input(id: string, value: string) {
  const element = root.querySelector<HTMLInputElement>(`#${id}`)!
  element.value = value
  element.dispatchEvent(new Event('input', { bubbles: true }))
}
function button(text: string): HTMLButtonElement {
  return [...root.querySelectorAll('button')].find(element => element.textContent?.includes(text))!
}
async function submit() {
  root.querySelector('form')!.dispatchEvent(new Event('submit', { bubbles: true, cancelable: true }))
  await settle()
}

beforeEach(async () => {
  vi.stubGlobal('matchMedia', (query: string) => ({
    matches: false,
    media: query,
    onchange: null,
    addListener: vi.fn(),
    removeListener: vi.fn(),
    addEventListener: vi.fn(),
    removeEventListener: vi.fn(),
    dispatchEvent: () => true,
  }))
  consumirConfirmacaoCadastro()
  root = document.createElement('div')
  document.body.append(root)
  router = createRouter({ history: createMemoryHistory(), routes: [
    { path: '/prospectos/novo', name: 'novo-prospecto', component: NovoProspectoView },
    { path: '/prospectos', name: 'prospectos', component: ProspectosView },
  ] })
  await router.push('/prospectos/novo')
  await router.isReady()
  app = createApp(RouterView).use(router).use(PrimeVue, { unstyled: true })
  app.mount(root)
  await nextTick()
})
afterEach(() => {
  app.unmount()
  root.remove()
  delete http.defaults.adapter
  consumirConfirmacaoCadastro()
  vi.restoreAllMocks()
  vi.unstubAllGlobals()
})

describe('Novo prospecto', () => {
  it('valida razão social e CNPJ, informa erros e foca o primeiro campo inválido', async () => {
    const adapter = vi.fn()
    http.defaults.adapter = adapter
    await submit()
    expect(root.textContent).toContain('Informe a razão social.')
    expect(document.activeElement?.id).toBe('razaoSocial')
    input('razaoSocial', 'Empresa')
    input('cnpj', '123')
    await submit()
    expect(root.textContent).toContain('Informe os 14 dígitos')
    expect(document.activeElement?.id).toBe('cnpj')
    expect(adapter).not.toHaveBeenCalled()
  })

  it('adiciona e remove sócios com foco e mantém os dados das demais linhas', async () => {
    button('Adicionar sócio').click()
    await nextTick()
    expect(document.activeElement?.id).toBe('socio-0-nome')
    input('socio-0-nome', 'Maria')
    button('Adicionar sócio').click()
    await nextTick()
    input('socio-1-nome', 'Ana')
    button('Remover sócio').click()
    await nextTick()
    expect(root.querySelector('#socio-0-nome')).toBeNull()
    expect(root.querySelector<HTMLInputElement>('#socio-1-nome')?.value).toBe('Ana')
    expect(document.activeElement).toBe(button('Adicionar sócio'))
  })

  it('exige nome e participação das linhas adicionadas e permite CPF vazio', async () => {
    http.defaults.adapter = vi.fn()
    input('razaoSocial', 'Empresa')
    button('Adicionar sócio').click()
    await nextTick()
    await submit()
    expect(root.textContent).toContain('Informe o nome do sócio.')
    expect(root.textContent).toContain('Informe uma participação')
    expect(root.textContent).not.toContain('Informe os 11 dígitos')
    expect(http.defaults.adapter).not.toHaveBeenCalled()
  })

  it('envia sócios, bloqueia POST duplicado e retorna à listagem atualizada com confirmação', async () => {
    let liberar!: () => void
    const pendente = new Promise<void>(resolve => { liberar = resolve })
    const chamadas: string[] = []
    http.defaults.adapter = async config => {
      chamadas.push(`${config.method} ${config.url}`)
      if (config.method === 'post') {
        expect(JSON.parse(config.data)).toEqual({ razaoSocial: 'Nova Empresa', cnpj: '12.345.678/0001-90', regimeTributario: null,
          socios: [{ nome: 'Maria', cpf: '123.456.789-00', participacao: 50.5 }] })
        await pendente
        return { data: { id: 'novo', razaoSocial: 'Nova Empresa' }, status: 201, statusText: 'Created', headers: new AxiosHeaders(), config }
      }
      expect(config.url).toBe('/v1/prospectos/Listar')
      expect(config.params).toMatchObject({ pagina: 1, busca: undefined, status: undefined })
      return { data: { data: [{ id: 'novo', razaoSocial: 'Nova Empresa', status: 'EmNegociacao' }], totalCount: 1, page: 1, pageSize: 20, totalPages: 1 }, status: 200, statusText: 'OK', headers: new AxiosHeaders(), config }
    }
    input('razaoSocial', ' Nova Empresa ')
    input('cnpj', '12345678000190')
    button('Adicionar sócio').click()
    await nextTick()
    input('socio-0-nome', ' Maria ')
    input('socio-0-cpf', '12345678900')
    input('socio-0-participacao', '50,5')
    await submit()
    expect(button('Cadastrando').disabled).toBe(true)
    expect(root.querySelector('form')?.getAttribute('aria-busy')).toBe('true')
    await submit()
    expect(chamadas.filter(item => item.startsWith('post'))).toHaveLength(1)
    liberar()
    await vi.waitFor(() => expect(router.currentRoute.value.path).toBe('/prospectos'))
    await vi.waitFor(() => expect(root.textContent).toContain('Nova Empresa'))
    expect(root.textContent).toContain('Prospecto cadastrado com sucesso.')
    expect(chamadas).toContain('get /v1/prospectos/Listar')
    button('Novo prospecto').click()
    await settle()
    expect(router.currentRoute.value.path).toBe('/prospectos/novo')
  })

  it.each(['negocio', 'rede'])('preserva formulário e sócios após erro de %s, permitindo nova tentativa', async tipo => {
    http.defaults.adapter = async config => {
      if (tipo === 'rede') throw new AxiosError('offline', 'ERR_NETWORK')
      throw new AxiosError('Bad Request', 'ERR_BAD_REQUEST', config, undefined,
        { data: 'Já existe um prospecto ou cliente com este CNPJ.', status: 400, statusText: 'Bad Request', headers: new AxiosHeaders(), config })
    }
    input('razaoSocial', 'Empresa mantida')
    input('cnpj', '12.345.678/0001-90')
    button('Adicionar sócio').click()
    await nextTick()
    input('socio-0-nome', 'Maria')
    input('socio-0-participacao', '100')
    await submit()
    expect(router.currentRoute.value.path).toBe('/prospectos/novo')
    expect(root.querySelector<HTMLInputElement>('#razaoSocial')?.value).toBe('Empresa mantida')
    expect(root.querySelector<HTMLInputElement>('#socio-0-nome')?.value).toBe('Maria')
    expect(root.querySelector<HTMLInputElement>('#cnpj')?.value).toBe('12.345.678/0001-90')
    expect(root.textContent).toContain(tipo === 'rede' ? 'Não foi possível conectar ao servidor.' : 'Já existe um prospecto')
    expect(button('Cadastrar prospecto').disabled).toBe(false)
    expect(document.activeElement?.getAttribute('tabindex')).toBe('-1')
  })

  it('permite reenviar os mesmos dados depois de uma falha de rede', async () => {
    let tentativas = 0
    http.defaults.adapter = async config => {
      if (config.method === 'post') {
        tentativas++
        expect(JSON.parse(config.data)).toEqual({ razaoSocial: 'Empresa', cnpj: null, regimeTributario: null, socios: [] })
        if (tentativas === 1) throw new AxiosError('offline', 'ERR_NETWORK')
        return { data: {}, status: 201, statusText: 'Created', headers: new AxiosHeaders(), config }
      }
      return { data: { data: [], totalCount: 0, page: 1, pageSize: 20, totalPages: 0 }, status: 200, statusText: 'OK', headers: new AxiosHeaders(), config }
    }
    input('razaoSocial', 'Empresa')
    await submit()
    expect(root.textContent).toContain('Não foi possível conectar ao servidor.')
    await submit()
    await vi.waitFor(() => expect(router.currentRoute.value.path).toBe('/prospectos'))
    expect(tentativas).toBe(2)
    expect(root.textContent).toContain('Prospecto cadastrado com sucesso.')
  })

  it('impede outro cadastro se a navegação após sucesso for bloqueada', async () => {
    const adapter = vi.fn(async config => ({ data: {}, status: 201, statusText: 'Created', headers: new AxiosHeaders(), config }))
    http.defaults.adapter = adapter
    const removerGuarda = router.beforeEach(() => false)
    try {
      input('razaoSocial', 'Empresa')
      await submit()
      expect(router.currentRoute.value.path).toBe('/prospectos/novo')
      expect(root.textContent).toContain('Prospecto cadastrado com sucesso.')
      expect(button('Cadastrar prospecto').disabled).toBe(true)
      await submit()
      expect(adapter).toHaveBeenCalledTimes(1)
    } finally {
      removerGuarda()
    }
  })
  it('envia o regime selecionado como enum numérico e preserva a seleção em erro', async () => {
    const adapter = vi.fn(async config => {
      throw new AxiosError('offline', 'ERR_NETWORK')
    })
    http.defaults.adapter = adapter
    input('razaoSocial', 'Empresa')
    root.querySelector<HTMLElement>('[role="combobox"]')!.click()
    await settle()
    const opcao = [...document.querySelectorAll<HTMLElement>('[role="option"]')]
      .find(element => element.textContent?.includes('Simples Nacional'))!
    opcao.dispatchEvent(new MouseEvent('mousedown', { bubbles: true }))
    await settle()
    await submit()
    expect(adapter).toHaveBeenCalledTimes(1)
    expect(JSON.parse(adapter.mock.calls[0]![0].data).regimeTributario).toBe(0)
    expect(root.querySelector('[role="combobox"]')?.textContent).toContain('Simples Nacional')
  })
  it('cancela sem enviar cadastro', async () => {
    const adapter = vi.fn(async config => ({ data: { data: [], totalCount: 0, page: 1, pageSize: 20, totalPages: 0 }, status: 200, statusText: 'OK', headers: new AxiosHeaders(), config }))
    http.defaults.adapter = adapter
    input('razaoSocial', 'Não salvar')
    button('Cancelar').click()
    await settle()
    expect(router.currentRoute.value.path).toBe('/prospectos')
    expect(adapter.mock.calls.every(([config]) => config.method === 'get')).toBe(true)
  })
})
