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
using System.Threading;
using System.Threading.Tasks;

namespace Anemoi.Hr.Application.Cqrs.Commands.ContractCommands.TerminateContract;

public sealed class TerminateContractHandler(
    ISqlRepository<EmployeeContract> employeeContractRepository,
    IUnitOfWork unitOfWork,
    EmployeeContractMapper mapper)
    : ICommandHandler<TerminateContractCommand, OneOf<EmployeeContractDetailResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<EmployeeContractDetailResponse, ErrorDetailResponse>> Handle(
        TerminateContractCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Contract existence check
        var contract = await employeeContractRepository.GetFirstByConditionAsync(x => x.Id == request.Id, null, cancellationToken);
        if (contract is null)
            return HrErrorResponses.Create(HrBusinessErrorCodes.ContractNotFound);

        // 2. Reject termination if already Terminated or Expired
        if (contract.StatusCode == ContractStatusCode.Terminated)
            return HrErrorResponses.Create(HrBusinessErrorCodes.ContractAlreadyTerminated);

        if (contract.StatusCode == ContractStatusCode.Expired)
            return HrErrorResponses.Create(HrBusinessErrorCodes.ContractAlreadyExpired);

        // 3. Date validation: termination date cannot be before contract start date
        if (request.TerminationDate < contract.StartDate)
            return HrErrorResponses.Create(HrBusinessErrorCodes.ContractInvalidDateRange);

        // 4. Update status and populate termination details
        contract.StatusCode = ContractStatusCode.Terminated;
        contract.EndDate = request.TerminationDate;
        contract.TerminationDetail = new ContractTerminationDetail
        {
            TerminatedDate = request.TerminationDate,
            ReasonCode = request.ReasonCode,
            Notes = request.Notes,
            TerminationAttachmentId = request.TerminationAttachmentId
        };
        contract.UpdatedAt = DateTime.UtcNow;
        contract.UpdatedBy = request.UpdatedBy ?? PayrollConstants.SystemActor;

        var saveResult = await unitOfWork.SaveChangesAsync(cancellationToken);
        if (saveResult.IsT1)
            return HrErrorResponses.FromSaveResult(saveResult.AsT1, HrBusinessErrorCodes.ContractConcurrencyConflict);

        return mapper.ToDetailResponse(contract);
    }
}
