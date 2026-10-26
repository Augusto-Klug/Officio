using Officio.Dominio.Entidades;
using Officio.Dominio.Enumeradores;
using Officio.Dominio.Contratos.Response;
using Officio.Dominio.Interfaces;
using Officio.Dominio.Contratos;
using Officio.Dominio.Contratos.Dtos;
using Officio.Dominio.Comum;
using Officio.Dominio.Eventos;

namespace Officio.Dominio.Servicos.Servicos;

internal class ProspectoServico(
    IProspectoRepositorio prospectoRepositorio,
    IServicoEnderecoComercial servicoEnderecoComercial,
    ILocalidadeServico localidadeServico,
    IEmpresaAtual empresaAtual,
    IPublicadorAtualizacaoTempoReal publicadorAtualizacaoTempoReal) : IProspectoServico
{
    public async Task<List<TipoRelacaoSocietariaDto>> ListarTiposRelacaoSocietariaAsync(CancellationToken ct = default)
        => (await prospectoRepositorio.ListarTiposRelacaoSocietariaAsync(ct))
            .Select(t => new TipoRelacaoSocietariaDto(t.Id, t.Codigo, t.Descricao)).ToList();

    public async Task<Resultado<ProspectoCriadoDto>> CriarAsync(
        CriarProspectoRequisicao requisicao,
        CancellationToken ct = default)
    {
        var cnpj = DocumentoFiscal.SomenteDigitos(requisicao.Cnpj);

        var cadastro = await ValidarCadastroAsync(requisicao, null, ct);
        if (!cadastro.Sucesso)
            return Resultado<ProspectoCriadoDto>.CriarFalha(cadastro.Erro!);

        var sociosValidados = await ValidarSociosAsync(requisicao.Socios, ct);
        if (!sociosValidados.Sucesso)
            return Resultado<ProspectoCriadoDto>.CriarFalha(sociosValidados.Erro!);

        var prospecto = Prospecto.Create(
            empresaAtual.TenantId,
            requisicao.RazaoSocial,
            cnpj,
            requisicao.RegimeTributario);

        AplicarDadosCadastrais(prospecto, requisicao, cadastro.Valor);

        await prospectoRepositorio.AdicionarAsync(prospecto, ct);

        if (requisicao.Socios is { Count: > 0 })
        {
            foreach (var socioRequisicao in requisicao.Socios)
            {
                var socio = Socio.Create(
                    empresaAtual.TenantId,
                    socioRequisicao.Nome,
                    socioRequisicao.Cpf,
                    socioRequisicao.Participacao,
                    prospectId: prospecto.Id);

                await AplicarSocioAsync(socio, socioRequisicao, ct);

                await prospectoRepositorio.AdicionarSocioAsync(socio, ct);
            }
        }

        await prospectoRepositorio.SalvarAlteracoesAsync(ct);
        await PublicarAtualizacaoAsync(prospecto.Id, ct);

        return Resultado<ProspectoCriadoDto>.CriarSucesso(
            new ProspectoCriadoDto(prospecto.Id, prospecto.RazaoSocial));
    }

    public async Task<Resultado<ProspectoDetalheDto>> ObterPorIdAsync(Guid id, CancellationToken ct = default, bool incluirSociosCliente = false)
    {
        var prospecto = await prospectoRepositorio.ObterComSociosAsync(id, ct);
        if (prospecto is null)
            return Resultado<ProspectoDetalheDto>.CriarFalha(ProspectoErros.NaoEncontrado);

        var socios = incluirSociosCliente ? await ObterSociosParaCadastroAsync(prospecto, ct) : prospecto.Socios.ToList();

        var dados = new CriarProspectoRequisicao(
            prospecto.RazaoSocial, prospecto.Cnpj, prospecto.RegimeTributario,
            socios.Select(s => new CriarSocioRequisicao(s.Nome, s.Cpf, s.Participacao, s.Id,
                s.Pessoa.Profissao, s.TipoRelacaoId, MapearEndereco(s.Pessoa.Endereco), s.PessoaId)).ToArray(),
            NomeFantasia: prospecto.NomeFantasia, Cpf: prospecto.Cpf,
            PorteEmpresa: prospecto.PorteEmpresa, CnaePrincipal: prospecto.CnaePrincipal,
            DataAbertura: prospecto.DataAbertura, Email: prospecto.Email, Telefone: prospecto.Telefone,
            Whatsapp: prospecto.Whatsapp, NomeContato: prospecto.NomeContato, Cep: prospecto.Cep,
            Logradouro: prospecto.Logradouro, Numero: prospecto.Numero, Complemento: prospecto.Complemento,
            Bairro: prospecto.Bairro, Cidade: prospecto.Cidade, Uf: prospecto.Uf,
            MunicipioCodigoIbge: prospecto.Municipio?.CodigoIbge, Origem: prospecto.Origem,
            ResponsavelId: prospecto.ResponsavelId, InscricaoEstadual: prospecto.InscricaoEstadual,
            InscricaoMunicipal: prospecto.InscricaoMunicipal, TipoEmpresa: prospecto.TipoEmpresa);
        return Resultado<ProspectoDetalheDto>.CriarSucesso(new(id, prospecto.Status.ToString(), dados));
    }

    public Task<Resultado<bool>> AtualizarAsync(Guid id, CriarProspectoRequisicao requisicao, CancellationToken ct = default)
        => AtualizarCadastroAsync(id, requisicao, false, ct);

    public Task<Resultado<bool>> AtualizarSociosAsync(Guid id, CriarProspectoRequisicao requisicao, CancellationToken ct = default)
        => AtualizarCadastroAsync(id, requisicao, true, ct);

    private async Task<Resultado<bool>> AtualizarCadastroAsync(Guid id, CriarProspectoRequisicao requisicao, bool somenteSocios, CancellationToken ct)
    {
        var prospecto = await prospectoRepositorio.ObterComSociosAsync(id, ct);
        if (prospecto is null)
            return Resultado<bool>.CriarFalha(ProspectoErros.NaoEncontrado);
        if (!somenteSocios && !prospecto.EstaNaEtapaInicial())
            return Resultado<bool>.CriarFalha(ProspectoErros.ForaDaEtapaInicial);

        var cadastro = somenteSocios ? Resultado<Municipio?>.CriarSucesso(prospecto.Municipio)
            : await ValidarCadastroAsync(requisicao, id, ct);
        if (!cadastro.Sucesso)
            return Resultado<bool>.CriarFalha(cadastro.Erro!);

        var socios = requisicao.Socios;
        var existentes = somenteSocios ? await ObterSociosParaCadastroAsync(prospecto, ct) : prospecto.Socios.ToList();
        var ids = socios?.Where(s => s.Id.HasValue).Select(s => s.Id!.Value).ToArray() ?? [];
        if (ids.Distinct().Count() != ids.Length || ids.Any(socioId => existentes.All(s => s.Id != socioId)))
            return Resultado<bool>.CriarFalha("Socio invalido para este prospecto.");

        var sociosValidados = await ValidarSociosAsync(socios, ct);
        if (!sociosValidados.Sucesso) return sociosValidados;

        var removidos = socios is null ? [] : existentes.Where(s => !ids.Contains(s.Id)).ToArray();
        if (await prospectoRepositorio.SociosPossuemDocumentosAsync(removidos.Select(s => s.Id).ToArray(), ct))
            return Resultado<bool>.CriarFalha("Nao e possivel remover socios com documentos vinculados.");
        if (await prospectoRepositorio.SociosPossuemContratosAsync(removidos.Select(s => s.Id).ToArray(), ct))
            return Resultado<bool>.CriarFalha("Não é possível remover um sócio selecionado em contrato.");

        if (!somenteSocios)
        {
            prospecto.AtualizarIdentificacao(requisicao.RazaoSocial, requisicao.Cnpj, requisicao.RegimeTributario);
            AplicarDadosCadastrais(prospecto, requisicao, cadastro.Valor);
        }
        prospectoRepositorio.RemoverSocios(removidos);
        if (socios is not null)
        {
            foreach (var socio in socios)
            {
                if (socio.Id.HasValue)
                    await AplicarSocioAsync(existentes.Single(s => s.Id == socio.Id), socio, ct);
                else
                {
                    var novo = Socio.Create(prospecto.TenantId, socio.Nome, socio.Cpf, socio.Participacao, prospectId: id);
                    await AplicarSocioAsync(novo, socio, ct);
                    await prospectoRepositorio.AdicionarSocioAsync(
                        novo, ct);
                }
            }
        }
        await prospectoRepositorio.SalvarAlteracoesAsync(ct);
        await PublicarAtualizacaoAsync(prospecto.Id, ct);
        return Resultado<bool>.CriarSucesso(true);
    }

    public async Task<Resultado<bool>> ExcluirAsync(Guid id, CancellationToken ct = default)
    {
        var prospecto = await prospectoRepositorio.ObterComSociosAsync(id, ct);
        if (prospecto is null)
            return Resultado<bool>.CriarFalha(ProspectoErros.NaoEncontrado);
        if (!prospecto.EstaNaEtapaInicial())
            return Resultado<bool>.CriarFalha(ProspectoErros.ForaDaEtapaInicial);
        if (await prospectoRepositorio.PossuiVinculosAsync(id, ct))
            return Resultado<bool>.CriarFalha(ProspectoErros.PossuiVinculos);

        prospectoRepositorio.Remover(prospecto);
        await prospectoRepositorio.SalvarAlteracoesAsync(ct);
        await PublicarAtualizacaoAsync(id, ct);
        return Resultado<bool>.CriarSucesso(true);
    }

    private async Task<Resultado<Municipio?>> ValidarCadastroAsync(
        CriarProspectoRequisicao requisicao, Guid? ignorarId, CancellationToken ct)
    {
        if (!string.IsNullOrWhiteSpace(requisicao.Cep))
        {
            var cep = await localidadeServico.ConsultarCepAsync(requisicao.Cep, ct);
            if (cep.Sucesso && cep.Valor is null)
                return Resultado<Municipio?>.CriarFalha(LocalidadeErros.CepInvalido);
            if (cep.Sucesso && !string.IsNullOrWhiteSpace(requisicao.MunicipioCodigoIbge)
                && cep.Valor!.MunicipioCodigoIbge != requisicao.MunicipioCodigoIbge.Trim())
                return Resultado<Municipio?>.CriarFalha(LocalidadeErros.MunicipioDivergenteDoCep);
        }

        var cnpj = DocumentoFiscal.SomenteDigitos(requisicao.Cnpj);
        if (cnpj is not null && await prospectoRepositorio.ExisteComCnpjAsync(cnpj, ct, ignorarId))
            return Resultado<Municipio?>.CriarFalha("Ja existe um prospecto ou cliente com este CNPJ.");

        Municipio? municipio = null;
        if (!string.IsNullOrWhiteSpace(requisicao.MunicipioCodigoIbge))
        {
            municipio = await prospectoRepositorio.ObterMunicipioPorCodigoIbgeAsync(requisicao.MunicipioCodigoIbge.Trim(), ct);
            if (municipio is null)
                return Resultado<Municipio?>.CriarFalha("Municipio invalido. Selecione novamente o municipio do endereco.");
        }
        return Resultado<Municipio?>.CriarSucesso(municipio);
    }

    private static EnderecoSocioRequisicao? MapearEndereco(Endereco? e) => e is null ? null : new(
        e.Cep, e.Logradouro, e.Numero, e.Complemento, e.Bairro, e.Municipio?.Nome,
        e.Municipio?.Estado.Sigla ?? e.Estado?.Sigla, e.Municipio?.CodigoIbge);

    private async Task<List<Socio>> ObterSociosParaCadastroAsync(Prospecto prospecto, CancellationToken ct)
    {
        var socios = prospecto.Socios.ToList();
        var cliente = await prospectoRepositorio.ObterClientePorProspectoAsync(prospecto.Id, ct);
        if (cliente is not null && cliente.TenantId == prospecto.TenantId)
            socios.AddRange(await prospectoRepositorio.ObterSociosClienteAsync(cliente.Id, ct));
        return socios.Where(s => s.TenantId == prospecto.TenantId).DistinctBy(s => s.PessoaId).ToList();
    }

    private async Task<Resultado<bool>> ValidarSociosAsync(IReadOnlyCollection<CriarSocioRequisicao>? socios,
        CancellationToken ct)
    {
        foreach (var s in socios ?? [])
        {
            if (s.TipoRelacaoId.HasValue && await prospectoRepositorio.ObterTipoRelacaoSocietariaAsync(s.TipoRelacaoId.Value, ct) is null)
                return Resultado<bool>.CriarFalha("Tipo de relacao societaria invalido.");
            if (s.Endereco is not { } e) continue;
            Municipio? municipio = null;
            if (!string.IsNullOrWhiteSpace(e.MunicipioCodigoIbge))
            {
                municipio = await prospectoRepositorio.ObterMunicipioPorCodigoIbgeAsync(e.MunicipioCodigoIbge.Trim(), ct);
                if (municipio is null || (!string.IsNullOrWhiteSpace(e.Uf) && !string.Equals(municipio.Estado.Sigla, e.Uf.Trim(), StringComparison.OrdinalIgnoreCase)))
                    return Resultado<bool>.CriarFalha("Municipio e UF do socio inconsistentes.");
            }
            if (!string.IsNullOrWhiteSpace(e.Uf) && await prospectoRepositorio.ObterEstadoPorUfAsync(e.Uf.Trim().ToUpperInvariant(), ct) is null)
                return Resultado<bool>.CriarFalha("UF do socio invalida.");
            if (!string.IsNullOrWhiteSpace(e.Cep))
            {
                var cep = await localidadeServico.ConsultarCepAsync(e.Cep, ct);
                if (cep.Sucesso && cep.Valor is null) return Resultado<bool>.CriarFalha(LocalidadeErros.CepInvalido);
                if (cep.Sucesso && municipio is not null && cep.Valor!.MunicipioCodigoIbge != municipio.CodigoIbge)
                    return Resultado<bool>.CriarFalha(LocalidadeErros.MunicipioDivergenteDoCep);
                if (cep.Sucesso && !string.IsNullOrWhiteSpace(e.Uf) && !string.Equals(cep.Valor!.Uf, e.Uf.Trim(), StringComparison.OrdinalIgnoreCase))
                    return Resultado<bool>.CriarFalha("UF divergente do CEP do socio.");
            }
        }
        return Resultado<bool>.CriarSucesso(true);
    }

    private async Task AplicarSocioAsync(Socio socio, CriarSocioRequisicao dados, CancellationToken ct)
    {
        var tipo = dados.TipoRelacaoId.HasValue
            ? await prospectoRepositorio.ObterTipoRelacaoSocietariaAsync(dados.TipoRelacaoId.Value, ct) : null;
        var endereco = socio.Pessoa.Endereco;
        if (dados.Endereco is { } e)
        {
            var municipio = string.IsNullOrWhiteSpace(e.MunicipioCodigoIbge) ? null
                : await prospectoRepositorio.ObterMunicipioPorCodigoIbgeAsync(e.MunicipioCodigoIbge.Trim(), ct);
            var resultado = await servicoEnderecoComercial.AtualizarOuCriarAsync(endereco,
                new(e.Cep, e.Logradouro, e.Numero, e.Complemento, e.Bairro, municipio?.Nome,
                    municipio?.Estado.Sigla ?? e.Uf, municipio?.Id, PermitirParcial: true), ct);
            if (!resultado.Sucesso) throw new InvalidOperationException(resultado.Erro);
            endereco = resultado.Valor;
        }
        socio.Pessoa.AtualizarDadosSocietarios(dados.Nome, dados.Cpf, dados.Profissao, endereco);
        socio.AtualizarVinculo(dados.Participacao, tipo);
    }

    private static void AplicarDadosCadastrais(Prospecto prospecto, CriarProspectoRequisicao requisicao, Municipio? municipio)
    {
        prospecto.AtualizarDadosCadastrais(
            requisicao.NomeFantasia, requisicao.Cpf, requisicao.PorteEmpresa, requisicao.CnaePrincipal,
            requisicao.DataAbertura, requisicao.Email, requisicao.Telefone, requisicao.Whatsapp,
            requisicao.NomeContato, requisicao.Cep, requisicao.Logradouro, requisicao.Numero,
            requisicao.Complemento, requisicao.Bairro, requisicao.Cidade, requisicao.Uf, requisicao.Origem,
            requisicao.ResponsavelId, requisicao.InscricaoEstadual, requisicao.InscricaoMunicipal, requisicao.TipoEmpresa);
        if (municipio is not null)
            prospecto.VincularMunicipio(municipio.Id, municipio.Nome, municipio.Estado.Sigla);
    }

    public async Task<Resultado<Guid>> ConverterParaClienteAsync(
        Guid prospectoId,
        CancellationToken ct = default)
    {
        var prospecto = await prospectoRepositorio.ObterComSociosAsync(prospectoId, ct);

        if (prospecto is null)
            return Resultado<Guid>.CriarFalha("Prospecto nÃ£o encontrado.");

        if (prospecto.Status == StatusProspecto.Convertido)
            return Resultado<Guid>.CriarFalha("Prospecto jÃ¡ foi convertido.");

        if (string.IsNullOrWhiteSpace(prospecto.Cnpj))
        {
            return Resultado<Guid>.CriarFalha(
                "Informe o CNPJ e constitua a empresa antes de converter o prospecto em cliente.");
        }

        var empresa = await prospectoRepositorio.ObterEmpresaPorCnpjAsync(prospecto.Cnpj, ct);
        if (empresa is null)
        {
            empresa = Empresa.CriarDoProspecto(prospecto);
            await prospectoRepositorio.AdicionarAsync(empresa, ct);
        }

        if (prospecto.EmpresaId != empresa.Id)
            prospecto.VincularEmpresa(empresa.Id);

        if (!empresa.EnderecoId.HasValue)
        {
            var resultadoEndereco = await servicoEnderecoComercial.CriarDoProspectoAsync(prospecto, ct);
            if (!resultadoEndereco.Sucesso)
                return Resultado<Guid>.CriarFalha(resultadoEndereco.Erro!);

            empresa.VincularEndereco(resultadoEndereco.Valor?.Id);
        }

        var statusCliente = await prospectoRepositorio.ObterOuCriarStatusClienteAsync(
            "ATIVO",
            "Cliente ativo",
            1,
            ct);

        var cliente = prospecto.ConverterParaCliente(statusCliente.Id);
        cliente.VincularEndereco(empresa.EnderecoId);
        await prospectoRepositorio.AdicionarAsync(cliente, ct);

        await prospectoRepositorio.SalvarAlteracoesAsync(ct);
        await PublicarAtualizacaoAsync(prospecto.Id, ct);
        await publicadorAtualizacaoTempoReal.PublicarAsync(
            new EventoAtualizacaoTempoReal(prospecto.TenantId, EntidadesAtualizacaoTempoReal.Cliente, cliente.Id),
            ct);

        return Resultado<Guid>.CriarSucesso(cliente.Id);
    }

    private Task PublicarAtualizacaoAsync(Guid prospectoId, CancellationToken ct)
        => publicadorAtualizacaoTempoReal.PublicarAsync(
            new EventoAtualizacaoTempoReal(empresaAtual.TenantId, EntidadesAtualizacaoTempoReal.Prospecto, prospectoId),
            ct);

    public async Task<Resultado<PagedResult<ProspectoDto>>> ObterAsync(
        ObterProspectosFiltro filtro,
        CancellationToken ct = default)
    {
        var query = prospectoRepositorio.MontarConsulta();

        if (!string.IsNullOrWhiteSpace(filtro.Busca))
        {
            var busca = filtro.Busca.Trim();
            var documento = DocumentoFiscal.SomenteDigitos(busca);

            query = query.Where(p =>
                p.RazaoSocial.Contains(busca) ||
                (p.Cnpj != null &&
                    (p.Cnpj.Contains(busca) ||
                     (documento != null && p.Cnpj
                         .Replace(".", "")
                         .Replace("/", "")
                         .Replace("-", "")
                         .Contains(documento)))));
        }

        if (!string.IsNullOrEmpty(filtro.Status) &&
            Enum.TryParse<StatusProspecto>(filtro.Status, ignoreCase: true, out var status))
        {
            query = query.Where(p => p.Status == status);
        }

        if (filtro.ResponsavelId.HasValue)
            query = query.Where(p => p.ResponsavelId == filtro.ResponsavelId);

        var totalRegistros = await prospectoRepositorio.ContarAsync(query, ct);
        var totalPaginas = (int)Math.Ceiling(totalRegistros / (double)filtro.TamanhoPagina);

        var dados = prospectoRepositorio.ObterPagina(
            query,
            filtro.Pagina,
            filtro.TamanhoPagina);

        return Resultado<PagedResult<ProspectoDto>>.CriarSucesso(
            new PagedResult<ProspectoDto>(
                dados
                    .Select(p => new ProspectoDto(
                        p.Id,
                        p.RazaoSocial,
                        p.Cnpj,
                        p.Status,
                        p.ResponsavelNome,
                        p.CriadoEm))
                    .ToList(),
                totalRegistros,
                filtro.Pagina,
                filtro.TamanhoPagina,
                totalPaginas));
    }
}
