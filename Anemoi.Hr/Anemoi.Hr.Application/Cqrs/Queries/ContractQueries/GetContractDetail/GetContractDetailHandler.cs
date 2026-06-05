using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Mappings;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Contracts;
using OneOf;
using System.Threading;
using System.Threading.Tasks;

namespace Anemoi.Hr.Application.Cqrs.Queries.ContractQueries.GetContractDetail;

public sealed class GetContractDetailHandler(
    ISqlRepository<EmployeeContract> contractRepository,
    EmployeeContractMapper mapper)
    : IQueryHandler<GetContractDetailQuery, OneOf<EmployeeContractDetailResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<EmployeeContractDetailResponse, ErrorDetailResponse>> Handle(
        GetContractDetailQuery request,
        CancellationToken cancellationToken)
    {
        // 1. Fetch contract by ID
        var contract = await contractRepository.GetFirstByConditionAsync(x => x.Id == request.Id, null, cancellationToken);
        if (contract is null)
            return HrErrorResponses.Create(HrBusinessErrorCodes.ContractNotFound);

        // 2. Map and return detail response
        return mapper.ToDetailResponse(contract);
    }
}
