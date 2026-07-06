using Anemoi.BuildingBlock.Application.Responses;

namespace Anemoi.Contract.Identity.Responses;

public sealed class UserRoleResponse : ModelResponse
{
    public string Name { get; set; }
    public string Group { get; set; }
    public string Description { get; set; }
}
