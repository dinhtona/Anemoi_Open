using System;
using System.Collections.Generic;

namespace Anemoi.Hr.Application.Responses;

public sealed record RecruitmentOverviewResponse(
    int OpenRequisitions,
    int ApprovedRequisitions,
    int PublishedPostings,
    int ActiveCandidates,
    int ApplicationsInPipeline,
    int HiredCandidates,
    int ConvertedCandidates,
    int PendingInterviews
);

public sealed record ApplicationsByStageItem(string Stage, int Count);

public sealed record HiringByDepartmentItem(
    string DepartmentId,
    string DepartmentName,
    int RequisitionCount,
    int ApplicationCount,
    int HireCount,
    int ConversionCount
);

public sealed record CandidateSourceEffectivenessItem(
    string Source,
    int CandidateCount,
    int ApplicationCount,
    int HireCount,
    int ConversionCount,
    double HireRate,
    double ConversionRate
);

public sealed record TimeToHireResponse(
    double AverageDaysToHire,
    double? MedianDaysToHire,
    int MinDaysToHire,
    int MaxDaysToHire,
    int Count
);

public sealed record RecruitmentAnalyticsDashboardResponse(
    RecruitmentOverviewResponse Overview,
    IReadOnlyCollection<ApplicationsByStageItem> ApplicationsByStage,
    IReadOnlyCollection<HiringByDepartmentItem> HiringByDepartment,
    IReadOnlyCollection<CandidateSourceEffectivenessItem> CandidateSourceEffectiveness,
    TimeToHireResponse TimeToHire
);
