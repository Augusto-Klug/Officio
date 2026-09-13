# Officio — frontend

Vue 3 + TypeScript + Vite + Vue Router + Axios + PrimeVue 4 (Aura).

## Executar

Requer Node.js 22.12+ (validado com 24.14) e npm.

```powershell
cd frontend
npm.cmd ci
Copy-Item .env.example .env
npm.cmd run dev
```

Abra http://localhost:5173. No Windows, `npm.cmd` evita o bloqueio de `npm.ps1` pela política de execução do PowerShell. Em outros sistemas, use `npm` normalmente.

```powershell
npm.cmd run build
npm.cmd test
npm.cmd run preview
```

O build executa `vue-tsc --noEmit` antes do Vite e gera `dist/`. O preview serve o build em http://localhost:4173; não é servidor de produção nem oferece o proxy de desenvolvimento configurado em `server.proxy`.

## API e autenticação local

Configure `Jwt__Key` no processo do backend (chave privada adequada a HS256, pelo menos 32 bytes), `ConnectionStrings__DefaultConnection` e um usuário ativo com senha válida. Use variáveis de ambiente ou user-secrets; nunca coloque segredos em `VITE_*`. Para criar o primeiro usuário em Development, consulte [o bootstrap existente](../docs/autenticacao-local.md) e configure `BootstrapUser:Email` como `admin@officio.com`, a senha local e `BootstrapUser:Role=Admin`. O TenantId é o GUID fixo do escritório. O bootstrap não atualiza a senha de usuários existentes.

Depois de configurar o ambiente do backend, execute a partir da raiz:

```powershell
dotnet run --project src/Officio.WebApi --launch-profile https
```

No frontend:

| Variável | Padrão | Responsabilidade |
| --- | --- | --- |
| `VITE_API_BASE_URL` | `/api` | Prefixo usado pelo Axios; pode ser uma URL absoluta terminada em `/api` |
| `API_PROXY_TARGET` | `https://localhost:7228` | Destino local do proxy Vite, sem `/api` |

O proxy preserva `/api`, evita CORS no desenvolvimento e aceita o certificado HTTPS local do ASP.NET. `secure: false` aplica-se somente a esse proxy de desenvolvimento. Não foi necessário alterar CORS no backend.

Mantenha em `frontend/.env`:

```dotenv
VITE_API_BASE_URL=/api
API_PROXY_TARGET=https://localhost:7228
```

Para usar o perfil HTTP do backend, altere somente `API_PROXY_TARGET=http://localhost:5131` e inicie com `--launch-profile http`. Reinicie o Vite após mudar a configuração. No padrão existente, uma URL absoluta em `VITE_API_BASE_URL` precisa terminar em `/api` (por exemplo `https://localhost:7228/api`) e passa a exigir CORS para a origem do frontend; prefira o proxy local já configurado.

Em produção, configure o servidor para entregar `index.html` nas rotas SPA e encaminhar `/api` para o backend. Alternativamente, defina uma URL absoluta em `VITE_API_BASE_URL` **antes do build** e configure CORS no servidor para a origem exata do frontend. As variáveis Vite são públicas e incorporadas ao bundle; nunca contêm segredos.

## Arquitetura

Organização híbrida: infraestrutura de frontend e componentes genéricos compartilhados; código de negócio agrupado por feature. Views coordenam interação e estado da tela, services concentram chamadas e adaptação dos DTOs, types descrevem o contrato interno. Não há repositories, use-cases ou camadas domain/application no frontend.

- `api/http.ts`: Axios, Bearer, timeout, FormData e callback de 401, sem depender do router.
- `api/errors.ts`: normaliza texto, `{ error }`, `{ erro }` e ValidationProblemDetails. Erros 500 não expõem detalhes internos.
- `api/auth.service.ts`: único local com a URL do login; converte a resposta em Session.
- `stores/session.ts`: estado reativo de autenticação em memória, persistido em sessionStorage por aba. Verifica a expiração ao restaurar e usar o token. A validação e autorização efetivas são do backend. Não existe endpoint de refresh informado; 401 exige novo login.
- `router/index.ts`: rotas lazy, proteção, título e retorno interno após login.
- `modules/prospectos/services`: único local com a URL de listagem. Converte `data/totalCount/page/pageSize/totalPages` ou `dados/totalRegistros/pagina/tamanhoPagina/totalPaginas`, camelCase ou PascalCase. `responsavel` e `responsavelNome` convergem para o mesmo campo.
- `modules/prospectos/types`: modelo e valores reais do enum de status. Valores desconhecidos permanecem visíveis com badge neutro.
- `components/base`: somente os cinco wrappers solicitados; `shared` contém StatusBadge. O mapeamento específico de prospectos fica em ProspectoStatus dentro da feature.

