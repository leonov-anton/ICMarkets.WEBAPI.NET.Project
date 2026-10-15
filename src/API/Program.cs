using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Formatting.Compact;

namespace ICMarkets.API;

// Public so integration tests can use WebApplicationFactory<Program>
public class Program
{
    public static void Main(string[] args)
    {
        CreateHostBuilder(args).Build().Run();
    }

    // WebApplicationFactory<Program> discovers the host through this method by name
    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .UseSerilog(ConfigureLogging)
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>();
            });

    // Levels come from the "Serilog" section; "LogFormat" is Json (for log collectors) or Text.
    private static void ConfigureLogging(HostBuilderContext context, LoggerConfiguration logger)
    {
        logger
            .ReadFrom.Configuration(context.Configuration)
            .Enrich.FromLogContext()
            .Enrich.WithProperty("Application", context.HostingEnvironment.ApplicationName);

        if (context.Configuration["LogFormat"] == "Json")
            logger.WriteTo.Console(new CompactJsonFormatter());
        else
            logger.WriteTo.Console(
                outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {CorrelationId} {SourceContext}: {Message:lj}{NewLine}{Exception}");
    }
}
