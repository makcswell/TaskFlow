using Ebceys.Infrastructure.Interfaces;
using Ebceys.Infrastructure.TestApplication.DaL;
using Microsoft.EntityFrameworkCore;

namespace Ebceys.Infrastructure.TestApplication.Commands;

// Registration in DI example:
// services.AddScopedCommand<GetEntitiesCommand, GetEntitiesCommandContext, GetEntitiesCommandResult>();

public record GetEntitiesCommandContext;

public record GetEntitiesCommandResult(IReadOnlyCollection<TestTableDbo> Dbos);

public class GetEntitiesCommand(IDbContextFactory<DataModelContext> dbFactory)
    : ICommand<GetEntitiesCommandContext, GetEntitiesCommandResult>
{
    public async Task<GetEntitiesCommandResult> ExecuteAsync(GetEntitiesCommandContext context,
        CancellationToken token = default)
    {
        await using var dbContext = await dbFactory.CreateDbContextAsync(token);

        var result = await dbContext.TestTable.AsNoTracking().ToArrayAsync(token);

        return new GetEntitiesCommandResult(result);
    }
}