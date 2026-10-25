using Officio.Dominio.Comum;
using Officio.Dominio.Contratos.Response;
using Officio.Dominio.Entidades;
using Officio.Dominio.Interfaces;

namespace Officio.Dominio.Servicos.Servicos;

internal sealed class ServicoEnderecoComercial(
    IProspectoRepositorio prospectoRepositorio,
    IConsultaMunicipioIbge consultaMunicipioIbge) : IServicoEnderecoComercial
{
    public async Task<Resultado<Endereco?>> CriarDoProspectoAsync(
        Prospecto prospecto,
        CancellationToken cancellationToken = default)
        => await CriarAsync(
            new EnderecoComercialRequisicao(
                prospecto.Cep,
                prospecto.Logradouro,
                prospecto.Numero,
                prospecto.Complemento,
                prospecto.Bairro,
                prospecto.Cidade,
                prospecto.Uf,
                prospecto.MunicipioId),
            cancellationToken);

    public Task<Resultado<Endereco?>> CriarAsync(
        EnderecoComercialRequisicao requisicao,
        CancellationToken cancellationToken = default)
        => AtualizarOuCriarAsync(null, requisicao, cancellationToken);

    public async Task<Resultado<Endereco?>> AtualizarOuCriarAsync(
        Endereco? existente,
        EnderecoComercialRequisicao requisicao,
        CancellationToken cancellationToken = default)
    {
        if (requisicao.PermitirParcial)
        {
            if (new[] { requisicao.Cep, requisicao.Logradouro, requisicao.Numero, requisicao.Complemento,
                requisicao.Bairro, requisicao.Uf }.All(string.IsNullOrWhiteSpace) && !requisicao.MunicipioId.HasValue)
                return Resultado<Endereco?>.CriarSucesso(null);
            var estado = string.IsNullOrWhiteSpace(requisicao.Uf) ? null
                : await prospectoRepositorio.ObterEstadoPorUfAsync(requisicao.Uf.Trim().ToUpperInvariant(), cancellationToken);
            var parcial = existente ?? Endereco.Criar(null, "");
            parcial.AtualizarParcial(requisicao.MunicipioId, estado?.Id, requisicao.Logradouro, requisicao.Cep,
                requisicao.Numero, requisicao.Bairro, requisicao.Complemento);
            if (existente is null) await prospectoRepositorio.AdicionarAsync(parcial, cancellationToken);
            return Resultado<Endereco?>.CriarSucesso(parcial);
        }
        if (string.IsNullOrWhiteSpace(requisicao.Logradouro) &&
            string.IsNullOrWhiteSpace(requisicao.Cidade) &&
            string.IsNullOrWhiteSpace(requisicao.Uf))
        {
            return Resultado<Endereco?>.CriarSucesso(null);
        }

        if (string.IsNullOrWhiteSpace(requisicao.Logradouro) ||
            string.IsNullOrWhiteSpace(requisicao.Cidade) ||
            string.IsNullOrWhiteSpace(requisicao.Uf))
        {
            return Resultado<Endereco?>.CriarFalha(
                "Para vincular o endereco, informe logradouro, cidade e UF.");
        }

        Municipio? municipio;
        if (requisicao.MunicipioId.HasValue)
        {
            municipio = await prospectoRepositorio.ObterMunicipioPorIdAsync(
                requisicao.MunicipioId.Value,
                cancellationToken);
        }
        else
        {
            var municipioIbge = await consultaMunicipioIbge.ObterPorNomeUfAsync(
                requisicao.Cidade,
                requisicao.Uf,
                cancellationToken);

            municipio = municipioIbge is null
                ? null
                : await prospectoRepositorio.ObterMunicipioPorCodigoIbgeAsync(
                    municipioIbge.CodigoIbge,
                    cancellationToken);

            if (municipio is null && municipioIbge is not null)
            {
                var estado = await prospectoRepositorio.ObterEstadoPorUfAsync(
                    municipioIbge.Uf,
                    cancellationToken);

                if (estado is null)
                {
                    estado = Estado.Criar(
                        municipioIbge.CodigoIbge[..2],
                        municipioIbge.Uf,
                        municipioIbge.Uf);
                    await prospectoRepositorio.AdicionarAsync(estado, cancellationToken);
                }

                municipio = Municipio.Criar(estado.Id, municipioIbge.CodigoIbge, municipioIbge.Nome);
                await prospectoRepositorio.AdicionarAsync(municipio, cancellationToken);
            }
        }

        if (municipio is null)
            return Resultado<Endereco?>.CriarFalha(
                "Nao foi possivel localizar o municipio informado no catalogo IBGE.");

        if (existente is not null)
        {
            existente.Atualizar(municipio.Id, requisicao.Logradouro, requisicao.Cep,
                requisicao.Numero, requisicao.Bairro, requisicao.Complemento);
            return Resultado<Endereco?>.CriarSucesso(existente);
        }

        var endereco = Endereco.Criar(
            municipio.Id,
            requisicao.Logradouro,
            requisicao.Cep,
            requisicao.Numero,
            requisicao.Bairro,
            requisicao.Complemento);
        await prospectoRepositorio.AdicionarAsync(endereco, cancellationToken);

        return Resultado<Endereco?>.CriarSucesso(endereco);
    }
}
