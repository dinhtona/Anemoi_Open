using Anemoi.BuildingBlock.Domain;
using Anemoi.Hr.ModelIds.ModelIds;
using System;
using System.Collections.Generic;

namespace Anemoi.Hr.Domain.Recruitment;

public sealed class JobPosting : ValueObject
{
    public JobPostingId Id { get; set; }
    public JobRequisitionId JobRequisitionId { get; set; }
    public string PostingTitle { get; set; }
    public string PostingDescription { get; set; }
    public DateOnly PublishDate { get; set; }
    public DateOnly ExpiryDate { get; set; }
    public string Status { get; private set; } = JobPostingStatusCode.Draft;
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string UpdatedBy { get; set; }
    public DateTime? PublishedAt { get; private set; }
    public string PublishedBy { get; private set; }
    public DateTime? ExpiredAt { get; private set; }
    public string ExpiredBy { get; private set; }
    public DateTime? ClosedAt { get; private set; }
    public string ClosedBy { get; private set; }

    // Navigation
    public JobRequisition JobRequisition { get; set; }

    public bool UpdateDetails(string title, string description, DateOnly publishDate, DateOnly expiryDate)
    {
        if (Status == JobPostingStatusCode.Closed || Status == JobPostingStatusCode.Expired)
            return false;
        if (publishDate > expiryDate)
            return false;

        PostingTitle = title;
        PostingDescription = description;
        PublishDate = publishDate;
        ExpiryDate = expiryDate;
        return true;
    }

    public bool Publish(string actor, DateTime now)
    {
        if (Status != JobPostingStatusCode.Draft && Status != JobPostingStatusCode.Expired)
            return false;

        if (PublishDate > ExpiryDate)
            return false;

        Status = JobPostingStatusCode.Published;
        PublishedBy = actor;
        PublishedAt = now;
        UpdatedBy = actor;
        UpdatedAt = now;
        return true;
    }

    public bool Expire(string actor, DateTime now)
    {
        if (Status != JobPostingStatusCode.Published)
            return false;

        Status = JobPostingStatusCode.Expired;
        ExpiredBy = actor;
        ExpiredAt = now;
        UpdatedBy = actor;
        UpdatedAt = now;
        return true;
    }

    public bool Close(string actor, DateTime now)
    {
        if (Status == JobPostingStatusCode.Closed)
            return false;

        Status = JobPostingStatusCode.Closed;
        ClosedBy = actor;
        ClosedAt = now;
        UpdatedBy = actor;
        UpdatedAt = now;
        return true;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Id;
    }
}
