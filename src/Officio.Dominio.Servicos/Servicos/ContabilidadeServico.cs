using Officio.Dominio.Comum;
using Officio.Dominio.Contratos;
using Officio.Dominio.Contratos.Response;
using Officio.Dominio.Entidades;
using Officio.Dominio.Interfaces;

namespace Officio.Dominio.Servicos.Servicos;

public sealed class ContabilidadeServico(IContabilidadeRepositorio repositorio, IProspectoRepositorio localidades,
    ILocalidadeServico localidadeServico, IServicoEnderecoComercial enderecos) : IContabilidadeServico
{
    public async Task<Resultado<DadosContabilidadeDto>> ObterAsync(CancellationToken ct = default)
    {
        var parceiro = await repositorio.ObterAtualAsync(ct);
        return parceiro is null ? Falha("Contabilidade não encontrada.") : Resultado<DadosContabilidadeDto>.CriarSucesso(Mapear(parceiro));
    }

    public async Task<Resultado<DadosContabilidadeDto>> AtualizarAsync(DadosContabilidadeDto dados, CancellationToken ct = default)
    {
        var parceiro = await repositorio.ObterAtualAsync(ct);
        if (parceiro is null) return Falha("Contabilidade não encontrada.");
        var cnpj = DocumentoFiscal.SomenteDigitos(dados.Cnpj)!;
        if (await repositorio.CnpjEmUsoAsync(cnpj, ct)) return Falha("CNPJ já utilizado por outra contabilidade.");

        // Valida ambos os endereços antes de alterar entidades rastreadas. A consulta de CEP
        // pode sincronizar o catálogo de localidades e salvar esse catálogo no mesmo contexto.
        var comercial = await ValidarEnderecoAsync(dados.Endereco, ct);
        if (!comercial.Sucesso) return Falha(comercial.Erro!);
        var tecnico = await ValidarEnderecoAsync(dados.ResponsavelTecnico?.Endereco, ct);
        if (!tecnico.Sucesso) return Falha(tecnico.Erro!);

        if (comercial.Valor is not null)
        {
            var resultado = await enderecos.AtualizarOuCriarAsync(parceiro.Endereco, comercial.Valor, ct);
            if (!resultado.Sucesso) return Falha(resultado.Erro!);
            parceiro.VincularEndereco(resultado.Valor!.Id);
        }
        else parceiro.VincularEndereco(null);

        if (dados.ResponsavelTecnico is { } entrada)
        {
            var responsavel = parceiro.ResponsavelTecnico;
            if (responsavel is null)
            {
                responsavel = ResponsavelTecnico.Criar(parceiro.Id, entrada.Nome, entrada.Cpf, entrada.Profissao, entrada.Crc);
                await repositorio.AdicionarResponsavelAsync(responsavel, ct);
            }
            else responsavel.Atualizar(entrada.Nome, entrada.Cpf, entrada.Profissao, entrada.Crc);
            if (tecnico.Valor is not null)
            {
                var resultado = await enderecos.AtualizarOuCriarAsync(responsavel.Endereco, tecnico.Valor, ct);
                if (!resultado.Sucesso) return Falha(resultado.Erro!);
                responsavel.VincularEndereco(resultado.Valor!.Id);
            }
            else responsavel.VincularEndereco(null);
        }
        // Ausência de responsável não exclui um cadastro já existente nesta operação.
        parceiro.AtualizarDadosContabilidade(cnpj, dados.RazaoSocial, dados.NomeFantasia,
            dados.Email, dados.Telefone, dados.Crc);
        await repositorio.SalvarAlteracoesAsync(ct);
        return await ObterAsync(ct);
    }

    private async Task<Resultado<EnderecoComercialRequisicao?>> ValidarEnderecoAsync(EnderecoContabilidadeDto? endereco, CancellationToken ct)
    {
        if (endereco is null) return Resultado<EnderecoComercialRequisicao?>.CriarSucesso(null);
        var municipio = await localidades.ObterMunicipioPorCodigoIbgeAsync(endereco.MunicipioCodigoIbge, ct);
        if (municipio is null || municipio.Estado.Sigla != endereco.Uf.Trim().ToUpperInvariant())
            return Resultado<EnderecoComercialRequisicao?>.CriarFalha("Selecione um município válido para a UF informada.");
        var cep = DocumentoFiscal.SomenteDigitos(endereco.Cep);
        if (cep is not null)
        {
            var consulta = await localidadeServico.ConsultarCepAsync(cep, ct);
            if (consulta.Sucesso && consulta.Valor is null)
                return Resultado<EnderecoComercialRequisicao?>.CriarFalha("CEP inválido.");
            if (consulta.Sucesso && consulta.Valor!.MunicipioCodigoIbge != municipio.CodigoIbge)
                return Resultado<EnderecoComercialRequisicao?>.CriarFalha("Município divergente do CEP.");
            // Indisponibilidade externa permite cadastro manual, como no cadastro de prospectos.
        }
        return Resultado<EnderecoComercialRequisicao?>.CriarSucesso(new(cep, endereco.Logradouro.Trim(),
            Limpar(endereco.Numero), Limpar(endereco.Complemento), Limpar(endereco.Bairro), municipio.Nome,
            municipio.Estado.Sigla, municipio.Id));
    }

    private static string? Limpar(string? valor) => string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();
    private static Resultado<DadosContabilidadeDto> Falha(string erro) => Resultado<DadosContabilidadeDto>.CriarFalha(erro);

    private static DadosContabilidadeDto Mapear(Parceiro p) => new(p.RazaoSocial, p.NomeFantasia, p.Cnpj, p.Crc,
        p.Email, p.Telefone, MapearEndereco(p.Endereco), p.ResponsavelTecnico is { } r
            ? new(r.Nome, r.Cpf, r.Profissao, r.Crc, MapearEndereco(r.Endereco)) : null);

    private static EnderecoContabilidadeDto? MapearEndereco(Endereco? e) => e is null ? null
        : new(e.Cep, e.Logradouro, e.Numero, e.Complemento, e.Bairro, e.Municipio.Nome,
            e.Municipio.Estado.Sigla, e.Municipio.CodigoIbge);
}
