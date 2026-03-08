using ContaFacil.Application.Common;
using ContaFacil.Application.Common.Interfaces;
using ContaFacil.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ContaFacil.Application.Features.Contracts.Commands.GenerateContractPdf;

public class GenerateContractPdfHandler(
    DbContext db,
    IPdfService pdfService,
    IStorageService storageService,
    ICurrentTenant currentTenant) : 
    IRequestHandler<GenerateContractPdfCommand, Result<ContractPdfResultDto>>
{
    public async Task<Result<ContractPdfResultDto>> Handle(GenerateContractPdfCommand request, CancellationToken ct)
    {
        var contract = await db.Set<Contract>()
            .Include(c => c.Cliente)
                .ThenInclude(cl => cl.Socios)
            .Include(c => c.Budget)
                .ThenInclude(b => b!.Itens)
            .FirstOrDefaultAsync(c => c.Id == request.ContractId, ct);

        if (contract is null)
            return Result<ContractPdfResultDto>.Failure("Contrato não encontrado.");

        var tenant = await db.Set<Tenant>()
            .FirstAsync(t => t.Id == currentTenant.TenantId, ct);

        var pdfData = ContractPdfDataMapper.Map(contract, tenant);

        await using var pdfStream = await pdfService.GenerateContractAsync(pdfData, ct);

        var path = await storageService.UploadAsync(
            pdfStream,
            $"contratos/{contract.Numero}.pdf",
            "contracts",
            ct);

        contract.SetPdfPath(path);
        await db.SaveChangesAsync(ct);

        var url = await storageService.GetPresignedUrlAsync(path, TimeSpan.FromHours(1), ct);

        return Result<ContractPdfResultDto>.Success(
            new ContractPdfResultDto(url, DateTime.UtcNow));
    }
}
