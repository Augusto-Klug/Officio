# Officio

Officio e uma API backend para apoiar fluxos operacionais de empresas de servicos, com foco em prospeccao de clientes, orcamentos, documentos, contratos e assinatura digital.

O repositorio contem uma solucao .NET organizada em projetos separados por responsabilidade. A aplicacao principal expoe endpoints HTTP, executa regras de negocio por meio de comandos e consultas, persiste dados em SQL Server e integra servicos externos para armazenamento, OCR, geracao de PDF e assinatura de documentos.

## Objetivo do Projeto

O sistema centraliza etapas comuns do ciclo comercial e contratual:

- cadastro e acompanhamento de prospectos;
- conversao de prospectos em clientes;
- criacao, atualizacao, aprovacao e recusa de orcamentos;
- manutencao de catalogo de itens de orcamento;
- envio e validacao de documentos;
- conversao de orcamentos aprovados em contratos;
- geracao de PDF de contrato;
- envio de contratos para assinatura digital;
- autenticacao de usuarios via JWT.

## Tecnologias Principais

- .NET 10
- ASP.NET Core Web API
- Entity Framework Core
- SQL Server
- MediatR
- FluentValidation
- xUnit
- QuestPDF
- Azure Blob Storage
- Azure Form Recognizer / Document Intelligence
- Assinatura digital

## Estrutura do Repositorio

```text
.
|-- Officio.WebApi
|-- Officio.ViewModels
|-- Officio.Dominio
|-- Officio.Dominio.Mediador
|-- Officio.Dominio.Servicos
|-- Officio.Infra.BancoDeDados
|-- Officio.Infraestrutura
|-- Officio.Testes
|-- docs
|-- Officio.slnx
`-- README.md
```

### Officio.WebApi

Projeto de entrada da aplicacao. Contem os controladores HTTP, configuracoes de autenticacao, middlewares, conversores de entrada e registro de dependencias da API.

Principais areas expostas:

- autenticacao;
- prospectos;
- clientes;
- orcamentos;
- documentos;
- contratos.

### Officio.ViewModels

Contem os modelos recebidos pela API nas requisicoes HTTP. Esses modelos representam o formato de entrada externo e sao convertidos para contratos internos antes de chegar aos comandos e consultas da aplicacao.

### Officio.Dominio

Contem os elementos centrais do dominio, como entidades, enumeradores, contratos de entrada e saida, interfaces de repositorios, interfaces de servicos e eventos de dominio.

Entre as entidades principais estao:

- Empresa;
- Usuario;
- Prospecto;
- Cliente;
- Orcamento;
- Contrato;
- Documento;
- Assinatura;
- Notificacao;
- RegistroAuditoria.

### Officio.Dominio.Mediador

Camada responsavel pelos comandos, consultas, handlers, validadores e comportamentos do MediatR. Ela organiza os casos de uso da aplicacao e faz a ponte entre a API e os servicos de dominio.

### Officio.Dominio.Servicos

Implementa regras de negocio e servicos internos relacionados a prospectos, orcamentos, documentos, contratos e estimativa de qualidade de documentos.

### Officio.Infra.BancoDeDados

Contem o contexto do Entity Framework Core, mapeamentos, repositorios e migrations. O banco configurado para a aplicacao e SQL Server.

### Officio.Infraestrutura

Contem integracoes tecnicas e servicos externos, incluindo:

- autenticacao JWT;
- identificacao de usuario e empresa atual;
- armazenamento em Azure Blob Storage;
- OCR com Azure Form Recognizer / Document Intelligence;
- geracao de PDFs com QuestPDF;
- assinatura digital com Autentique;
- envio de e-mail.

### Officio.Testes

Projeto de testes automatizados com xUnit. Inclui testes de regras de dominio, rotas, conversores, contratos e comportamentos relacionados aos principais fluxos da aplicacao.

## Requisitos para Execucao

- SDK do .NET 10 instalado;
- SQL Server acessivel;
- string de conexao configurada;
- chave JWT configurada;
- credenciais dos servicos externos quando os fluxos correspondentes forem utilizados.

## Configuracao

A aplicacao utiliza configuracoes do ASP.NET Core, podendo ser informadas por `appsettings`, variaveis de ambiente ou user secrets.

Configuracoes relevantes:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=...;Database=...;User Id=...;Password=...;",
    "AzureBlobStorage": "DefaultEndpointsProtocol=...;"
  },
  "Jwt": {
    "Key": "chave-secreta-com-tamanho-adequado",
    "Issuer": "Officio",
    "Audience": "Officio"
  },
  "AzureOcr": {
    "Endpoint": "https://...",
    "Key": "..."
  },
  "Pdf": {
  },
  "Autentique": {
    "BaseUrl": "https://api.autentique.com.br/v2/graphql",
    "ApiToken": "",
    "WebhookSecret": "",
    "OrganizationId": "",
    "FolderId": ""
  }
}
```

Valores sensiveis nao devem ser versionados no repositorio.

## Como Restaurar, Compilar e Testar

Restaurar dependencias:

```bash
dotnet restore Officio.slnx
```

Compilar a solucao:

```bash
dotnet build Officio.slnx
```

Executar testes:

```bash
dotnet test Officio.Testes/Officio.Testes.csproj
```

## Como Executar a API

Executar o projeto da Web API:

```bash
dotnet run --project Officio.WebApi/Officio.WebApi.csproj
```

Em ambiente de desenvolvimento, a API utiliza os perfis definidos em `Officio.WebApi/Properties/launchSettings.json`:

- HTTP: `http://localhost:5131`
- HTTPS: `https://localhost:7228`

Quando executada em ambiente de desenvolvimento, a aplicacao tambem mapeia a especificacao OpenAPI.

## Banco de Dados

O acesso a dados usa Entity Framework Core com SQL Server. As migrations ficam no projeto `Officio.Infra.BancoDeDados`.

As migrations versionadas sao incrementais e pressupõem um schema base já criado. Para uma instalação limpa, configure `ConnectionStrings__DefaultConnection` para apontar a um banco vazio (veja o exemplo comentado em `.envexample`) e execute a partir da pasta `src`:

```bash
dotnet run --project Officio.WebApi -- --bootstrap-schema
```

Esse comando cria o schema pelo modelo EF Core atual e registra as migrations existentes como baseline. Para aplicar migrations pendentes em uma base existente, faça backup e execute:

```bash
dotnet run --project Officio.WebApi -- --apply-migrations
```

A inicializacao normal da API em producao nao aplica migrations automaticamente.

## Observacoes para Leitores Externos

Este repositorio representa o backend da aplicacao Officio. Ele nao contem uma interface frontend completa. A comunicacao externa acontece por endpoints HTTP expostos pela Web API.

A organizacao geral segue uma separacao entre API, dominio, casos de uso, persistencia e integracoes externas. Essa divisao facilita manutencao e testes sem exigir que o leitor conheca todos os detalhes internos para compreender o proposito do projeto.
