using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using VisaBot.Config;
using VisaBot.Workers;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

try
{
    var host = Host.CreateDefaultBuilder(args)
        .UseSerilog()
        .ConfigureServices((context, services) =>
        {
            services.Configure<ConfigSettings>(context.Configuration.GetSection(ConfigSettings.SectionName));
            services.AddHostedService<VisaBotWorker>();
        })
        .Build();

    await host.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Uygulama sonlandı.");
    Environment.ExitCode = 1;
}
finally
{
    Log.CloseAndFlush();
}
