using Ebceys.Infrastructure;
using TasksService;

await WebApplicationBase<Startup>
    .Create(args)
    .Build(args)
    .RunAsync();