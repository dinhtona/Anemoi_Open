using Anemoi.Hr.Application.Responses;
using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using System;

namespace Anemoi.Hr.Application.Cqrs.Commands.PayslipCommands.GeneratePayslipPdf;

public sealed record GeneratePayslipPdfCommand(
    Guid PayslipId,
    string? GeneratedBy = null,
    bool ForceRegenerate = false) : ICommandResult<PayslipDocumentResponse>;
