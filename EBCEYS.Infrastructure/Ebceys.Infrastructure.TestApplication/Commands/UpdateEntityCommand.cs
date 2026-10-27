using Ebceys.Infrastructure.Exceptions;
using Ebceys.Infrastructure.Interfaces;
using Ebceys.Infrastructure.TestApplication.DaL;
using Microsoft.EntityFrameworkCore;

namespace Ebceys.Infrastructure.TestApplication.Commands;

// Registration in DI example:
// services.AddScopedCommand<UpdateEntityCommand, UpdateEntityCommandContext, UpdateEntityCommandResult>();

public record UpdateEntityCommandContext(string CurrentName, string NewName);

public record UpdateEntityCommandResult;

public class UpdateEntityCommand(IDbContextFactory<DataModelContext> contextFactory)
    : ICommand<UpdateEntityCommandContext, UpdateEntityCommandResult>
{
    public async Task<UpdateEntityCommandResult> ExecuteAsync(UpdateEntityCommandContext context,
        CancellationToken token = default)
    {
        await using var dbContext = await contextFactory.CreateDbContextAsync(token);

        var existsEntity = await dbContext.TestTable.FirstOrDefaultAsync(t => t.Name == context.CurrentName, token);

        if (existsEntity is null)
        {
            return ApiExceptionHelper.ThrowNotFound<UpdateEntityCommandResult>("Entity not found");
        }

        existsEntity.Name = context.NewName;
        await dbContext.SaveChangesAsync(token);
        return new UpdateEntityCommandResult();
    }
}