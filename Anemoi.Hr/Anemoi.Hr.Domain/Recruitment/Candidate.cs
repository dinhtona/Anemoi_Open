using Anemoi.BuildingBlock.Domain;
using Anemoi.Hr.Domain.Employees;
using Anemoi.Hr.ModelIds.ModelIds;
using System;
using System.Collections.Generic;

namespace Anemoi.Hr.Domain.Recruitment;

public sealed class Candidate : ValueObject
{
    public CandidateId Id { get; set; }
    public string CandidateCode { get; set; }
    public string FullName { get; set; }
    public string Email { get; set; }
    public string PhoneNumber { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    public string Address { get; set; }
    public string ResumeUrl { get; set; }
    public string Source { get; set; }
    public string Status { get; private set; } = CandidateStatusCode.Active;
    public string Notes { get; set; }
    public EmployeeId? EmployeeId { get; private set; }
    public DateTime? ConvertedAt { get; private set; }
    public string ConvertedBy { get; private set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string UpdatedBy { get; set; }

    // Navigation
    public Employee Employee { get; set; }

    public bool UpdateProfile(string fullName, string email, string phoneNumber,
        DateOnly? dateOfBirth, string address, string resumeUrl, string notes)
    {
        if (string.IsNullOrWhiteSpace(fullName))
            return false;
        if (string.IsNullOrWhiteSpace(email))
            return false;

        FullName = fullName;
        Email = email;
        PhoneNumber = phoneNumber;
        DateOfBirth = dateOfBirth;
        Address = address;
        ResumeUrl = resumeUrl;
        Notes = notes;
        return true;
    }

    public bool ChangeSource(string source, string actor)
    {
        Source = source;
        UpdatedBy = actor;
        UpdatedAt = DateTime.UtcNow;
        return true;
    }

    public bool Blacklist(string actor, DateTime now)
    {
        if (Status == CandidateStatusCode.Blacklisted)
            return false;

        Status = CandidateStatusCode.Blacklisted;
        UpdatedBy = actor;
        UpdatedAt = now;
        return true;
    }

    public bool Archive(string actor, DateTime now)
    {
        if (Status == CandidateStatusCode.Archived)
            return false;

        Status = CandidateStatusCode.Archived;
        UpdatedBy = actor;
        UpdatedAt = now;
        return true;
    }

    public bool Reactivate(string actor, DateTime now)
    {
        if (Status == CandidateStatusCode.Blacklisted)
            return false;

        if (Status == CandidateStatusCode.Active)
            return false;

        Status = CandidateStatusCode.Active;
        UpdatedBy = actor;
        UpdatedAt = now;
        return true;
    }

    public bool LinkEmployee(EmployeeId employeeId, string actor, DateTime now)
    {
        if (EmployeeId is not null)
            return false;

        EmployeeId = employeeId;
        ConvertedBy = actor;
        ConvertedAt = now;
        UpdatedBy = actor;
        UpdatedAt = now;
        return true;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Id;
    }
}
