using Anemoi.BuildingBlock.Application.Responses;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Serilog;

namespace Anemoi.Hr.Application.Configurations;

public static class HrErrorResponses
{
    public static ErrorDetailResponse Create(string code)
    {
        return new ErrorDetailResponse
        {
            Code = code,
            Messages = [code]
        };
    }

    public static ErrorDetailResponse FromSaveResult(
        Exception exception, string concurrencyCode)
    {
        var logger = Log.ForContext("SourceContext", nameof(HrErrorResponses));

        if (exception is DbUpdateConcurrencyException)
            return Create(concurrencyCode ?? HrBusinessErrorCodes.SaveChangesFailed);

        if (exception is DbUpdateException dbEx && dbEx.InnerException is { } innerEx)
        {
            var sqlState = innerEx is PostgresException pg
                ? pg.SqlState
                : innerEx.GetType().GetProperty("SqlState")?.GetValue(innerEx) as string;

            if (sqlState is not null)
            {
                logger.Error(innerEx,
                    "Database persistence error (SqlState: {SqlState})", sqlState);

                return sqlState switch
                {
                    PostgresErrorCodes.UniqueViolation => Create(HrBusinessErrorCodes.DbUniqueConstraint),
                    PostgresErrorCodes.ForeignKeyViolation => Create(HrBusinessErrorCodes.DbForeignKeyViolation),
                    PostgresErrorCodes.CheckViolation => Create(HrBusinessErrorCodes.DbCheckViolation),
                    _ => Create(HrBusinessErrorCodes.SaveChangesFailed)
                };
            }
        }

        logger.Error(exception, "SaveChanges failed with unexpected exception");
        return Create(HrBusinessErrorCodes.SaveChangesFailed);
    }
}
