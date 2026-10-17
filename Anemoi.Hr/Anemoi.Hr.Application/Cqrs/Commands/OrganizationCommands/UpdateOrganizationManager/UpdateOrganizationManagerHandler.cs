using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Employees;
using Anemoi.Hr.ModelIds.ModelIds;
using Microsoft.EntityFrameworkCore;
using OneOf;

namespace Anemoi.Hr.Application.Cqrs.Commands.OrganizationCommands.UpdateOrganizationManager;

public sealed class UpdateOrganizationManagerHandler(
    ISqlRepository<Employee> employeeRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<UpdateOrganizationManagerCommand, OneOf<SuccessResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<SuccessResponse, ErrorDetailResponse>> Handle(
        UpdateOrganizationManagerCommand request, CancellationToken cancellationToken)
    {
        var employee = await employeeRepository.GetQueryable()
            .FirstOrDefaultAsync(e => e.Id == request.EmployeeId, cancellationToken);

        if (employee is null)
            return HrErrorResponses.Create(HrBusinessErrorCodes.EmployeeNotFound);

        if (request.ManagerId is not null)
        {
            if (request.ManagerId == request.EmployeeId)
                return HrErrorResponses.Create(HrBusinessErrorCodes.WorkflowApproverNotFound);

            var manager = await employeeRepository.GetQueryable()
                .FirstOrDefaultAsync(e => e.Id == request.ManagerId, cancellationToken);
            if (manager is null)
                return HrErrorResponses.Create(HrBusinessErrorCodes.EmployeeNotFound);

            // Load full manager chain in one query to detect circular hierarchy
            var managerChain = await employeeRepository.GetQueryable()
                .Select(e => new { e.Id, e.DirectManagerEmployeeId })
                .ToDictionaryAsync(e => e.Id, cancellationToken);

            var visited = new HashSet<EmployeeId> { request.EmployeeId };
            var currentManagerId = manager.DirectManagerEmployeeId;
            while (currentManagerId is not null)
            {
                if (!visited.Add(currentManagerId))
                    return HrErrorResponses.Create(HrBusinessErrorCodes.WorkflowApproverNotFound);
                if (!managerChain.TryGetValue(currentManagerId, out var mgr)) break;
                currentManagerId = mgr.DirectManagerEmployeeId;
            }
        }

        employee.DirectManagerEmployeeId = request.ManagerId;
        employee.UpdatedAt = DateTime.UtcNow;

        var saveResult = await unitOfWork.SaveChangesAsync(cancellationToken);
        if (saveResult.IsT1)
            return HrErrorResponses.FromSaveResult(saveResult.AsT1, HrBusinessErrorCodes.SaveChangesFailed);

        return new SuccessResponse();
    }
}
