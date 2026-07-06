using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Payroll;
using Riok.Mapperly.Abstractions;
using System.Collections.Generic;
using System.Linq;

namespace Anemoi.Hr.Application.Mappings;

[Mapper]
public partial class PayslipDocumentMapper
{
    public PayslipDocumentResponse ToResponse(PayslipDocument entity)
    {
        if (entity is null) return null!;
        return new PayslipDocumentResponse
        {
            Id = entity.Id.Value,
            PayslipId = entity.PayslipId.Value,
            FileName = entity.FileName,
            ContentType = entity.ContentType,
            StoragePath = entity.StoragePath,
            FileSize = entity.FileSize,
            ChecksumHash = entity.ChecksumHash,
            GeneratedBy = entity.GeneratedBy,
            GeneratedAt = entity.GeneratedAt,
            Version = entity.Version,
            IsActive = entity.IsActive
        };
    }

    public IReadOnlyCollection<PayslipDocumentResponse> ToResponses(IEnumerable<PayslipDocument> entities)
    {
        if (entities is null) return [];
        return entities.Select(ToResponse).ToList();
    }

    public PayslipEmailDeliveryResponse ToResponse(PayslipEmailDelivery entity)
    {
        if (entity is null) return null!;
        return new PayslipEmailDeliveryResponse
        {
            Id = entity.Id.Value,
            PayslipId = entity.PayslipId.Value,
            PayslipDocumentId = entity.PayslipDocumentId.Value,
            ToEmail = entity.ToEmail,
            Subject = entity.Subject,
            Status = entity.Status.ToString(),
            ErrorMessage = entity.ErrorMessage,
            SentBy = entity.SentBy,
            SentAt = entity.SentAt,
            FailedAt = entity.FailedAt,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };
    }

    public IReadOnlyCollection<PayslipEmailDeliveryResponse> ToResponses(IEnumerable<PayslipEmailDelivery> entities)
    {
        if (entities is null) return [];
        return entities.Select(ToResponse).ToList();
    }
}
