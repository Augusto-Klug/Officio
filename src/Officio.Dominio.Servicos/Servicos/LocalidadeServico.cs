using System.Net.Http;
using System.Text.Json;
using Officio.Dominio.Comum;
using Officio.Dominio.Contratos;
using Officio.Dominio.Contratos.Response;
using Officio.Dominio.Entidades;
using Officio.Dominio.Interfaces;

namespace Officio.Dominio.Servicos.Servicos;

internal sealed class LocalidadeServico(
    ILocalidadeRepositorio repositorio,
    IConsultaMunicipioIbge consultaIbge,
    IConsultaCep consultaCep,
    TimeProvider timeProvider) : ILocalidadeServico
{
    private static readonly TimeSpan IntervaloSincronizacao = TimeSpan.FromDays(30);

    public async Task<Resultado<IReadOnlyCollection<EstadoLocalidade>>> ListarEstadosAsync(
        CancellationToken cancellationToken = default)
    {
        var estados = await repositorio.ListarEstadosAsync(cancellationToken);

        if (estados.Count < 27)
        {
            try
            {
                var estadosIbge = await consultaIbge.ListarEstadosAsync(cancellationToken);
                foreach (var estadoIbge in estadosIbge)
                {
                    var estado = estados.FirstOrDefault(item => item.Sigla == estadoIbge.Sigla);
                    if (estado is null)
                    {
                        estado = Estado.Criar(estadoIbge.CodigoIbge, estadoIbge.Sigla, estadoIbge.Nome);
                        await repositorio.AdicionarEstadoAsync(estado, cancellationToken);
                        estados.Add(estado);
                    }
                    else
                    {
                        estado.Atualizar(estadoIbge.CodigoIbge, estadoIbge.Nome);
                    }
                }

                await repositorio.SalvarAlteracoesAsync(cancellationToken);
            }
            catch (Exception exception) when (EhFalhaIntegracao(exception))
            {
                if (estados.Count == 0)
                    return Resultado<IReadOnlyCollection<EstadoLocalidade>>.CriarFalha(
                        "Nao foi possivel carregar os estados no momento.");
            }
        }

        return Resultado<IReadOnlyCollection<EstadoLocalidade>>.CriarSucesso(estados
            .OrderBy(estado => estado.Nome)
            .Select(estado => new EstadoLocalidade(estado.CodigoIbge, estado.Sigla, estado.Nome))
            .ToList());
    }

    public async Task<Resultado<IReadOnlyCollection<MunicipioLocalidade>>> ListarMunicipiosPorUfAsync(
        string uf,
        CancellationToken cancellationToken = default)
    {
        var ufNormalizada = uf.Trim().ToUpperInvariant();
        if (ufNormalizada.Length != 2)
            return Resultado<IReadOnlyCollection<MunicipioLocalidade>>.CriarFalha("UF invalida.");

        var estadosResultado = await ListarEstadosAsync(cancellationToken);
        if (!estadosResultado.Sucesso)
            return Resultado<IReadOnlyCollection<MunicipioLocalidade>>.CriarFalha(estadosResultado.Erro!);

        var estado = await repositorio.ObterEstadoPorUfAsync(ufNormalizada, cancellationToken);
        if (estado is null)
            return Resultado<IReadOnlyCollection<MunicipioLocalidade>>.CriarFalha("UF nao encontrada.");

        var municipios = await repositorio.ListarMunicipiosPorEstadoAsync(estado.Id, cancellationToken);
        var sincronizacaoExpirada = !estado.MunicipiosSincronizadosEm.HasValue ||
            estado.MunicipiosSincronizadosEm.Value < timeProvider.GetUtcNow().UtcDateTime.Subtract(IntervaloSincronizacao);

        if (municipios.Count == 0 || sincronizacaoExpirada)
        {
            try
            {
                var municipiosIbge = await consultaIbge.ListarMunicipiosPorUfAsync(ufNormalizada, cancellationToken);
                var porCodigo = municipios.ToDictionary(item => item.CodigoIbge);

                foreach (var municipioIbge in municipiosIbge)
                {
                    if (porCodigo.TryGetValue(municipioIbge.CodigoIbge, out var municipio))
                    {
                        municipio.Atualizar(estado.Id, municipioIbge.Nome);
                        continue;
                    }

                    municipio = Municipio.Criar(estado.Id, municipioIbge.CodigoIbge, municipioIbge.Nome);
                    await repositorio.AdicionarMunicipioAsync(municipio, cancellationToken);
                    municipios.Add(municipio);
                }

                estado.MarcarMunicipiosSincronizados(timeProvider.GetUtcNow().UtcDateTime);
                await repositorio.SalvarAlteracoesAsync(cancellationToken);
            }
            catch (Exception exception) when (EhFalhaIntegracao(exception))
            {
                if (municipios.Count == 0)
                    return Resultado<IReadOnlyCollection<MunicipioLocalidade>>.CriarFalha(
                        "Nao foi possivel carregar os municipios no momento.");
            }
        }

        return Resultado<IReadOnlyCollection<MunicipioLocalidade>>.CriarSucesso(municipios
            .OrderBy(municipio => municipio.Nome)
            .Select(municipio => new MunicipioLocalidade(
                municipio.CodigoIbge,
                municipio.Nome,
                ufNormalizada))
            .ToList());
    }

    public async Task<Resultado<EnderecoCepLocalidade?>> ConsultarCepAsync(
        string cep,
        CancellationToken cancellationToken = default)
    {
        var cepNormalizado = DocumentoFiscal.SomenteDigitos(cep);
        if (cepNormalizado?.Length != 8)
            return Resultado<EnderecoCepLocalidade?>.CriarFalha("Informe um CEP com 8 digitos.");

        try
        {
            var endereco = await consultaCep.ConsultarAsync(cepNormalizado, cancellationToken);
            if (endereco is null)
                return Resultado<EnderecoCepLocalidade?>.CriarSucesso(null);

            var municipiosResultado = await ListarMunicipiosPorUfAsync(endereco.Uf, cancellationToken);
            if (!municipiosResultado.Sucesso)
                return Resultado<EnderecoCepLocalidade?>.CriarFalha(municipiosResultado.Erro!);

            if (!municipiosResultado.Valor!.Any(item => item.CodigoIbge == endereco.CodigoMunicipioIbge))
                return Resultado<EnderecoCepLocalidade?>.CriarFalha("O municipio do CEP nao foi localizado no IBGE.");

            return Resultado<EnderecoCepLocalidade?>.CriarSucesso(new EnderecoCepLocalidade(
                endereco.Cep,
                endereco.Logradouro,
                endereco.Complemento,
                endereco.Bairro,
                endereco.Cidade,
                endereco.Uf,
                endereco.CodigoMunicipioIbge));
        }
        catch (Exception exception) when (EhFalhaIntegracao(exception))
        {
            return Resultado<EnderecoCepLocalidade?>.CriarFalha(LocalidadeErros.ConsultaCepIndisponivel);
        }
    }

    private static bool EhFalhaIntegracao(Exception exception)
        => exception is HttpRequestException or TaskCanceledException or JsonException;
}
