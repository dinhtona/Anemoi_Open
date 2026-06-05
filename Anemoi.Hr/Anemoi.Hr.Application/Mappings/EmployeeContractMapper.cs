using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Anemoi.Hr.Application.Mappings;

public sealed class EmployeeContractMapper
{
    public EmployeeContractResponse ToResponse(EmployeeContract contract)
    {
        if (contract is null) return null;
        return new EmployeeContractResponse
        {
            Id = contract.Id.Value.ToString(),
            EmployeeId = contract.EmployeeId.Value.ToString(),
            ContractNumber = contract.ContractNumber,
            ContractTypeCode = contract.ContractTypeCode,
            StartDate = contract.StartDate,
            EndDate = contract.EndDate,
            SignedDate = contract.SignedDate,
            StatusCode = contract.StatusCode,
            Notes = contract.Notes,
            AttachmentFileId = contract.AttachmentFileId,
            PreviousContractId = contract.PreviousContractId?.ToString(),
            CreatedBy = contract.CreatedBy,
            CreatedAt = contract.CreatedAt,
            UpdatedBy = contract.UpdatedBy,
            UpdatedAt = contract.UpdatedAt
        };
    }

    public EmployeeContractDetailResponse ToDetailResponse(EmployeeContract contract)
    {
        if (contract is null) return null;
        return new EmployeeContractDetailResponse
        {
            Id = contract.Id.Value.ToString(),
            EmployeeId = contract.EmployeeId.Value.ToString(),
            ContractNumber = contract.ContractNumber,
            ContractTypeCode = contract.ContractTypeCode,
            StartDate = contract.StartDate,
            EndDate = contract.EndDate,
            SignedDate = contract.SignedDate,
            StatusCode = contract.StatusCode,
            Notes = contract.Notes,
            AttachmentFileId = contract.AttachmentFileId,
            PreviousContractId = contract.PreviousContractId?.ToString(),
            CreatedBy = contract.CreatedBy,
            CreatedAt = contract.CreatedAt,
            UpdatedBy = contract.UpdatedBy,
            UpdatedAt = contract.UpdatedAt,
            
            // Termination Details
            TerminatedDate = contract.TerminationDetail?.TerminatedDate,
            TerminationReasonCode = contract.TerminationDetail?.ReasonCode,
            TerminationNotes = contract.TerminationDetail?.Notes,
            TerminationAttachmentId = contract.TerminationDetail?.TerminationAttachmentId
        };
    }

    public IReadOnlyCollection<EmployeeContractResponse> ToResponses(IEnumerable<EmployeeContract> contracts)
    {
        if (contracts is null) return [];
        return contracts.Select(ToResponse).ToList();
    }
}
