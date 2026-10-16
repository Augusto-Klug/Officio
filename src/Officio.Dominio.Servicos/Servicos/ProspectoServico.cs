using Officio.Dominio.Entidades;
using Officio.Dominio.Enumeradores;
using Officio.Dominio.Contratos.Response;
using Officio.Dominio.Interfaces;
using Officio.Dominio.Contratos;
using Officio.Dominio.Contratos.Dtos;
using Officio.Dominio.Comum;

namespace Officio.Dominio.Servicos.Servicos;

internal class ProspectoServico(
    IProspectoRepositorio prospectoRepositorio,
    IServicoEnderecoComercial servicoEnderecoComercial,
    ILocalidadeServico localidadeServico,
    IEmpresaAtual empresaAtual) : IProspectoServico
{
    public async Task<Resultado<ProspectoCriadoDto>> CriarAsync(
        CriarProspectoRequisicao requisicao,
        CancellationToken ct = default)
    {
        var cnpj = DocumentoFiscal.SomenteDigitos(requisicao.Cnpj);

        var cadastro = await ValidarCadastroAsync(requisicao, null, ct);
        if (!cadastro.Sucesso)
            return Resultado<ProspectoCriadoDto>.CriarFalha(cadastro.Erro!);

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

                await prospectoRepositorio.AdicionarSocioAsync(socio, ct);
            }
        }

        await prospectoRepositorio.SalvarAlteracoesAsync(ct);

        return Resultado<ProspectoCriadoDto>.CriarSucesso(
            new ProspectoCriadoDto(prospecto.Id, prospecto.RazaoSocial));
    }

    public async Task<Resultado<ProspectoDetalheDto>> ObterPorIdAsync(Guid id, CancellationToken ct = default)
    {
        var prospecto = await prospectoRepositorio.ObterComSociosAsync(id, ct);
        if (prospecto is null)
            return Resultado<ProspectoDetalheDto>.CriarFalha(ProspectoErros.NaoEncontrado);

        var dados = new CriarProspectoRequisicao(
            prospecto.RazaoSocial, prospecto.Cnpj, prospecto.RegimeTributario,
            prospecto.Socios.Select(s => new CriarSocioRequisicao(s.Nome, s.Cpf, s.Participacao, s.Id)).ToArray(),
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

    public async Task<Resultado<bool>> AtualizarAsync(Guid id, CriarProspectoRequisicao requisicao, CancellationToken ct = default)
    {
        var prospecto = await prospectoRepositorio.ObterComSociosAsync(id, ct);
        if (prospecto is null)
            return Resultado<bool>.CriarFalha(ProspectoErros.NaoEncontrado);
        if (!prospecto.EstaNaEtapaInicial())
            return Resultado<bool>.CriarFalha(ProspectoErros.ForaDaEtapaInicial);

        var cadastro = await ValidarCadastroAsync(requisicao, id, ct);
        if (!cadastro.Sucesso)
            return Resultado<bool>.CriarFalha(cadastro.Erro!);

        var socios = requisicao.Socios;
        var ids = socios?.Where(s => s.Id.HasValue).Select(s => s.Id!.Value).ToArray() ?? [];
        if (ids.Distinct().Count() != ids.Length || ids.Any(socioId => prospecto.Socios.All(s => s.Id != socioId)))
            return Resultado<bool>.CriarFalha("Socio invalido para este prospecto.");

        var removidos = socios is null ? [] : prospecto.Socios.Where(s => !ids.Contains(s.Id)).ToArray();
        if (await prospectoRepositorio.SociosPossuemDocumentosAsync(removidos.Select(s => s.Id).ToArray(), ct))
            return Resultado<bool>.CriarFalha("Nao e possivel remover socios com documentos vinculados.");

        prospecto.AtualizarIdentificacao(requisicao.RazaoSocial, requisicao.Cnpj, requisicao.RegimeTributario);
        AplicarDadosCadastrais(prospecto, requisicao, cadastro.Valor);
        prospectoRepositorio.RemoverSocios(removidos);
        if (socios is not null)
        {
            foreach (var socio in socios)
            {
                if (socio.Id.HasValue)
                    prospecto.Socios.Single(s => s.Id == socio.Id).AtualizarDados(socio.Nome, socio.Cpf, socio.Participacao);
                else
                    await prospectoRepositorio.AdicionarSocioAsync(
                        Socio.Create(prospecto.TenantId, socio.Nome.Trim(), socio.Cpf, socio.Participacao, prospectId: id), ct);
            }
        }
        await prospectoRepositorio.SalvarAlteracoesAsync(ct);
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

        return Resultado<Guid>.CriarSucesso(cliente.Id);
    }

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
