using ContaFacil.Application.Common.Models.Pdf;

namespace ContaFacil.Application.Common.Interfaces;

public interface IPdfService
{
    Task<Stream> GenerateContractAsync(ContractPdfData data, CancellationToken ct = default);
}
