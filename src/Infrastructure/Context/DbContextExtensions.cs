using Application.Errors;
using FluentResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Context;

public static class DbContextExtensions
{
    public static async Task<Result> SaveChangesResultAsync(
        this DbContext context,
        ILogger logger,
        Func<BaseError> errorFactory)
    {
        try
        {
            await context.SaveChangesAsync();
            return Result.Ok();
        }
        catch (DbUpdateConcurrencyException e)
        {
            logger.LogError(e, "Concurrency error while saving changes");
            return Result.Fail(errorFactory());
        }
        catch (DbUpdateException e)
        {
            logger.LogError(e, "Database update error");
            return Result.Fail(errorFactory());
        }
        catch (Exception e)
        {
            logger.LogError(e, "Unexpected error when saving changes");
            return Result.Fail(errorFactory());
        }
    }
}
