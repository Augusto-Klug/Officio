# Revisão estática da primeira entrega

Escopo: fontes do frontend; sem evidência visual de navegador. O runtime de navegador retornou lista vazia de navegadores. O carregador Impeccable não encontrou seu binário. Essas limitações não equivalem a aprovação visual.

## web-design-guidelines

Referência consultada: https://raw.githubusercontent.com/vercel-labs/web-interface-guidelines/main/command.md

- `src/components/base/BaseInput.vue`: label associado, descrições de ajuda/erro e atributos nativos repassados ao input.
- `src/components/base/BaseSelect.vue`: label associado; seleção e teclado delegados ao PrimeVue.
- `src/components/layout/AppLayout.vue`: skip link, landmarks e Drawer do PrimeVue para gerenciamento de foco.
- `src/modules/prospectos/views/ProspectosView.vue`: busca e paginação na URL, estados textuais, erro com recuperação, dados sem HTML não confiável.
- `src/assets/styles.css`: foco visível, quebra de texto, números tabulares, scroll local na tabela, movimento reduzido.

Verificação pendente no navegador: foco ao abrir/fechar Drawer, contraste dos estilos computados de todos os estados PrimeVue, tabela com nomes longos e navegação a 375 px/1440 px.

## emil-design-eng / review-animations

| Before | After | Why |
| --- | --- | --- |
| Transições padrão dos controles e overlays PrimeVue | Transições desativadas para os controles frequentes | Resposta imediata, inclusive por teclado |
| Indicador girando com preferência de movimento reduzido | Ícone estático e texto preservado com prefers-reduced-motion | Comunicar espera sem movimento |

**Approve — revisão estática de movimento.** Não há entrada de rotas, linhas ou filtros. A única animação própria é a rotação de transform no indicador de espera: linear, 1 s por volta, enquanto a requisição estiver pendente. A duração indica um ciclo de atividade, não atraso de uma interação. Sem medição de fluidez em navegador.

## find-animation-opportunities

Nenhuma oportunidade adicional passou pelo filtro de utilidade.

| Local | Estado atual | Finalidade | Frequência | Movimento sugerido |
| --- | --- | --- | --- | --- |
| `src/assets/styles.css` — `.loading-spinner` | Indicação de espera já implementada | Indicação de estado | Durante requisições | Manter transform rotate(360deg), linear, 1 s; estático com movimento reduzido |

Candidatos rejeitados:

- `src/router/index.ts`: transição entre rotas — navegação frequente e iniciada por teclado.
- `src/modules/prospectos/views/ProspectosView.vue`: entrada escalonada de linhas — interfere na leitura de dados funcionais.
- `src/components/layout/AppLayout.vue`: animação do menu móvel — navegação recorrente; abertura imediata é suficiente.

Veredito: nenhum movimento extra necessário. Não há proposta pendente de implementação.

## animate / impeccable

Gate: spinner aprovado pela finalidade de indicar solicitação em andamento; demais animações rejeitadas pela frequência e pelo caráter administrativo. CSS com transform e linear, sem dependência adicional.

Modo Operate aplicado: hierarquia de página, filtros próximos dos dados, ações honestas, superfície sem métricas fictícias, sistema documentado em DESIGN.md. Não foi executada revisão visual independente nem o detector binário da skill.
