using System.Threading;
using System.Threading.Tasks;
using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.BuildingBlock.Application.Extensions;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.BuildingBlock.Application.Results;
using Anemoi.Contract.Identity.Commands.UserCommands.RevokeUserSession;
using Anemoi.Identity.Application.Abstractions;
using OneOf;

namespace Anemoi.Identity.Application.Cqrs.Commands.UserCommands.RevokeUserSession;

public sealed class RevokeUserSessionHandler(
    IUserSessionRevocationService sessionRevocationService)
    : ICommandHandler<RevokeUserSessionCommand, OneOf<None, ErrorDetailResponse>>
{
    public async Task<OneOf<None, ErrorDetailResponse>> Handle(
        RevokeUserSessionCommand request, CancellationToken cancellationToken)
    {
        var result = await sessionRevocationService.RevokeAsync([request.UserId], cancellationToken);
        return result.MapT1(error => error.ToErrorDetailResponse());
    }
}
