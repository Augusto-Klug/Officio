# Task 5 Report - Estimativa Real de Qualidade do Documento

## STATUS

Concluida.

## CHANGES

- Adicionados `IEstimadorQualidadeDocumento` e o estimador deterministico inicial.
- `DocumentoServico.EnviarAsync` estima a qualidade antes do armazenamento e rejeita valores abaixo de `0.4m`.
- Registrada a dependencia na extensao publica de servicos de dominio.
- Adicionado teste de rejeicao com qualidade configuravel no fake; os cinco testes existentes foram preservados.
- OCR indisponivel continua encaminhando o documento para revisao manual no fluxo de validacao.
- `Enumerable.Average` retorna `double` para bytes; a media foi convertida explicitamente para `decimal` antes do calculo.

## TESTS

- `dotnet test Officio.Testes/Officio.Testes.csproj --filter DocumentoServicoTestes`: 6 aprovados, 0 falhas.
- `dotnet build Officio.slnx`: concluido com 0 avisos e 0 erros.
- `dotnet test Officio.Testes/Officio.Testes.csproj --filter "DocumentoServicoTestes|ValidarDocumento"`: 6 aprovados, 0 falhas.

## COMMITS

- `feat: estimate document quality before upload`.

## SELF_REVIEW

- A qualidade e estimada antes de enviar o arquivo ao armazenamento.
- O fluxo de validacao nao recebeu filtro manual de tenant nem setters publicos em entidades.
- O fallback de OCR indisponivel para revisao manual permanece coberto.

## CONCERNS

- A heuristica inicial usa bytes do arquivo como aproximacao deterministica; nao substitui uma analise visual especializada de imagem.
