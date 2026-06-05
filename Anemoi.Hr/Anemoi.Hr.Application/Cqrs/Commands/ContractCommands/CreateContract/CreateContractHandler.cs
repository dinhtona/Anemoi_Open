using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.BuildingBlock.Application.Helpers;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Mappings;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Contracts;
using Anemoi.Hr.Domain.Employees;
using Anemoi.Hr.ModelIds.ModelIds;
using Microsoft.EntityFrameworkCore;
using OneOf;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Anemoi.Hr.Application.Cqrs.Commands.ContractCommands.CreateContract;

public sealed class CreateContractHandler(
    ISqlRepository<Employee> employeeRepository,
    ISqlRepository<EmployeeContract> employeeContractRepository,
    IUnitOfWork unitOfWork,
    EmployeeContractMapper mapper,
    HrSettings hrSettings)
    : ICommandHandler<CreateContractCommand, OneOf<EmployeeContractDetailResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<EmployeeContractDetailResponse, ErrorDetailResponse>> Handle(
        CreateContractCommand request,
        CancellationToken cancellationToken)
    {
        var today = GetBusinessToday();

        // 1. Employee existence check
        var employee = await employeeRepository.GetFirstByConditionAsync(x => x.Id == request.EmployeeId, null, cancellationToken);
        if (employee is null)
            return HrErrorResponses.Create(HrBusinessErrorCodes.EmployeeNotFound);

        // 2. Date range validation
        if (request.EndDate.HasValue && request.EndDate.Value < request.StartDate)
            return HrErrorResponses.Create(HrBusinessErrorCodes.ContractInvalidDateRange);

        // 3. Unique contract number check
        var isNumberDuplicated = await employeeContractRepository.ExistByConditionAsync(x => x.ContractNumber == request.ContractNumber, cancellationToken);
        if (isNumberDuplicated)
            return HrErrorResponses.Create(HrBusinessErrorCodes.ContractNumberDuplicated);

        // 4. Resolve previous contract link
        EmployeeContract prevContract = null;
        if (request.PreviousContractId.HasValue)
        {
            var prevId = new EmployeeContractId(request.PreviousContractId.Value);
            prevContract = await employeeContractRepository.GetFirstByConditionAsync(x => x.Id == prevId, null, cancellationToken);
            if (prevContract is null || prevContract.EmployeeId != request.EmployeeId)
                return HrErrorResponses.Create(HrBusinessErrorCodes.ContractNotFound);
        }

        // 5. Overlap check against active/expired/terminated contracts
        var existingContracts = await employeeContractRepository.GetManyByConditionAsync(x => x.EmployeeId == request.EmployeeId, null, cancellationToken);
        var newEndDateVal = request.EndDate ?? DateOnly.MaxValue;
        var hasOverlap = existingContracts.Any(c =>
            c.StatusCode != "Draft" &&
            (prevContract == null || c.Id != prevContract.Id) &&
            c.StartDate <= newEndDateVal &&
            request.StartDate <= (c.EndDate ?? DateOnly.MaxValue));

        if (!request.IsDraft && hasOverlap)
            return HrErrorResponses.Create(HrBusinessErrorCodes.ContractOverlapping);

        // 6. Close the previous contract if it was active
        if (prevContract is not null && prevContract.StatusCode == "Active")
        {
            var targetEndDate = request.StartDate.AddDays(-1);
            if (targetEndDate < prevContract.StartDate)
                return HrErrorResponses.Create(HrBusinessErrorCodes.ContractInvalidDateRange);

            prevContract.EndDate = targetEndDate;
            if (targetEndDate < today)
            {
                prevContract.StatusCode = "Expired";
            }
            prevContract.UpdatedAt = DateTime.UtcNow;
            prevContract.UpdatedBy = request.CreatedBy ?? "system";
        }

        // 7. Create new contract
        var statusCode = request.IsDraft ? "Draft" : "Active";

        var newContract = new EmployeeContract
        {
            Id = new EmployeeContractId(IdGenerator.NextGuid()),
            EmployeeId = request.EmployeeId,
            ContractNumber = request.ContractNumber,
            ContractTypeCode = request.ContractTypeCode,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            SignedDate = request.SignedDate,
            StatusCode = statusCode,
            Notes = request.Notes,
            AttachmentFileId = request.AttachmentFileId,
            PreviousContractId = request.PreviousContractId,
            CreatedBy = request.CreatedBy ?? "system",
            CreatedAt = DateTime.UtcNow,
            UpdatedBy = request.CreatedBy ?? "system",
            UpdatedAt = DateTime.UtcNow
        };

        await employeeContractRepository.CreateOneAsync(newContract, cancellationToken);
        var saveResult = await unitOfWork.SaveChangesAsync(cancellationToken);
        if (saveResult.IsT1)
        {
            return saveResult.AsT1 is DbUpdateConcurrencyException
                ? HrErrorResponses.Create(HrBusinessErrorCodes.ContractConcurrencyConflict)
                : HrErrorResponses.Create("HR_SAVE_CHANGES_FAILED");
        }

        return mapper.ToDetailResponse(newContract);
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
