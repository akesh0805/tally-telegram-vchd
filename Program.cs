using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot;
using Microsoft.Extensions.Configuration;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureServices(services =>
    {
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();

        services.AddTransient<ITelegramBotClient, TelegramBotClient>(provider =>
        {
            var token = provider.GetRequiredService<IConfiguration>()["Bot:Token"];
            return new TelegramBotClient(token ?? throw new Exception("Telegram bot token not configured."));
        });
    })
    .Build();

host.Run();