Não foi necessário adicionar Pinia para uma única sessão reativa; `stores/` é o ponto de extensão quando houver mais estado compartilhado. Não há interceptores que adivinham envelopes ou corrigem silenciosamente respostas inválidas.

## Comportamento e escopo

Login envia `{ email, senha }` para `POST /api/v1/autenticacao/Login`. A listagem envia `pagina`, `tamanhoPagina` (20), `status`, `busca` e, quando presente na URL, `responsavelId` para `GET /api/v1/prospectos/Listar`. Não foi inventado um endpoint de responsáveis para montar um seletor.

O login existente valida campos obrigatórios, bloqueia submits simultâneos e apresenta carregamento. O serviço normaliza o e-mail, mantém a senha intacta e mostra mensagem genérica para qualquer 401. Falhas de rede e servidor possuem mensagens amigáveis. Token, expiração e usuário são validados antes de salvar a sessão e navegar para `/prospectos`.

O guard exige autenticação nas rotas dentro de AppLayout. Sem token válido, redireciona ao login; autenticado no login, retorna a prospectos. O interceptor central envia Bearer e limpa a sessão em 401 de rota protegida. O botão Sair limpa token/usuário e sessionStorage e volta ao login, sem endpoint de logout.

A persistência em sessionStorage já existia e foi mantida como decisão temporária do MVP para sobreviver a recargas na mesma aba. Não foi introduzido localStorage, cookie, refresh token ou nova store. Não havia mock de autenticação a remover; dados simulados aparecem somente nos testes.

Busca com debounce de 350 ms, filtros e página na URL, cancelamento de requisições obsoletas, correção de página fora do intervalo, retry, loading e vazio contextual. A interface não exibe dados fictícios nem páginas de funcionalidades futuras. “Novo prospecto” está desabilitado com explicação visível.

## Validação

- Build de produção e TypeScript estrito executados com sucesso.
- Testes Vitest cobrem contratos inglês/português, PascalCase, resposta inválida versus vazio, Bearer, FormData, 401 protegido, login inválido e normalização de erros.
- Teste HTTP real pelo proxy: login com o e-mail configurado nos user-secrets (`admin@officio.test`) retornou JWT com perfil Admin; prospectos com Bearer retornou 200 e sem Bearer retornou 401. O e-mail inicialmente informado (`admin@officio.com`) retornou 401. A senha não foi gravada no frontend e usuários existentes não foram alterados.
- 19 testes Vitest aprovados, incluindo login válido/inválido, falhas de rede/servidor, resposta inesperada, restauração de sessão, expiração e logout local.
- Inspeção visual em navegador não executada: nenhum navegador estava conectado à sessão. Responsividade e acessibilidade foram revisadas no código, sem afirmar validação visual.
- Não há script de lint configurado. A checagem TypeScript faz parte do build; não foi adicionada ferramenta de lint nesta tarefa.

Veja [DESIGN.md](DESIGN.md) para os componentes e tokens, [PRODUCT.md](PRODUCT.md) para escopo e [REVIEW.md](REVIEW.md) para a revisão estática das skills.

## Arquivos criados

```text
frontend/
├── .env.example
├── .gitignore
├── DESIGN.md
├── PRODUCT.md
├── README.md
├── REVIEW.md
├── index.html
├── package.json
├── package-lock.json
├── tsconfig.json
├── vite.config.ts
├── public/
│   └── favicon.svg
└── src/
    ├── api/
    │   ├── auth.service.ts
    │   ├── contracts.ts
    │   ├── errors.ts
    │   ├── http.ts
    │   └── http.test.ts
    ├── assets/
    │   └── styles.css
    ├── components/
    │   ├── base/
    │   │   ├── BaseBadge.vue
    │   │   ├── BaseButton.vue
    │   │   ├── BaseCard.vue
    │   │   ├── BaseInput.vue
    │   │   └── BaseSelect.vue
    │   ├── layout/
    │   │   ├── AppHeader.vue
    │   │   ├── AppLayout.vue
    │   │   ├── AppSidebar.vue
    │   │   ├── PageContainer.vue
    │   │   └── PageHeader.vue
    │   └── shared/
    │       └── StatusBadge.vue
    ├── modules/
    │   └── prospectos/
    │       ├── components/ProspectoStatus.vue
    │       ├── services/prospectos.service.ts
    │       ├── services/prospectos.service.test.ts
    │       ├── types/prospecto.ts
    │       └── views/ProspectosView.vue
    ├── router/index.ts
    ├── stores/session.ts
    ├── utils/format.ts
    ├── views/auth/LoginView.vue
    ├── App.vue
    └── main.ts
```

`node_modules/` e `dist/` são artefatos locais ignorados pelo Git. Nenhum commit ou push foi feito.
