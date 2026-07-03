using Anemoi.BuildingBlock.Application.BulkImport.Models;
using Anemoi.BuildingBlock.Application.BulkImport.Services;
using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.BulkImport.EmployeeImport;
using Anemoi.Hr.Application.Responses;
using OneOf;

namespace Anemoi.Hr.Application.Cqrs.Queries.EmployeeImport;

public sealed class GetImportTemplateHandler(
    BulkExcelTemplateService templateService,
    EmployeeImportTemplateProvider templateProvider)
    : IQueryHandler<GetImportTemplateQuery, OneOf<FileResponse, ErrorDetailResponse>>
{
    public Task<OneOf<FileResponse, ErrorDetailResponse>> Handle(
        GetImportTemplateQuery request, CancellationToken ct)
    {
        var bytes = templateService.GenerateTemplate(templateProvider.Columns);
        return Task.FromResult<OneOf<FileResponse, ErrorDetailResponse>>(
            new FileResponse(bytes, BulkImportConstants.ExcelContentType, templateProvider.TemplateFileName));
    }
}
