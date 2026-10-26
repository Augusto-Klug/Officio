using Officio.Dominio.Enumeradores;

namespace Officio.Dominio.Admissoes;

public static class AdmissaoDocumentoRegras
{
    public static bool PossuiIdentificacao(IEnumerable<Officio.Dominio.Entidades.AdmissaoDocumento> documentos)
        => documentos.Any(item => item.DocumentoId.HasValue &&
            item.TipoDocumento.Codigo is "IDENTIDADE" or "CNH");

    public static bool IdentificacaoAprovada(IEnumerable<Officio.Dominio.Entidades.AdmissaoDocumento> documentos)
        => documentos.Where(item => item.DocumentoId.HasValue && !item.RevisaoPendente &&
                item.TipoDocumento.Codigo is "IDENTIDADE" or "CNH")
            .Any(item => item.Documento?.Validacoes
                .OrderByDescending(validacao => validacao.CriadoEm)
                .FirstOrDefault()?.Status == Officio.Dominio.Enumeradores.StatusValidacao.Aprovado);

    public static string? ValidarDocumentosParaConclusao(
        IEnumerable<Officio.Dominio.Entidades.AdmissaoDocumento> documentos)
    {
        var itens = documentos.ToList();
        if (itens.Any(item => item.Obrigatorio && item.DocumentoId is null &&
                item.TipoDocumento.Codigo is not ("IDENTIDADE" or "CNH")))
            return "Envie todos os documentos obrigatorios antes de concluir a revisao documental.";

        if (!PossuiIdentificacao(itens))
            return "Envie uma identidade (RG/CIN) ou CNH antes de concluir a revisao documental.";

        var documentoPendente = itens.FirstOrDefault(item => item.DocumentoId.HasValue && item.RevisaoPendente);
        if (documentoPendente is not null)
            return $"O documento '{documentoPendente.RotuloExibicao ?? documentoPendente.TipoDocumento.Descricao}' precisa ser revisado novamente.";

        if (!IdentificacaoAprovada(itens))
            return "Aprove uma identidade ou CNH antes de concluir a revisao documental.";

        foreach (var item in itens.Where(item => item.DocumentoId.HasValue))
        {
            var validacao = item.Documento!.Validacoes
                .OrderByDescending(registro => registro.CriadoEm)
                .FirstOrDefault();
            if (validacao is null || validacao.Status != StatusValidacao.Aprovado)
                return $"O documento '{item.RotuloExibicao ?? item.TipoDocumento.Descricao}' ainda nao foi aprovado.";
            if (validacao.TipoDocumentoIncompativel)
                return $"O documento '{item.RotuloExibicao ?? item.TipoDocumento.Descricao}' foi enviado no tipo incorreto.";
        }

        return null;
    }
}
