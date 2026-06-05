using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.BuildingBlock.Application.Helpers;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Mappings;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Contracts;
using Microsoft.EntityFrameworkCore;
using OneOf;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Anemoi.Hr.Application.Cqrs.Commands.ContractCommands.UpdateContract;

public sealed class UpdateContractHandler(
    ISqlRepository<EmployeeContract> employeeContractRepository,
    IUnitOfWork unitOfWork,
    EmployeeContractMapper mapper)
    : ICommandHandler<UpdateContractCommand, OneOf<EmployeeContractDetailResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<EmployeeContractDetailResponse, ErrorDetailResponse>> Handle(
        UpdateContractCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Contract existence check
        var contract = await employeeContractRepository.GetFirstByConditionAsync(x => x.Id == request.Id, null, cancellationToken);
        if (contract is null)
            return HrErrorResponses.Create(HrBusinessErrorCodes.ContractNotFound);

        // 2. Reject modifications on Terminated or Expired contracts
        if (contract.StatusCode == "Terminated")
            return HrErrorResponses.Create(HrBusinessErrorCodes.ContractAlreadyTerminated);

        if (contract.StatusCode == "Expired")
            return HrErrorResponses.Create(HrBusinessErrorCodes.ContractAlreadyExpired);

        // 3. Date range validation
        if (request.EndDate.HasValue && request.EndDate.Value < request.StartDate)
            return HrErrorResponses.Create(HrBusinessErrorCodes.ContractInvalidDateRange);

        // 4. Unique contract number check
        var isNumberDuplicated = await employeeContractRepository.ExistByConditionAsync(x => x.ContractNumber == request.ContractNumber && x.Id != request.Id, cancellationToken);
        if (isNumberDuplicated)
            return HrErrorResponses.Create(HrBusinessErrorCodes.ContractNumberDuplicated);

        // 5. Overlap check excluding this contract
        var existingContracts = await employeeContractRepository.GetManyByConditionAsync(x => x.EmployeeId == contract.EmployeeId && x.Id != request.Id, null, cancellationToken);
        var newEndDateVal = request.EndDate ?? DateOnly.MaxValue;
        var hasOverlap = existingContracts.Any(c =>
            c.StatusCode != "Draft" &&
            c.StartDate <= newEndDateVal &&
            request.StartDate <= (c.EndDate ?? DateOnly.MaxValue));

        if (hasOverlap)
            return HrErrorResponses.Create(HrBusinessErrorCodes.ContractOverlapping);

        // 6. Update fields
        contract.ContractNumber = request.ContractNumber;
        contract.ContractTypeCode = request.ContractTypeCode;
        contract.StartDate = request.StartDate;
        contract.EndDate = request.EndDate;
        contract.SignedDate = request.SignedDate;
        contract.Notes = request.Notes;
        contract.AttachmentFileId = request.AttachmentFileId;
        contract.UpdatedAt = DateTime.UtcNow;
        contract.UpdatedBy = request.UpdatedBy ?? "system";

        var saveResult = await unitOfWork.SaveChangesAsync(cancellationToken);
        if (saveResult.IsT1)
        {
            return saveResult.AsT1 is DbUpdateConcurrencyException
                ? HrErrorResponses.Create(HrBusinessErrorCodes.ContractConcurrencyConflict)
                : HrErrorResponses.Create("HR_SAVE_CHANGES_FAILED");
        }

        return mapper.ToDetailResponse(contract);
    }
}
