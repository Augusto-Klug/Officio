# Officio

Sistema administrativo e contábil para a rotina de um escritório. Nesta entrega, o usuário autentica, cadastra prospectos com sócios opcionais e consulta a listagem, buscando por razão social/CNPJ, filtrando por status e navegando na paginação.

O frontend consome o backend ASP.NET existente, com JWT Bearer e isolamento por empresa implementado pelo servidor. Não presume permissões a partir da interface.

Escopo: login, shell, listagem e cadastro inicial de prospectos. O cadastro inclui razão social, CNPJ e regime tributário opcionais e sócios (nome, CPF opcional e participação). Edição, detalhe, conversão, orçamento, contrato, OCR e dashboard estão fora desta entrega.

Prioridades: clareza, consistência, acessibilidade, resposta imediata e manutenção simples. Interface administrativa para uso recorrente em desktop, também utilizável em telas pequenas. Sem métricas ou dados fictícios na aplicação.
