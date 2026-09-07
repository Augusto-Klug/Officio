# Officio

Sistema administrativo e contábil para a rotina de um escritório. Nesta entrega, o usuário autentica e consulta prospectos, buscando por razão social/CNPJ, filtrando por status e navegando na paginação.

O frontend consome o backend ASP.NET existente, com JWT Bearer e isolamento por empresa implementado pelo servidor. Não presume permissões a partir da interface.

Escopo: login, shell e listagem. Cadastro, detalhe, conversão, orçamento, contrato, OCR e dashboard estão fora desta entrega. O botão Novo prospecto comunica a indisponibilidade do cadastro sem simular uma operação.

Prioridades: clareza, consistência, acessibilidade, resposta imediata e manutenção simples. Interface administrativa para uso recorrente em desktop, também utilizável em telas pequenas. Sem métricas ou dados fictícios na aplicação.
