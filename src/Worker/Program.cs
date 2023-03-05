using Contract;
using MessageHandler.Runtime;
using MessageHandler.Runtime.AtomicProcessing;
using Worker;


IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext, services) =>
    {
        services.AddLogging();

        var serviceBusConnectionString = hostContext.Configuration.GetValue<string>("servicebusnamespace")
                                                ?? throw new Exception("No 'servicebusnamespace' was provided. Use User Secrets or specify via environment variable.");

        var sqlServerConnectionString = hostContext.Configuration.GetValue<string>("sqlserverconnectionstring")
                              ?? throw new Exception("No 'sqlserverconnectionstring' was provided. Use User Secrets or specify via environment variable.");
           
        services.AddMessageHandler("emailprocessor", runtimeConfiguration =>
        {
            runtimeConfiguration.ImmediateDispatchingPipeline(dispatching =>
            {
                dispatching.RouteMessagesOfType<SendEmailCommand>(to => to.Queue("emails", serviceBusConnectionString));
            });
        });

        services.AddSingleton<IPersistAvailableConfirmationMails>(new PersistAvailableConfirmationMails(sqlServerConnectionString));
        services.AddSingleton<SendAvailableConfirmationMails>();
        services.AddHostedService<ConfirmationMailWorker>();

        services.AddHostedService(sp => new InsertAvailableConfirmationMail(sqlServerConnectionString, sp.GetRequiredService<ILogger<InsertAvailableConfirmationMail>>()));
    })
    .Build();

await host.RunAsync();
