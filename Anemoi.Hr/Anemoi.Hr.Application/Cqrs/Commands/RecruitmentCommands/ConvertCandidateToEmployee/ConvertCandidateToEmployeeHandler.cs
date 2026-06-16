using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.BuildingBlock.Application.Helpers;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Mappings;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Employees;
using Anemoi.Hr.Domain.Recruitment;
using Anemoi.Hr.ModelIds.ModelIds;
using Microsoft.EntityFrameworkCore;
using OneOf;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Anemoi.Hr.Application.Cqrs.Commands.RecruitmentCommands.ConvertCandidateToEmployee;

public sealed class ConvertCandidateToEmployeeHandler(
    ISqlRepository<Candidate> candidateRepository,
    ISqlRepository<CandidateApplication> applicationRepository,
    ISqlRepository<HiringDecision> decisionRepository,
    ISqlRepository<Employee> employeeRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<ConvertCandidateToEmployeeCommand, OneOf<CandidateConversionResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<CandidateConversionResponse, ErrorDetailResponse>> Handle(
        ConvertCandidateToEmployeeCommand request,
        CancellationToken cancellationToken)
    {
        var candidate = await candidateRepository.GetQueryable()
            .FirstOrDefaultAsync(x => x.Id == request.CandidateId, cancellationToken);
        if (candidate is null)
            return HrErrorResponses.Create(HrBusinessErrorCodes.CandidateNotFound);

        if (candidate.Status == CandidateStatusCode.Blacklisted ||
            candidate.Status == CandidateStatusCode.Archived)
            return HrErrorResponses.Create(HrBusinessErrorCodes.ConversionInvalidCandidateStatus);

        // Idempotent: if already linked, return existing
        if (candidate.EmployeeId is not null)
        {
            var existingEmployee = await employeeRepository.GetFirstByConditionAsync(
                x => x.Id == candidate.EmployeeId, token: cancellationToken);
            return new CandidateConversionResponse
            {
                CandidateId = candidate.Id.Value.ToString(),
                EmployeeId = candidate.EmployeeId.Value.ToString(),
                EmployeeCode = existingEmployee?.EmployeeCode,
                AlreadyConverted = true,
                RequiresContractCreation = true,
                ConvertedAt = candidate.ConvertedAt
            };
        }

        // Verify Hire decision exists
        var hireDecision = await decisionRepository.GetFirstByConditionAsync(
            x => x.CandidateApplication.CandidateId == request.CandidateId &&
                 x.Decision == HiringDecisionCode.Hire,
            q => q.OrderByDescending(x => x.DecidedAt),
            cancellationToken);
        if (hireDecision is null)
            return HrErrorResponses.Create(HrBusinessErrorCodes.ConversionRequiresHireDecision);

        // Verify application is in Hired stage
        var application = await applicationRepository.GetFirstByConditionAsync(
            x => x.CandidateId == request.CandidateId && x.CurrentStage == CandidateApplicationStageCode.Hired,
            token: cancellationToken);
        if (application is null)
            return HrErrorResponses.Create(HrBusinessErrorCodes.ConversionRequiresHiredStage);

        var now = DateTime.UtcNow;
        var employeeId = new EmployeeId(IdGenerator.NextGuid());

        var employee = new Employee
        {
            Id = employeeId,
            EmployeeCode = request.EmployeeCode,
            FullName = request.FullName,
            WorkEmail = request.WorkEmail,
            JoinDate = request.JoinDate,
            EmploymentStatusCode = EmploymentStatusCode.Active,
            EmploymentTypeCode = request.EmploymentTypeCode,
            GradeCode = "G1",
            PrimaryDepartmentId = request.DepartmentId,
            PrimaryPositionId = request.PositionId,
            CreatedAt = now,
            UpdatedAt = now
        };

        var createResult = await employeeRepository.CreateOneAsync(employee, cancellationToken);
        if (createResult.TryPickT1(out var exception, out _))
            return HrErrorResponses.FromSaveResult(exception,
                HrBusinessErrorCodes.ConversionEmployeeCreationFailed);

        // Link candidate to employee
        candidate.LinkEmployee(employeeId, request.ConvertedBy, now);

        var saveResult = await unitOfWork.SaveChangesAsync(cancellationToken);
        if (saveResult.TryPickT1(out var saveException, out _))
            return HrErrorResponses.FromSaveResult(saveException,
                HrBusinessErrorCodes.ConversionEmployeeCreationFailed);

        return new CandidateConversionResponse
        {
            CandidateId = candidate.Id.Value.ToString(),
            EmployeeId = employeeId.Value.ToString(),
            EmployeeCode = employee.EmployeeCode,
            AlreadyConverted = false,
            RequiresContractCreation = true,
            ConvertedAt = candidate.ConvertedAt
        };
    }
}
