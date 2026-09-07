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

## API e bloqueio encontrado

O backend foi preservado integralmente. A tentativa de executar `dotnet run --project src/Officio.WebApi --launch-profile https` compilou o projeto, mas encerrou com:

> A configuracao 'Jwt:Key' e obrigatoria para autenticacao JWT.

Configure `Jwt__Key` no ambiente do processo do backend, com uma chave secreta adequada a HS256 (pelo menos 32 bytes), ou utilize o mecanismo de secrets do ASP.NET. Não coloque a chave em variáveis `VITE_*`, no frontend ou no Git. O servidor também depende de `ConnectionStrings:DefaultConnection` e de um usuário existente com senha hash válida. Banco e credenciais reais não foram validados nesta sessão porque a inicialização parou na chave JWT. Nenhum usuário, dado ou segredo foi criado.

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

Busca com debounce de 350 ms, filtros e página na URL, cancelamento de requisições obsoletas, correção de página fora do intervalo, retry, loading e vazio contextual. A interface não exibe dados fictícios nem páginas de funcionalidades futuras. “Novo prospecto” está desabilitado com explicação visível.

## Validação

- Build de produção e TypeScript estrito executados com sucesso.
- Testes Vitest cobrem contratos inglês/português, PascalCase, resposta inválida versus vazio, Bearer, FormData, 401 protegido, login inválido e normalização de erros.
- Não foi possível validar login/listagem ponta a ponta: backend não inicializou sem `Jwt:Key`.
- Inspeção visual em navegador não executada: nenhum navegador estava conectado à sessão. Responsividade e acessibilidade foram revisadas no código, sem afirmar validação visual.
- Dependências instaladas com lockfile; instalação reportou zero vulnerabilidades.

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
