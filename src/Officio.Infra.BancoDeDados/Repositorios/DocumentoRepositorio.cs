using Microsoft.EntityFrameworkCore;
using Officio.Dominio.Entidades;
using Officio.Dominio.Interfaces;
using Officio.Infra.BancoDeDados.Contextos;

namespace Officio.Infra.BancoDeDados.Repositorios;

internal class DocumentoRepositorio(
    ContextoAplicacao contexto) : RepositorioBase<ContextoAplicacao, Guid>(contexto), IDocumentoRepositorio
{
    public async Task<Documento?> ObterPorIdAsync(Guid documentoId, CancellationToken cancellationToken = default)
        => await ObterPorIdAsync<Documento>(documentoId, cancellationToken);

    public Task<Guid?> ObterAdmissaoDocumentoIdAsync(Guid documentoId, CancellationToken cancellationToken = default)
        => Contexto.AdmissaoDocumentos
            .Where(item => item.DocumentoId == documentoId)
            .Select(item => (Guid?)item.Id)
            .FirstOrDefaultAsync(cancellationToken);

    public Task<string?> ObterTipoDocumentoAdmissaoAsync(Guid admissaoDocumentoId, CancellationToken cancellationToken = default)
        => Contexto.AdmissaoDocumentos
            .Where(item => item.Id == admissaoDocumentoId)
            .Select(item => item.TipoDocumento.Codigo)
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<Dictionary<string, string>> ObterDadosCadastroAsync(
        Documento documento,
        CancellationToken cancellationToken = default)
    {
        if (documento.AdmissaoId.HasValue)
        {
            var admissao = await Contexto.Admissoes
                .FirstOrDefaultAsync(item => item.Id == documento.AdmissaoId.Value, cancellationToken);

            return admissao is null
                ? []
                : RemoverVazios(new Dictionary<string, string?>
                {
                    ["nome"] = admissao.NomeFuncionario,
                    ["cpf"] = admissao.Cpf,
                    ["rg"] = admissao.Rg,
                    ["data_nascimento"] = admissao.DataNascimento?.ToString("yyyy-MM-dd"),
                    ["escolaridade"] = admissao.Escolaridade
                });
        }

        if (documento.SocioId.HasValue)
        {
            var socio = await Contexto.Socios
                .FirstOrDefaultAsync(s => s.Id == documento.SocioId.Value, cancellationToken);

            return socio is null
                ? []
                : RemoverVazios(new Dictionary<string, string?>
                {
                    ["nome"] = socio.Nome,
                    ["cpf"] = socio.Cpf
                });
        }

        if (documento.ClientId.HasValue)
        {
            var cliente = await Contexto.Clients
                .Include(c => c.Empresa)
                .FirstOrDefaultAsync(c => c.Id == documento.ClientId.Value, cancellationToken);

            return cliente is null
                ? []
                : RemoverVazios(new Dictionary<string, string?>
                {
                    ["razao_social"] = cliente.Empresa.RazaoSocial,
                    ["nome_fantasia"] = cliente.Empresa.NomeFantasia,
                    ["cnpj"] = cliente.Empresa.Cnpj,
                    ["email"] = cliente.Empresa.Email
                });
        }

        if (documento.ProspectId.HasValue)
        {
            var prospecto = await Contexto.Prospects
                .FirstOrDefaultAsync(p => p.Id == documento.ProspectId.Value, cancellationToken);

            return prospecto is null
                ? []
                : RemoverVazios(new Dictionary<string, string?>
                {
                    ["razao_social"] = prospecto.RazaoSocial,
                    ["nome_fantasia"] = prospecto.NomeFantasia,
                    ["cnpj"] = prospecto.Cnpj,
                    ["cpf"] = prospecto.Cpf,
                    ["email"] = prospecto.Email
                });
        }

        return [];
    }

    public async Task AdicionarAsync(Documento documento, CancellationToken cancellationToken = default)
        => await AdicionarAsync<Documento>(documento, cancellationToken);

    public async Task AdicionarValidacaoAsync(ValidacaoDocumento validacao, CancellationToken cancellationToken = default)
        => await AdicionarAsync<ValidacaoDocumento>(validacao, cancellationToken);

    public async Task AdicionarExecucaoOcrAsync(OcrExecucao execucao, CancellationToken cancellationToken = default)
        => await AdicionarAsync<OcrExecucao>(execucao, cancellationToken);

    private static Dictionary<string, string> RemoverVazios(Dictionary<string, string?> valores)
        => valores
            .Where(par => !string.IsNullOrWhiteSpace(par.Value))
            .ToDictionary(par => par.Key, par => par.Value!);
}
