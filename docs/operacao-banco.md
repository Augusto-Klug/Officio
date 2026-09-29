# Runbook de banco de dados

Este procedimento cobre atualizacoes de schema do SQL Server e recuperacao a partir de backup. Em producao, siga tambem a janela de manutencao, as credenciais e a politica de backup aprovadas para o ambiente. Nunca use o banco local `Officio` como alvo de um ensaio destrutivo.

## Antes da migration

1. Confirme servidor, nome do banco e ambiente da API. Configure `ConnectionStrings__DefaultConnection` e `Database__Password` por variaveis de ambiente ou pelo cofre de segredos do ambiente.
2. Interrompa escritas na aplicacao durante a janela de manutencao ou combine uma estrategia que garanta consistencia dos dados.
3. Gere um backup completo em um destino independente do volume do banco. Em SQL Server, use `BACKUP DATABASE` com `CHECKSUM`; evite sobrescrever o unico backup recuperavel.
4. Verifique o arquivo com `RESTORE VERIFYONLY ... WITH CHECKSUM` e registre o horario, o banco, a versao da aplicacao e as migrations pendentes.
5. Para validar recuperacao, restaure primeiro com outro nome, sem sobrescrever a base original, e execute `DBCC CHECKDB` e verificacoes funcionais de dados.

## Aplicar migrations

As migrations do Officio sao incrementais. Em uma base existente, faca o backup acima e execute a partir da raiz:

```powershell
dotnet run --project src/Officio.WebApi -- --apply-migrations
```

O comando usa a conexao configurada e aplica somente migrations pendentes. A inicializacao normal da API nao migra o banco automaticamente. `--bootstrap-schema` e exclusivo para um banco vazio e nao deve ser usado para recuperar ou atualizar uma base existente.

Depois da migration, confira o historico em `__EFMigrationsHistory`, valide tabelas/dados afetados e execute smoke tests dos fluxos relevantes antes de reabrir as escritas.

## Restaurar

Restaure o backup em uma base separada. Consulte os nomes logicos dos arquivos com `RESTORE FILELISTONLY` e use `WITH MOVE` para atribuir caminhos/nomes de arquivos que nao colidam com a base original. Em seguida:

```sql
DBCC CHECKDB ([NomeDaBaseRestaurada]) WITH NO_INFOMSGS;
```

Valide contagens ou registros conhecidos e execute smoke tests apontando a aplicacao para a base restaurada. So entao defina, com a equipe responsavel, se a recuperacao sera feita por troca da connection string ou por outro procedimento de cutover. Preserve a base original e o backup ate concluir a validacao.

Em Azure SQL ou outro servico gerenciado, use os recursos nativos de backup/PITR do provedor. O caminho `/var/opt/mssql/data` e especifico do SQL Server local em Docker e nao e um destino de backup externo ou adequado para producao.

## Ensaio local realizado

Em 2026-09-29, no SQL Server 2022 local, o ensaio usou apenas bases temporarias:

- Criacao do schema e baseline: 58 tabelas e 21 migrations registradas.
- Backup completo com checksum antes da migration.
- Simulacao de uma migration pendente removendo do historico da base descartavel o registro `20260927164639_RetirarCpfChecklistAdmissao`; a migration real foi aplicada de 20 para 21 entradas, preservando um registro marcador.
- Restauracao do backup pre-migration em outra base; a mesma migration foi aplicada novamente e `DBCC CHECKDB` passou.
- As duas bases temporarias e os arquivos `.bak` do ensaio foram removidos. O banco local `Officio` nao foi alterado.

Essa simulacao validou o comando de migration, backup e recuperacao, mas partiu de um schema atual e forjou o estado pendente apenas pelo historico. A migration exercitada e de dados, nao representa a transicao completa de um schema historico antigo. Antes de producao, repita o ensaio em uma copia representativa do ambiente de destino, com a migration de schema que sera implantada e o mecanismo real de backup/PITR.
