# Design System inicial do Officio

Modo da listagem: Operate. A tarefa principal é localizar e acompanhar prospectos. Estrutura: navegação lateral, cabeçalho, título com ação, filtros, tabela e paginação.

## Fundamentos

- Superfície clara para leitura prolongada; sidebar verde escura `#183d35`, ação verde `#205740`, texto `#253630`, texto secundário `#617169`, divisórias `#dfe6e2`.
- Tipografia do sistema (Segoe UI no Windows), sem downloads externos; título 26–32 px, conteúdo 13–14 px e números tabulares. Texto longo quebra nas células.
- Escala de espaçamento: 4, 8, 12, 16, 24, 32 e 40 px. Cartões com borda, raio 12 px e sem sombra; controles com raio 6 px.
- Tokens em `src/assets/styles.css`; cores semânticas do PrimeVue em `src/main.ts` (Aura). Não criar uma segunda biblioteca de controles.

## Componentes e responsabilidades

| Componente | Uso |
| --- | --- |
| BaseButton | Ações primary, secondary ou text; tipo button por padrão; loading e disabled |
| BaseInput | Campo textual com label, hint, erro e atributos nativos repassados ao input |
| BaseSelect | Seleção simples com label, opção de limpar e valores string |
| BaseCard | Superfície com borda e espaçamento padrão |
| BaseBadge | Etiqueta semântica sobre Tag |
| StatusBadge | Indicador textual com ponto; não depende apenas de cor |
| AppSidebar / AppHeader | Navegação, identificação e saída |
| PageContainer / PageHeader | Largura, margem, hierarquia e ações de página |
| ProspectoStatus | Tradução do enum de prospectos para texto e severidade; permanece na feature |

DataTable, Column, Paginator, Drawer e Message são usados diretamente: nenhum wrapper sem regra de reutilização.

## Estados e acessibilidade

Carregamento textual anunciado, erro com tentativa novamente, vazio contextual e botão de limpar filtros. Filtros e página refletem a URL. Busca tem debounce de 350 ms e requisições obsoletas são canceladas. Não reutilizar dados de outra consulta durante loading ou erro.

Sidebar oculta abaixo de 760 px e substituída por Drawer com foco gerenciado pelo PrimeVue. A tabela preserva colunas e permite rolagem horizontal dentro da superfície. Skip link, foco visível, labels e nomes acessíveis em controles.

## Movimento

## Cadastro de prospecto

Formulário em uma única superfície, com dados do prospecto, sócios opcionais e ações ao final. Duas colunas no desktop e uma no mobile; sócios agrupados por fieldset com legenda. Reutiliza os controles base e o PageContainer do layout. Campos obrigatórios identificados, erros associados aos inputs e foco no primeiro inválido. Adicionar sócio foca seu nome; remover devolve o foco à ação de adicionar. Envio bloqueia nova submissão, erro preserva dados e sucesso retorna à listagem sem filtros, com confirmação.

## Movimento

Rejeitados: transição de rota (alta frequência), entrada de linhas (dados em leitura), animação de filtros/paginação (interações frequentes e por teclado). Controles e Drawer respondem imediatamente.

Apenas o indicador de espera usa rotação CSS de transform, linear, 1 s por volta enquanto há uma solicitação; finalidade: indicar trabalho em andamento. Prefers-reduced-motion remove a rotação, mantendo ícone e texto. Não há biblioteca adicional de animação.

## Referências aplicadas

Skills locais web-design-guidelines, impeccable, emil-design-eng, animate, find-animation-opportunities e review-animations. Diretrizes de acessibilidade: https://raw.githubusercontent.com/vercel-labs/web-interface-guidelines/main/command.md. O executável de contexto do Impeccable estava indisponível; a implementação seguiu o brief e as referências textuais.
