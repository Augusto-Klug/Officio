namespace Officio.Dominio.Admissoes;

public static class AdmissaoDocumentoRegras
{
    public static bool PossuiIdentificacao(IEnumerable<Officio.Dominio.Entidades.AdmissaoDocumento> documentos)
        => documentos.Any(item => item.DocumentoId.HasValue &&
            item.TipoDocumento.Codigo is "IDENTIDADE" or "CNH");

    public static bool IdentificacaoAprovada(IEnumerable<Officio.Dominio.Entidades.AdmissaoDocumento> documentos)
        => documentos.Where(item => item.DocumentoId.HasValue &&
                item.TipoDocumento.Codigo is "IDENTIDADE" or "CNH")
            .Any(item => item.Documento?.Validacoes
                .OrderByDescending(validacao => validacao.CriadoEm)
                .FirstOrDefault()?.Status == Officio.Dominio.Enumeradores.StatusValidacao.Aprovado);
}
