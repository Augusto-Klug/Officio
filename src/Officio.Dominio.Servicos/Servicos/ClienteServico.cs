using Officio.Dominio.Contratos;
using Officio.Dominio.Contratos.Dtos;
using Officio.Dominio.Contratos.Response;
using Officio.Dominio.Comum;
using Officio.Dominio.Entidades;
using Officio.Dominio.Eventos;
using Officio.Dominio.Interfaces;

namespace Officio.Dominio.Servicos.Servicos;

public class ClienteServico(
    IClienteRepositorio clienteRepositorio,
    IServicoEnderecoComercial servicoEnderecoComercial,
    IPublicadorAtualizacaoTempoReal publicadorAtualizacaoTempoReal,
    IProspectoRepositorio prospectoRepositorio,
    IServicoAssinatura servicoAssinatura) : IClienteServico
{
    public async Task<Resultado<List<ClienteDto>>> ListarAsync(
        string? busca = null,
        bool? ativo = null,
        CancellationToken ct = default)
    {
        var clientes = await clienteRepositorio.ListarAsync(busca, ativo, ct);
        return Resultado<List<ClienteDto>>.CriarSucesso(clientes.Select(Mapear).ToList());
    }

    public async Task<Resultado<ClienteDetalheDto>> ObterPorIdAsync(
        Guid clienteId,
        CancellationToken ct = default)
    {
        var cliente = await clienteRepositorio.ObterDetalheAsync(clienteId, ct);
        if (cliente is null)
            return Resultado<ClienteDetalheDto>.CriarFalha("Cliente nao encontrado.");

        return Resultado<ClienteDetalheDto>.CriarSucesso(MapearDetalhe(cliente));
    }

    public async Task<Resultado<Guid>> AtualizarAsync(
        AtualizarClienteRequisicao requisicao,
        CancellationToken ct = default)
    {
        await using var bloqueio = requisicao.ClienteId == Guid.Empty
            ? NoopAsyncDisposable.Instance
            : await BloquearFluxoAsync(requisicao.ClienteId, ct);

        var cliente = await clienteRepositorio.ObterPorIdAsync(requisicao.ClienteId, ct);
        if (cliente is null)
            return Resultado<Guid>.CriarFalha("Cliente nao encontrado.");

        if (DocumentoFiscal.SomenteDigitos(requisicao.Cnpj) != DocumentoFiscal.SomenteDigitos(cliente.Empresa.Cnpj))
            return Resultado<Guid>.CriarFalha("O CNPJ do cliente nao pode ser alterado.");

        var alteracaoSocios = await PrepararSociosAsync(cliente, requisicao.Socios, ct);
        if (!alteracaoSocios.Sucesso)
            return Resultado<Guid>.CriarFalha(alteracaoSocios.Erro!);

        Guid? municipioEmpresaId = null;
        var enderecoEmpresaInformado = new[] { requisicao.Cep, requisicao.Logradouro, requisicao.Numero,
            requisicao.Complemento, requisicao.Bairro, requisicao.Cidade, requisicao.Uf }.Any(v => !string.IsNullOrWhiteSpace(v));
        if (enderecoEmpresaInformado && (string.IsNullOrWhiteSpace(requisicao.Logradouro) ||
            string.IsNullOrWhiteSpace(requisicao.Cidade) || string.IsNullOrWhiteSpace(requisicao.Uf) ||
            string.IsNullOrWhiteSpace(requisicao.MunicipioCodigoIbge)))
            return Resultado<Guid>.CriarFalha("Informe logradouro, cidade e UF selecionados pelo catalogo IBGE para o endereco da empresa.");
        if (!string.IsNullOrWhiteSpace(requisicao.MunicipioCodigoIbge))
        {
            municipioEmpresaId = (await prospectoRepositorio.ObterMunicipioPorCodigoIbgeAsync(
                requisicao.MunicipioCodigoIbge.Trim(), ct))?.Id;
            if (!municipioEmpresaId.HasValue)
                return Resultado<Guid>.CriarFalha("Selecione um municipio valido para o endereco da empresa.");
        }

        var alterouCadastro = DadosEmpresaAlterados(cliente, requisicao) || alteracaoSocios.Valor;
        var contratos = cliente.ProspectOrigemId.HasValue
            ? await prospectoRepositorio.ListarContratosCancelaveisAsync(cliente.ProspectOrigemId.Value, ct)
            : [];
        if (alterouCadastro && contratos.Count > 0 &&
            (!requisicao.ConfirmarCancelamentoContratos || requisicao.ContratosConfirmados is null ||
             !contratos.Select(c => c.Id).ToHashSet().SetEquals(requisicao.ContratosConfirmados)))
            return Resultado<Guid>.CriarFalha("CONFIRMACAO_CANCELAMENTO_CONTRATOS: Existem contratos em rascunho ou aguardando assinatura que serao cancelados.");

        var documentosBloqueados = alterouCadastro
            ? await BloquearAssinaturasAsync(contratos, ct)
            : Resultado<bool>.CriarSucesso(true);
        if (!documentosBloqueados.Sucesso)
            return Resultado<Guid>.CriarFalha(documentosBloqueados.Erro!);

        try
        {
            cliente.Empresa.AtualizarDadosCliente(
                requisicao.RazaoSocial,
                requisicao.NomeFantasia,
                requisicao.Email,
                requisicao.Telefone,
                requisicao.Whatsapp,
                requisicao.RegimeTributario?.ToString(),
                requisicao.InscricaoEstadual,
                requisicao.InscricaoMunicipal,
                requisicao.TipoEmpresa);

            var endereco = await servicoEnderecoComercial.AtualizarOuCriarAsync(
                cliente.Empresa.Endereco,
                new EnderecoComercialRequisicao(
                    requisicao.Cep,
                    requisicao.Logradouro,
                    requisicao.Numero,
                    requisicao.Complemento,
                    requisicao.Bairro,
                    requisicao.Cidade,
                    requisicao.Uf,
                    MunicipioId: municipioEmpresaId),
                ct);

            if (!endereco.Sucesso)
                return Resultado<Guid>.CriarFalha(endereco.Erro ?? "Nao foi possivel atualizar o endereco.");

            cliente.Empresa.VincularEndereco(endereco.Valor?.Id);
            cliente.VincularEndereco(endereco.Valor?.Id);
            cliente.AtualizarDados(
                requisicao.RazaoSocial, requisicao.NomeFantasia, cliente.Empresa.Cnpj, requisicao.Cpf,
                requisicao.RegimeTributario, requisicao.Email, requisicao.Telefone, requisicao.Whatsapp,
                requisicao.Cep, requisicao.Logradouro, requisicao.Numero, requisicao.Bairro,
                requisicao.Cidade, requisicao.Uf, requisicao.Observacoes);

            if (alteracaoSocios.Valor)
                await AplicarSociosAsync(cliente, requisicao.Socios!, ct);
        }
        catch (ArgumentException exception)
        {
            return Resultado<Guid>.CriarFalha(exception.Message);
        }

        await clienteRepositorio.SalvarAlteracoesAsync(ct);
        await PublicarAtualizacaoAsync(cliente, ct);
        foreach (var contrato in contratos)
            await publicadorAtualizacaoTempoReal.PublicarAsync(
                new EventoAtualizacaoTempoReal(contrato.TenantId, EntidadesAtualizacaoTempoReal.Contrato, contrato.Id), ct);

        return Resultado<Guid>.CriarSucesso(cliente.Id);
    }

    public async Task<Resultado<bool>> VincularCodigoSciAsync(
        VincularCodigoSciRequisicao requisicao,
        CancellationToken ct = default)
    {
        var cliente = await clienteRepositorio.ObterPorIdAsync(requisicao.ClienteId, ct);
        if (cliente is null)
            return Resultado<bool>.CriarFalha("Cliente nao encontrado.");

        try
        {
            cliente.VincularSciCodigo(requisicao.SciCodigo);
        }
        catch (ArgumentException exception)
        {
            return Resultado<bool>.CriarFalha(exception.Message);
        }

        await clienteRepositorio.SalvarAlteracoesAsync(ct);
        await PublicarAtualizacaoAsync(cliente, ct);

        return Resultado<bool>.CriarSucesso(true);
    }

    private Task PublicarAtualizacaoAsync(Cliente cliente, CancellationToken ct)
        => publicadorAtualizacaoTempoReal.PublicarAsync(
            new EventoAtualizacaoTempoReal(cliente.TenantId, EntidadesAtualizacaoTempoReal.Cliente, cliente.Id),
            ct);

    private static ClienteDto Mapear(Cliente cliente)
        => new(
            cliente.Id,
            cliente.EmpresaId,
            cliente.Empresa.RazaoSocial,
            cliente.Empresa.NomeFantasia,
            cliente.Empresa.Cnpj,
            null,
            cliente.Empresa.RegimeTributario,
            cliente.SciCodigo,
            cliente.Ativo,
            cliente.Empresa.Email,
            cliente.Empresa.Telefone,
            cliente.Empresa.Whatsapp,
            ObterCep(cliente),
            ObterLogradouro(cliente),
            ObterNumero(cliente),
            ObterBairro(cliente),
            ObterCidade(cliente),
            ObterUf(cliente),
            cliente.Observacoes,
            cliente.ProspectOrigemId,
            cliente.Socios.Count(socio => socio.Ativo),
            cliente.Contratos.Count,
            cliente.Documentos.Count);

    private static ClienteDetalheDto MapearDetalhe(Cliente cliente)
        => new(
            cliente.Id,
            cliente.EmpresaId,
            cliente.Empresa.RazaoSocial,
            cliente.Empresa.NomeFantasia,
            cliente.Empresa.Cnpj,
            null,
            cliente.Empresa.RegimeTributario,
            cliente.SciCodigo,
            cliente.Ativo,
            cliente.Empresa.Email,
            cliente.Empresa.Telefone,
            cliente.Empresa.Whatsapp,
            ObterCep(cliente),
            ObterLogradouro(cliente),
            ObterNumero(cliente),
            ObterBairro(cliente),
            ObterCidade(cliente),
            ObterUf(cliente),
            cliente.Observacoes,
            cliente.ProspectOrigemId,
            cliente.InicioRelacionamentoEm,
            cliente.Socios
                .OrderByDescending(socio => socio.Ativo)
                .ThenBy(socio => socio.VigenteDesde)
                .Select(socio => new ClienteSocioDto(
                    socio.Id, socio.Nome, socio.Cpf, socio.Participacao ?? 0, socio.Ativo,
                    socio.VigenteDesde, socio.VigenteAte, socio.Pessoa.Profissao, socio.TipoRelacaoId,
                    socio.TipoRelacao?.Descricao, socio.Pessoa.Endereco?.Cep, socio.Pessoa.Endereco?.Logradouro,
                    socio.Pessoa.Endereco?.Numero, socio.Pessoa.Endereco?.Complemento, socio.Pessoa.Endereco?.Bairro,
                    socio.Pessoa.Endereco?.Municipio?.Nome, socio.Pessoa.Endereco?.Municipio?.Estado.Sigla ?? socio.Pessoa.Endereco?.Estado?.Sigla,
                    socio.Pessoa.Endereco?.Municipio?.CodigoIbge))
                .ToList(),
            cliente.Contratos
                .Select(contrato => new ClienteContratoDto(
                    contrato.Id,
                    contrato.Numero,
                    contrato.Status.ToString(),
                    contrato.ValorTotal,
                    contrato.DataInicio,
                    contrato.DataFim))
                .ToList(),
            cliente.Documentos
                .Select(documento => new ClienteDocumentoDto(
                    documento.Id,
                    documento.NomeArquivo,
                    documento.Tipo.ToString(),
                    documento.Legivel,
                    documento.QualidadeEstimada))
                .ToList(),
            cliente.Empresa.InscricaoEstadual,
            cliente.Empresa.InscricaoMunicipal,
            cliente.Empresa.TipoEmpresa,
            cliente.Empresa.Endereco?.Complemento,
            cliente.Empresa.Endereco?.Municipio?.CodigoIbge);

    private static string? ObterCep(Cliente cliente)
        => cliente.Empresa.Endereco?.Cep ?? cliente.Cep;

    private static string? ObterLogradouro(Cliente cliente)
        => cliente.Empresa.Endereco?.Logradouro ?? cliente.Logradouro;

    private static string? ObterNumero(Cliente cliente)
        => cliente.Empresa.Endereco?.Numero ?? cliente.Numero;

    private static string? ObterBairro(Cliente cliente)
        => cliente.Empresa.Endereco?.Bairro ?? cliente.Bairro;

    private static string? ObterCidade(Cliente cliente)
        => cliente.Empresa.Endereco?.Municipio.Nome ?? cliente.Cidade;

    private static string? ObterUf(Cliente cliente)
        => cliente.Empresa.Endereco?.Municipio.Estado.Sigla ?? cliente.Uf;

    private async Task<IAsyncDisposable> BloquearFluxoAsync(Guid clienteId, CancellationToken ct)
    {
        var cliente = await clienteRepositorio.ObterPorIdAsync(clienteId, ct);
        return cliente?.ProspectOrigemId is Guid prospectoId
            ? await prospectoRepositorio.BloquearFluxoContratosAsync(prospectoId, ct)
            : NoopAsyncDisposable.Instance;
    }

    private async Task<Resultado<bool>> PrepararSociosAsync(
        Cliente cliente, IReadOnlyCollection<CriarSocioRequisicao>? requisicoes, CancellationToken ct)
    {
        if (requisicoes is null) return Resultado<bool>.CriarSucesso(false);
        var atuais = cliente.Socios.Where(s => s.Ativo).ToDictionary(s => s.Id);
        var ids = requisicoes.Where(s => s.Id.HasValue).Select(s => s.Id!.Value).ToArray();
        if (ids.Distinct().Count() != ids.Length || ids.Any(id => !atuais.ContainsKey(id)))
            return Resultado<bool>.CriarFalha("A lista de socios contem vinculos invalidos ou desatualizados.");
        foreach (var socio in requisicoes)
        {
            if (string.IsNullOrWhiteSpace(socio.Nome) || socio.Participacao is null or < 0 or > 100)
                return Resultado<bool>.CriarFalha("Informe nome e participacao valida para cada socio.");
            if (socio.TipoRelacaoId.HasValue && await prospectoRepositorio.ObterTipoRelacaoSocietariaAsync(socio.TipoRelacaoId.Value, ct) is null)
                return Resultado<bool>.CriarFalha("Tipo de relacao societaria invalido.");
            if (socio.Endereco is { } endereco && new[] { endereco.Cep, endereco.Logradouro, endereco.Numero,
                    endereco.Complemento, endereco.Bairro, endereco.Cidade, endereco.Uf }.Any(v => !string.IsNullOrWhiteSpace(v)))
            {
                if (string.IsNullOrWhiteSpace(endereco.Logradouro) || string.IsNullOrWhiteSpace(endereco.Cidade) ||
                    string.IsNullOrWhiteSpace(endereco.Uf) || string.IsNullOrWhiteSpace(endereco.MunicipioCodigoIbge))
                    return Resultado<bool>.CriarFalha("Informe um endereco completo com municipio selecionado para cada socio.");
                var municipio = await prospectoRepositorio.ObterMunicipioPorCodigoIbgeAsync(endereco.MunicipioCodigoIbge, ct);
                if (municipio is null || !string.Equals(municipio.Estado.Sigla, endereco.Uf, StringComparison.OrdinalIgnoreCase))
                    return Resultado<bool>.CriarFalha("Municipio e UF do endereco do socio inconsistentes.");
            }
        }
        var mudou = atuais.Count != requisicoes.Count;
        if (!mudou)
            mudou = requisicoes.Any(req => !SocioIgual(atuais[req.Id!.Value], req));
        return Resultado<bool>.CriarSucesso(mudou);
    }

    private async Task AplicarSociosAsync(Cliente cliente, IReadOnlyCollection<CriarSocioRequisicao> requisicoes, CancellationToken ct)
    {
        var atuais = cliente.Socios.Where(s => s.Ativo).ToDictionary(s => s.Id);
        var porId = requisicoes.Where(s => s.Id.HasValue).ToDictionary(s => s.Id!.Value);
        foreach (var socioAtual in atuais.Values)
        {
            if (!porId.TryGetValue(socioAtual.Id, out var dados) || !SocioIgual(socioAtual, dados))
            {
                var enderecoHistorico = socioAtual.Pessoa.Endereco?.CriarCopia();
                if (enderecoHistorico is not null)
                    await clienteRepositorio.AdicionarEnderecoAsync(enderecoHistorico, ct);
                var pessoaHistorica = socioAtual.Pessoa.CriarCopia(enderecoHistorico);
                await clienteRepositorio.AdicionarPessoaAsync(pessoaHistorica, ct);
                socioAtual.VincularPessoa(pessoaHistorica);
                socioAtual.EncerrarVigencia();
                if (porId.TryGetValue(socioAtual.Id, out dados))
                {
                    var endereco = await CriarEnderecoSocioAsync(dados.Endereco, ct);
                    var versao = Socio.CriarVersao(socioAtual, cliente.Id, socioAtual.Pessoa.Endereco);
                    versao.AplicarDados(dados.Nome!, dados.Cpf, dados.Participacao!.Value, dados.Profissao,
                        dados.TipoRelacaoId.HasValue ? await prospectoRepositorio.ObterTipoRelacaoSocietariaAsync(dados.TipoRelacaoId.Value, ct) : null,
                        endereco);
                    await clienteRepositorio.AdicionarSocioAsync(versao, ct);
                }
            }
        }
        foreach (var dados in requisicoes.Where(s => !s.Id.HasValue))
        {
            var endereco = await CriarEnderecoSocioAsync(dados.Endereco, ct);
            var tipo = dados.TipoRelacaoId.HasValue
                ? await prospectoRepositorio.ObterTipoRelacaoSocietariaAsync(dados.TipoRelacaoId.Value, ct) : null;
            cliente.AddSocio(Socio.CriarParaCliente(cliente.TenantId, cliente.Id, dados.Nome!, dados.Cpf,
                dados.Participacao!.Value, dados.Profissao, tipo, endereco));
        }
    }

    private async Task<Endereco?> CriarEnderecoSocioAsync(EnderecoSocioRequisicao? requisicao, CancellationToken ct)
    {
        if (requisicao is null || new[] { requisicao.Cep, requisicao.Logradouro, requisicao.Numero,
                requisicao.Complemento, requisicao.Bairro, requisicao.Cidade, requisicao.Uf }.All(string.IsNullOrWhiteSpace))
            return null;
        var municipioId = string.IsNullOrWhiteSpace(requisicao.MunicipioCodigoIbge) ? (Guid?)null
            : (await prospectoRepositorio.ObterMunicipioPorCodigoIbgeAsync(requisicao.MunicipioCodigoIbge, ct))?.Id;
        if (string.IsNullOrWhiteSpace(requisicao.MunicipioCodigoIbge) || !municipioId.HasValue)
            throw new ArgumentException("Selecione um municipio valido para o endereco do socio.");
        var resultado = await servicoEnderecoComercial.CriarAsync(new EnderecoComercialRequisicao(
            requisicao.Cep, requisicao.Logradouro, requisicao.Numero, requisicao.Complemento,
            requisicao.Bairro, requisicao.Cidade, requisicao.Uf, municipioId), ct);
        if (!resultado.Sucesso) throw new ArgumentException(resultado.Erro ?? "Endereco do socio invalido.");
        return resultado.Valor;
    }

    private async Task<Resultado<bool>> BloquearAssinaturasAsync(
        IReadOnlyCollection<Contrato> contratos, CancellationToken ct)
    {
        var documentos = new List<(string Id, HashSet<string> Emails)>();
        foreach (var contrato in contratos.Where(c => c.Status == Officio.Dominio.Enumeradores.StatusContrato.AguardandoAssinatura))
        {
            var solicitacoes = contrato.SolicitacoesAssinatura.Where(s => !string.IsNullOrWhiteSpace(s.IdExterno)).ToArray();
            if (solicitacoes.Length == 0)
                return Resultado<bool>.CriarFalha("Nao foi encontrada solicitacao para bloquear o contrato na Autentique.");
            foreach (var solicitacao in solicitacoes)
            {
                if (!string.Equals(solicitacao.Provedor, "autentique", StringComparison.OrdinalIgnoreCase))
                    return Resultado<bool>.CriarFalha("O provedor da assinatura nao permite cancelamento automatico.");
                var emails = solicitacao.Signatarios.Select(s => s.Email.Trim()).ToHashSet(StringComparer.OrdinalIgnoreCase);
                var antes = await servicoAssinatura.ConsultarEstadoDocumentoAsync(solicitacao.IdExterno!, ct);
                if (!antes.Sucesso || antes.Concluido || emails.Count == 0 || !emails.SetEquals(antes.EmailsSignatarios))
                    return Resultado<bool>.CriarFalha("Nao foi possivel confirmar que o documento continua pendente na Autentique.");
                documentos.Add((solicitacao.IdExterno!, emails));
            }
        }
        foreach (var documento in documentos)
        {
            var bloqueio = await servicoAssinatura.BloquearDocumentoAsync(documento.Id, ct);
            var depois = await servicoAssinatura.ConsultarEstadoDocumentoAsync(documento.Id, ct);
            if (!bloqueio.Sucesso || !depois.Sucesso || depois.Concluido || !documento.Emails.SetEquals(depois.EmailsSignatarios))
                return Resultado<bool>.CriarFalha("Nao foi possivel confirmar o cancelamento de todos os documentos na Autentique.");
        }
        foreach (var contrato in contratos) contrato.Cancelar();
        return Resultado<bool>.CriarSucesso(true);
    }

    private static bool SocioIgual(Socio socio, CriarSocioRequisicao req)
    {
        var endereco = socio.Pessoa.Endereco;
        var atual = endereco is null ? null : new EnderecoSocioRequisicao(
            endereco.Cep, endereco.Logradouro, endereco.Numero, endereco.Complemento, endereco.Bairro,
            endereco.Municipio?.Nome, endereco.Municipio?.Estado.Sigla ?? endereco.Estado?.Sigla,
            endereco.Municipio?.CodigoIbge);
        return string.Equals(socio.Nome.Trim(), req.Nome?.Trim(), StringComparison.Ordinal) &&
               DocumentoFiscal.SomenteDigitos(socio.Cpf) == DocumentoFiscal.SomenteDigitos(req.Cpf) &&
               socio.Participacao == req.Participacao && socio.Pessoa.Profissao == req.Profissao &&
               socio.TipoRelacaoId == req.TipoRelacaoId && EnderecoIgual(atual, req.Endereco);
    }

    private static bool EnderecoIgual(EnderecoSocioRequisicao? a, EnderecoSocioRequisicao? b)
        => string.Equals(a?.Cep, b?.Cep, StringComparison.OrdinalIgnoreCase) &&
           string.Equals(a?.Logradouro, b?.Logradouro, StringComparison.OrdinalIgnoreCase) &&
           string.Equals(a?.Numero, b?.Numero, StringComparison.OrdinalIgnoreCase) &&
           string.Equals(a?.Complemento, b?.Complemento, StringComparison.OrdinalIgnoreCase) &&
           string.Equals(a?.Bairro, b?.Bairro, StringComparison.OrdinalIgnoreCase) &&
           string.Equals(a?.MunicipioCodigoIbge, b?.MunicipioCodigoIbge, StringComparison.OrdinalIgnoreCase);

    private static bool DadosEmpresaAlterados(Cliente cliente, AtualizarClienteRequisicao req)
        => cliente.Empresa.RazaoSocial != req.RazaoSocial || cliente.Empresa.NomeFantasia != req.NomeFantasia ||
           cliente.Cpf != DocumentoFiscal.SomenteDigitos(req.Cpf) || cliente.Observacoes != req.Observacoes ||
           cliente.Empresa.Email != req.Email || cliente.Empresa.Telefone != req.Telefone ||
           cliente.Empresa.Whatsapp != req.Whatsapp || cliente.Empresa.RegimeTributario != req.RegimeTributario?.ToString() ||
           cliente.Empresa.InscricaoEstadual != req.InscricaoEstadual || cliente.Empresa.InscricaoMunicipal != req.InscricaoMunicipal ||
           cliente.Empresa.TipoEmpresa != req.TipoEmpresa || cliente.Empresa.Endereco?.Cep != req.Cep ||
           cliente.Empresa.Endereco?.Logradouro != req.Logradouro || cliente.Empresa.Endereco?.Numero != req.Numero ||
           cliente.Empresa.Endereco?.Bairro != req.Bairro || cliente.Empresa.Endereco?.Complemento != req.Complemento ||
           cliente.Empresa.Endereco?.Municipio?.CodigoIbge != req.MunicipioCodigoIbge || cliente.Empresa.Endereco?.Municipio?.Nome != req.Cidade ||
           cliente.Empresa.Endereco?.Municipio?.Estado.Sigla != req.Uf;

    private sealed class NoopAsyncDisposable : IAsyncDisposable
    {
        public static NoopAsyncDisposable Instance { get; } = new();
        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }
}
