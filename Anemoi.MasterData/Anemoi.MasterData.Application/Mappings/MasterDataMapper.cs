using Anemoi.BuildingBlock.Application.Errors;
using Anemoi.BuildingBlock.Application.Extensions;
using Anemoi.BuildingBlock.Application.Helpers;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Contract.MasterData.Commands.DistrictCommands.CreateDistrict;
using Anemoi.Contract.MasterData.Commands.DistrictCommands.UpdateDistrict;
using Anemoi.Contract.MasterData.Commands.ProvinceCommands.CreateProvince;
using Anemoi.Contract.MasterData.Commands.ProvinceCommands.UpdateProvince;
using Anemoi.Contract.MasterData.Commands.SeedFunctionCommands.CreateSeedFunction;
using Anemoi.Contract.MasterData.Commands.SeedFunctionCommands.UpdateSeedFunction;
using Anemoi.Contract.MasterData.Commands.SeedServerCommands.CreateSeedServer;
using Anemoi.Contract.MasterData.Commands.SeedServerCommands.UpdateSeedServer;
using Anemoi.Contract.MasterData.Commands.SeedTemplateCommands.CreateSeedTemplate;
using Anemoi.Contract.MasterData.Commands.SeedTemplateCommands.UpdateSeedTemplate;
using Anemoi.Contract.MasterData.ModelIds;
using Anemoi.Contract.MasterData.Responses;
using Anemoi.MasterData.Domain.Models;
using Riok.Mapperly.Abstractions;


namespace Anemoi.MasterData.Application.Mappings;

[Mapper]
public partial class MasterDataMapper
{
    // Province Mappings
    public Province ToProvince(CreateProvinceCommand command)
    {
        var province = MapToProvince(command);
        province.Id = new ProvinceId(IdGenerator.NextGuid());
        province.Slug = command.Name.GenerateSlug();
        province.SearchHint = command.Name.GenerateSearchHint();
        return province;
    }

    [MapperIgnoreTarget(nameof(Province.Id))]
    [MapperIgnoreTarget(nameof(Province.Slug))]
    [MapperIgnoreTarget(nameof(Province.SearchHint))]
    private partial Province MapToProvince(CreateProvinceCommand command);

    public void UpdateProvince(UpdateProvinceCommand command, Province province)
    {
        MapToProvince(command, province);
        if (command.Name is { })
        {
            province.Slug = command.Name.GenerateSlug();
            province.SearchHint = command.Name.GenerateSearchHint();
        }
    }

    [MapperIgnoreTarget(nameof(Province.Id))]
    [MapperIgnoreTarget(nameof(Province.Slug))]
    [MapperIgnoreTarget(nameof(Province.SearchHint))]
    private partial void MapToProvince(UpdateProvinceCommand command, Province province);

    public partial ProvinceResponse ToProvinceResponse(Province province);
    public partial IQueryable<ProvinceResponse> ProjectToProvinceResponse(IQueryable<Province> query);

    // District Mappings
    public District ToDistrict(CreateDistrictCommand command)
    {
        var district = MapToDistrict(command);
        district.Id = new DistrictId(IdGenerator.NextGuid());
        district.Slug = command.Name.GenerateSlug();
        district.SearchHint = command.Name.GenerateSearchHint();
        return district;
    }

    [MapperIgnoreTarget(nameof(District.Id))]
    [MapperIgnoreTarget(nameof(District.Slug))]
    [MapperIgnoreTarget(nameof(District.SearchHint))]
    private partial District MapToDistrict(CreateDistrictCommand command);

    public void UpdateDistrict(UpdateDistrictCommand command, District district)
    {
        MapToDistrict(command, district);
        if (command.Name is { })
        {
            district.Slug = command.Name.GenerateSlug();
            district.SearchHint = command.Name.GenerateSearchHint();
        }
    }

    [MapperIgnoreTarget(nameof(District.Id))]
    [MapperIgnoreTarget(nameof(District.Slug))]
    [MapperIgnoreTarget(nameof(District.SearchHint))]
    [MapperIgnoreTarget(nameof(District.ProvinceId))]
    private partial void MapToDistrict(UpdateDistrictCommand command, District district);

    public partial DistrictResponse ToDistrictResponse(District district);
    public partial IQueryable<DistrictResponse> ProjectToDistrictResponse(IQueryable<District> query);

    // Seed Data Mappings
    public SeedServer ToSeedServer(CreateSeedServerCommand command)
    {
        var server = MapToSeedServer(command);
        server.Id = new SeedServerId(IdGenerator.NextGuid());
        return server;
    }

    [MapperIgnoreTarget(nameof(SeedServer.Id))]
    private partial SeedServer MapToSeedServer(CreateSeedServerCommand command);

    public partial void UpdateSeedServer(UpdateSeedServerCommand command, SeedServer server);

    public SeedFunction ToSeedFunction(CreateSeedFunctionCommand command)
    {
        var function = MapToSeedFunction(command);
        function.Id = new SeedFunctionId(IdGenerator.NextGuid());
        return function;
    }

    [MapperIgnoreTarget(nameof(SeedFunction.Id))]
    private partial SeedFunction MapToSeedFunction(CreateSeedFunctionCommand command);

    public partial void UpdateSeedFunction(UpdateSeedFunctionCommand command, SeedFunction function);

    public SeedTemplate ToSeedTemplate(CreateSeedTemplateCommand command)
    {
        var template = MapToSeedTemplate(command);
        template.Id = new SeedTemplateId(IdGenerator.NextGuid());
        return template;
    }

    [MapperIgnoreTarget(nameof(SeedTemplate.Id))]
    private partial SeedTemplate MapToSeedTemplate(CreateSeedTemplateCommand command);

    public partial void UpdateSeedTemplate(UpdateSeedTemplateCommand command, SeedTemplate template);

    public partial SeedServerResponse ToSeedServerResponse(SeedServer server);
    public partial IQueryable<SeedServerResponse> ProjectToSeedServerResponse(IQueryable<SeedServer> query);

    public partial SeedFunctionResponse ToSeedFunctionResponse(SeedFunction function);
    public partial IQueryable<SeedFunctionResponse> ProjectToSeedFunctionResponse(IQueryable<SeedFunction> query);

    public partial SeedTemplateResponse ToSeedTemplateResponse(SeedTemplate template);
    public partial IQueryable<SeedTemplateResponse> ProjectToSeedTemplateResponse(IQueryable<SeedTemplate> query);


    // Common Mappings
    public partial ErrorDetailResponse ToErrorDetailResponse(ErrorDetail errorDetail);
    public partial ErrorDetail ToErrorDetail(ErrorDetailResponse errorDetailResponse);
}
