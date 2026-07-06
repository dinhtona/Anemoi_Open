using System.Reflection;
using Anemoi.Contract.Identity;
using Anemoi.Contract.MasterData;
using Anemoi.Contract.Workspace;
using Anemoi.Contract.Notification;

namespace Anemoi.Centralize.Application.ContractAssemblies;

public static class ContractAssembly
{
    public static IEnumerable<Assembly> GetAllContractAssemblies() =>
    [
        typeof(IIdentityContractAssemblyMarker).Assembly,
        typeof(IMasterDataContractAssemblyMarker).Assembly,
        typeof(IWorkspaceContractAssemblyMarker).Assembly,
        typeof(INotificationContractAssemblyMarker).Assembly
    ];
}