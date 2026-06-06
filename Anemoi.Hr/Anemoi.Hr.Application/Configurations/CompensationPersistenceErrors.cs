using System;
using Microsoft.EntityFrameworkCore;

namespace Anemoi.Hr.Application.Configurations;

internal static class CompensationPersistenceErrors
{
    public static bool IsUniqueConstraintViolation(Exception exception)
    {
        if (exception is not DbUpdateException dbUpdateException)
            return false;

        var innerException = dbUpdateException.InnerException;
        if (innerException is null)
            return false;

        var sqlState = innerException.GetType().GetProperty("SqlState")?.GetValue(innerException) as string;
        return string.Equals(sqlState, "23505", StringComparison.Ordinal);
    }
}
