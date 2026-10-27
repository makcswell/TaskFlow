using Ebceys.Infrastructure;
using NotificationService;

await WebApplicationBase<Startup>
    .Create(args)
    .Build(args)
    .RunAsync();