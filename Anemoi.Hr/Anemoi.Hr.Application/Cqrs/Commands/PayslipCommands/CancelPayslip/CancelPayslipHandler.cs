using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.BuildingBlock.Application.Helpers;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Mappings;
using Anemoi.Contract.Hr.Events;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Payroll;
using Anemoi.Hr.ModelIds.ModelIds;
using MassTransit;
using OneOf;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Anemoi.Hr.Application.Cqrs.Commands.PayslipCommands.CancelPayslip;

public sealed class CancelPayslipHandler(
    ISqlRepository<Payslip> payslipRepository,
    IUnitOfWork unitOfWork,
    IPublishEndpoint publishEndpoint,
    PayslipMapper mapper)
    : ICommandHandler<CancelPayslipCommand, OneOf<PayslipResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<PayslipResponse, ErrorDetailResponse>> Handle(
        CancelPayslipCommand request,
        CancellationToken cancellationToken)
    {
        var payslip = await payslipRepository.GetFirstByConditionAsync(
            x => x.Id == request.PayslipId,
            null,
            cancellationToken);

        if (payslip is null)
            return HrErrorResponses.Create(HrBusinessErrorCodes.PayslipNotFound);

        if (!payslip.Cancel(request.CancelledBy ?? PayrollConstants.SystemActor, DateTime.UtcNow))
            return HrErrorResponses.Create(HrBusinessErrorCodes.PayslipInvalidStatus);

        await publishEndpoint.Publish(new PayslipCancelledIntegrationEvent(
            payslip.Id.Value.ToString(),
            payslip.EmployeeId.Value.ToString()), cancellationToken);

        var saveResult = await unitOfWork.SaveChangesAsync(cancellationToken);
        if (saveResult.IsT1)
            return HrErrorResponses.FromSaveResult(saveResult.AsT1, null);

        return mapper.ToResponse(payslip);
    }
}
