using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.BuildingBlock.Application.Helpers;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Mappings;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Contracts;
using Anemoi.Hr.ModelIds.ModelIds;
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
    EmployeeContractMapper mapper,
    HrSettings hrSettings)
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

        // 2. Reject modifications unless contract is Draft
        if (contract.StatusCode != ContractStatusCode.Draft)
            return HrErrorResponses.Create(HrBusinessErrorCodes.ContractNotDraft);

        // 3. Date range validation
        if (request.EndDate.HasValue && request.EndDate.Value < request.StartDate)
            return HrErrorResponses.Create(HrBusinessErrorCodes.ContractInvalidDateRange);

        // 4. Unique contract number check
        var isNumberDuplicated = await employeeContractRepository.ExistByConditionAsync(x => x.ContractNumber == request.ContractNumber && x.Id != request.Id, cancellationToken);
        if (isNumberDuplicated)
            return HrErrorResponses.Create(HrBusinessErrorCodes.ContractNumberDuplicated);

        // 5. Overlap check excluding this contract and the previous contract (only when activating)
        if (request.Activate)
        {
            var existingContracts = await employeeContractRepository.GetManyByConditionAsync(x => x.EmployeeId == contract.EmployeeId && x.Id != request.Id, null, cancellationToken);
            var newEndDateVal = request.EndDate ?? DateOnly.MaxValue;
            var hasOverlap = existingContracts.Any(c =>
                c.StatusCode != ContractStatusCode.Draft &&
                (contract.PreviousContractId == null || c.Id.Value != contract.PreviousContractId.Value) &&
                c.StartDate <= newEndDateVal &&
                request.StartDate <= (c.EndDate ?? DateOnly.MaxValue));

            if (hasOverlap)
                return HrErrorResponses.Create(HrBusinessErrorCodes.ContractOverlapping);
        }

        // 6. Renewal previous-contract handling
        if (contract.PreviousContractId.HasValue && request.StartDate != contract.StartDate)
        {
            var prevId = new EmployeeContractId(contract.PreviousContractId.Value);
            var prevContract = await employeeContractRepository.GetFirstByConditionAsync(x => x.Id == prevId, null, cancellationToken);
            if (prevContract is not null)
            {
                var targetEndDate = request.StartDate.AddDays(-1);
                if (targetEndDate < prevContract.StartDate)
                    return HrErrorResponses.Create(HrBusinessErrorCodes.ContractInvalidDateRange);

                prevContract.EndDate = targetEndDate;
                var today = GetBusinessToday();
                if (targetEndDate < today)
                {
                    prevContract.StatusCode = ContractStatusCode.Expired;
                }
                else
                {
                    if (prevContract.StatusCode == ContractStatusCode.Expired)
                    {
                        prevContract.StatusCode = ContractStatusCode.Active;
                    }
                }
                prevContract.UpdatedAt = DateTime.UtcNow;
                prevContract.UpdatedBy = request.UpdatedBy ?? PayrollConstants.SystemActor;
            }
        }

        // 7. Update fields
        contract.ContractNumber = request.ContractNumber;
        contract.ContractTypeCode = request.ContractTypeCode;
        contract.StartDate = request.StartDate;
        contract.EndDate = request.EndDate;
        contract.SignedDate = request.SignedDate;
        contract.Notes = request.Notes;
        contract.AttachmentFileId = request.AttachmentFileId;
        contract.UpdatedAt = DateTime.UtcNow;
        contract.UpdatedBy = request.UpdatedBy ?? PayrollConstants.SystemActor;

        if (request.Activate)
        {
            contract.StatusCode = ContractStatusCode.Active;
        }

        var saveResult = await unitOfWork.SaveChangesAsync(cancellationToken);
        if (saveResult.IsT1)
            return HrErrorResponses.FromSaveResult(saveResult.AsT1, HrBusinessErrorCodes.ContractConcurrencyConflict);

        return mapper.ToDetailResponse(contract);
    }

    private DateOnly GetBusinessToday()
    {
        try
        {
            var timeZone = TimeZoneInfo.FindSystemTimeZoneById(hrSettings.BusinessTimeZone);
            var localTime = TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, timeZone);
            return DateOnly.FromDateTime(localTime.DateTime);
        }
        catch
        {
            return DateOnly.FromDateTime(DateTime.UtcNow);
        }
    }
}
