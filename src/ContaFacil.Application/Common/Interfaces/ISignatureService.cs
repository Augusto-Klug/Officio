using ContaFacil.Application.Common.Models.Signature;

namespace ContaFacil.Application.Common.Interfaces;

public interface ISignatureService
{
    Task<SignatureResult> SendForSignatureAsync(SignatureRequest request, CancellationToken ct = default);
    Task<SignatureStatusResult> GetStatusAsync(string envelopeId, CancellationToken ct = default);
}
