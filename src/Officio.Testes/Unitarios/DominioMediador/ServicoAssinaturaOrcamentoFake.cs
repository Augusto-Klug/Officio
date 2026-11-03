using Officio.Dominio.Interfaces;

namespace Officio.Testes.Unitarios.DominioMediador;

internal sealed class ServicoAssinaturaOrcamentoFake : IServicoAssinatura
{
    public int DocumentosBloqueados { get; private set; }
    public bool BloqueioSucesso { get; set; } = true;
    public bool ConsultaSucesso { get; set; } = true;
    public bool DocumentoConcluido { get; set; }
    public IReadOnlyList<string> EmailsSignatarios { get; set; } = ["socio@empresa.test", "contador@contabilidade.test"];

    public Task<ResultadoBloqueioAssinatura> BloquearDocumentoAsync(string documentoId, CancellationToken ct = default)
    {
        DocumentosBloqueados++;
        return Task.FromResult(new ResultadoBloqueioAssinatura(BloqueioSucesso));
    }

    public Task<ResultadoAssinatura> EnviarParaAssinaturaAsync(SolicitacaoAssinatura request, CancellationToken ct = default)
        => Task.FromResult(ResultadoAssinatura.CriarFalha("Nao implementado no teste."));

    public Task<ResultadoEstadoAssinatura> ConsultarEstadoDocumentoAsync(string documentoId, CancellationToken ct = default)
        => Task.FromResult(ConsultaSucesso
            ? ResultadoEstadoAssinatura.CriarSucesso(DocumentoConcluido, emailsSignatarios: EmailsSignatarios)
            : ResultadoEstadoAssinatura.CriarFalha("Falha simulada."));

    public Task<ResultadoDocumentoAssinado> BaixarDocumentoAssinadoAsync(string documentoId, CancellationToken ct = default)
        => Task.FromResult(ResultadoDocumentoAssinado.CriarFalha("Nao implementado no teste."));
}
