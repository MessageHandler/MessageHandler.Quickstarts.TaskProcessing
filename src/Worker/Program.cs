using Contract;
using MessageHandler.Runtime;
using MessageHandler.Runtime.AtomicProcessing;
using Worker;


IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext, services) =>
    {
        services.AddLogging();

        var serviceBusConnectionString = hostContext.Configuration.GetValue<string>("ServiceBusConnectionString")
                                                ?? throw new Exception("No 'ServiceBusConnectionString' was provided. Use User Secrets or specify via environment variable.");

        var sqlServerConnectionString = hostContext.Configuration.GetValue<string>("SqlServerConnectionString")
                              ?? throw new Exception("No 'SqlServerConnectionString' was provided. Use User Secrets or specify via environment variable.");
           
        services.AddMessageHandler("emailprocessor", runtimeConfiguration =>
        {
            runtimeConfiguration.ImmediateDispatchingPipeline(dispatching =>
            {
                dispatching.RouteMessagesOfType<SendEmailCommand>(to => to.Queue("emails", serviceBusConnectionString));
            });
        });

        services.AddSingleton<IProcessAvailableConfirmationMails>(new AvailableConfirmationMails(sqlServerConnectionString));
        services.AddHostedService<SendConfirmationMail>();

        return;
    })
    .Build();

await host.RunAsync();
