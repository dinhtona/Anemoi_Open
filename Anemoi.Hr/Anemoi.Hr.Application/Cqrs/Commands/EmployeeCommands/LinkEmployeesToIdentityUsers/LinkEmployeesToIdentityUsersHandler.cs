using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.BuildingBlock.Application.Helpers;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Employees;
using Anemoi.Hr.ModelIds.ModelIds;
using Microsoft.EntityFrameworkCore;
using OneOf;

namespace Anemoi.Hr.Application.Cqrs.Commands.EmployeeCommands.LinkEmployeesToIdentityUsers;

public sealed class LinkEmployeesToIdentityUsersHandler(
    ISqlRepository<Employee> employeeRepository,
    ISqlRepository<EmployeeIdentityLinkLog> linkLogRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<LinkEmployeesToIdentityUsersCommand, OneOf<EmployeeIdentityLinkResultResponse, ErrorDetailResponse>>
{
    private const string SystemCreatedBy = "system:employee-identity-linker";

    public async Task<OneOf<EmployeeIdentityLinkResultResponse, ErrorDetailResponse>> Handle(
        LinkEmployeesToIdentityUsersCommand request,
        CancellationToken cancellationToken)
    {
        var createdBy = string.IsNullOrWhiteSpace(request.CreatedBy) ? SystemCreatedBy : request.CreatedBy;
        var now = DateTime.UtcNow;
        var result = new EmployeeIdentityLinkResultResponse();
        var candidatesByEmail = (request.Candidates ?? [])
            .Where(x => !string.IsNullOrWhiteSpace(x.Email))
            .GroupBy(x => NormalizeEmail(x.Email))
            .ToDictionary(x => x.Key, x => x.ToList());
        var employees = await employeeRepository.GetQueryable()
            .OrderBy(x => x.EmployeeCode)
            .ToListAsync(cancellationToken);
        var employeeEmailCounts = employees
            .Select(x => NormalizeEmail(x.WorkEmail))
            .Where(x => x is not null && ValidationHelper.IsEmail(x))
            .GroupBy(x => x)
            .ToDictionary(x => x.Key, x => x.Count());
        var logs = new List<EmployeeIdentityLinkLog>();

        foreach (var employee in employees)
        {
            result.TotalEvaluated++;
            var normalizedEmail = NormalizeEmail(employee.WorkEmail);
            var status = ResolveStatus(employee, normalizedEmail, employeeEmailCounts, candidatesByEmail,
                out var matchedIdentityUserId);

            if (status == EmployeeIdentityLinkMatchStatuses.Matched && !request.DryRun)
            {
                employee.IdentityUserId = matchedIdentityUserId;
            }

            Increment(result, status);
            logs.Add(new EmployeeIdentityLinkLog
            {
                Id = new EmployeeIdentityLinkLogId(IdGenerator.NextGuid()),
                EmployeeId = employee.Id,
                WorkEmail = employee.WorkEmail,
                IdentityUserId = matchedIdentityUserId,
                MatchStatus = status,
                CreatedAt = now,
                CreatedBy = createdBy,
                IsDryRun = request.DryRun
            });
        }

        if (logs.Count > 0)
        {
            await linkLogRepository.CreateManyAsync(logs, cancellationToken);
        }

        var saveResult = await unitOfWork.SaveChangesAsync(cancellationToken);
        return saveResult.Match<OneOf<EmployeeIdentityLinkResultResponse, ErrorDetailResponse>>(
            _ => result,
            _ => HrErrorResponses.Create(HrBusinessErrorCodes.SaveChangesFailed));
    }

    private static string ResolveStatus(
        Employee employee,
        string normalizedEmail,
        IReadOnlyDictionary<string, int> employeeEmailCounts,
        IReadOnlyDictionary<string, List<IdentityUserLinkCandidate>> candidatesByEmail,
        out Guid? matchedIdentityUserId)
    {
        matchedIdentityUserId = null;
        if (string.IsNullOrWhiteSpace(employee.WorkEmail)) return EmployeeIdentityLinkMatchStatuses.Skipped;
        if (normalizedEmail is null || !ValidationHelper.IsEmail(normalizedEmail))
            return EmployeeIdentityLinkMatchStatuses.InvalidEmail;
        if (employeeEmailCounts.TryGetValue(normalizedEmail, out var employeeEmailCount) && employeeEmailCount > 1)
            return EmployeeIdentityLinkMatchStatuses.DuplicateEmployeeEmail;
        if (!candidatesByEmail.TryGetValue(normalizedEmail, out var candidates))
            return EmployeeIdentityLinkMatchStatuses.NoMatch;
        if (candidates.Count > 1) return EmployeeIdentityLinkMatchStatuses.DuplicateIdentityEmail;

        matchedIdentityUserId = candidates[0].IdentityUserId;
        if (employee.IdentityUserId is null) return EmployeeIdentityLinkMatchStatuses.Matched;
        return employee.IdentityUserId == matchedIdentityUserId
            ? EmployeeIdentityLinkMatchStatuses.AlreadyLinked
            : EmployeeIdentityLinkMatchStatuses.ConflictExistingMapping;
    }

    private static string NormalizeEmail(string email) =>
        string.IsNullOrWhiteSpace(email) ? null : email.Trim().ToLowerInvariant();

    private static void Increment(EmployeeIdentityLinkResultResponse result, string status)
    {
        switch (status)
        {
            case EmployeeIdentityLinkMatchStatuses.Matched:
                result.Matched++;
                break;
            case EmployeeIdentityLinkMatchStatuses.NoMatch:
                result.NoMatch++;
                break;
            case EmployeeIdentityLinkMatchStatuses.AlreadyLinked:
                result.AlreadyLinked++;
                break;
            case EmployeeIdentityLinkMatchStatuses.ConflictExistingMapping:
                result.ConflictExistingMapping++;
                break;
            case EmployeeIdentityLinkMatchStatuses.DuplicateEmployeeEmail:
                result.DuplicateEmployeeEmail++;
                break;
            case EmployeeIdentityLinkMatchStatuses.DuplicateIdentityEmail:
                result.DuplicateIdentityEmail++;
                break;
            case EmployeeIdentityLinkMatchStatuses.InvalidEmail:
                result.InvalidEmail++;
                break;
            default:
                result.Skipped++;
                break;
        }
    }
}

public static class EmployeeIdentityLinkMatchStatuses
{
    public const string Matched = "Matched";
    public const string NoMatch = "NoMatch";
    public const string AlreadyLinked = "AlreadyLinked";
    public const string ConflictExistingMapping = "ConflictExistingMapping";
    public const string DuplicateEmployeeEmail = "DuplicateEmployeeEmail";
    public const string DuplicateIdentityEmail = "DuplicateIdentityEmail";
    public const string InvalidEmail = "InvalidEmail";
    public const string Skipped = "Skipped";
}
