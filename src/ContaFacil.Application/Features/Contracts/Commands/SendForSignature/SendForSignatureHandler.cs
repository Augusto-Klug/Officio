using ContaFacil.Application.Common;
using ContaFacil.Application.Common.Interfaces;
using ContaFacil.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ContaFacil.Application.Features.Contracts.Commands.SendForSignature;

public class SendForSignatureHandler(
    DbContext db,
    ISignatureService signatureService,
    IStorageService storageService) : IRequestHandler<SendForSignatureCommand, Result<SendForSignatureResultDto>>
{
    public async Task<Result<SendForSignatureResultDto>> Handle(
        SendForSignatureCommand request,
        CancellationToken ct)
    {
        var contract = await db.Set<Contract>()
            .Include(c => c.Cliente)
            .FirstOrDefaultAsync(c => c.Id == request.ContractId, ct);

        if (contract is null)
            return Result<SendForSignatureResultDto>.Failure("Contrato não encontrado.");

        if (string.IsNullOrEmpty(contract.PdfPath))
            return Result<SendForSignatureResultDto>.Failure("PDF ainda não gerado — execute generate-pdf primeiro.");

        await using var pdfStream = await storageService.DownloadAsync(contract.PdfPath, ct);

        var signers = request.Signatarios
            .Select(s => new SignerInfo(s.Nome, s.Email, s.Cpf, s.Tipo))
            .ToList();

        var signatureRequest = new SignatureRequest(pdfStream, contract.Numero, signers);
        var result = await signatureService.SendForSignatureAsync(signatureRequest, ct);

        if (result.FallbackRequired)
        {
            var pdfUrl = await storageService.GetPresignedUrlAsync(contract.PdfPath, TimeSpan.FromHours(24), ct);
            return Result<SendForSignatureResultDto>.Success(
                new SendForSignatureResultDto("manual_upload", null, null, result.Mensagem, pdfUrl));
        }

        contract.SetD4SignEnvelope(result.EnvelopeId!);

        foreach (var sig in request.Signatarios)
        {
            var signature = Signature.Create(
                contract.TenantId, contract.Id, sig.Nome, sig.Email, sig.Cpf, sig.Tipo);
            db.Set<Signature>().Add(signature);
        }

        await db.SaveChangesAsync(ct);

        return Result<SendForSignatureResultDto>.Success(
            new SendForSignatureResultDto("digital_api", result.EnvelopeId, result.SignersLinks, null, null));
    }
}
