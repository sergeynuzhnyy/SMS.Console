using System.Net.Http.Headers;
using System.Text;
using Core.Services;
using Infrastructure.Common;
using Infrastructure.Repositories;
using Library.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Compact;

namespace Infrastructure;

public static class Startup
{
    public static IHostBuilder ConfigureHost(this IHostBuilder hostBuilder, string[]? args) =>
        hostBuilder
            .ConfigureDefaults(args)
            .ConfigureConfiguration()
            .ConfigureLogging()
            .ConfigureServices();

    private static IHostBuilder ConfigureConfiguration(this IHostBuilder hostBuilder) =>
        hostBuilder
            .ConfigureAppConfiguration((context, builder) => { builder.AddJsonFile("appsettings.json", true, true); });


    private static IHostBuilder ConfigureServices(this IHostBuilder hostBuilder) =>
        hostBuilder
            .ConfigureServices((context, collection) =>
            {
                collection
                    .AddDatabases(context.Configuration)
                    .AddTransient<ICafeService, CafeService>()
                    .AddHttpClient(context.Configuration);
            });

    private static IHostBuilder ConfigureLogging(this IHostBuilder hostBuilder) =>
        hostBuilder
            .ConfigureLogging((_, builder) => { builder.ClearProviders(); })
            .UseSerilog((_, _, serilogConfig) =>
            {
                var localAppData = Environment
                    .GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                var appFolder = Path.Combine(localAppData, "SMS");
                if (!Directory.Exists(appFolder))
                {
                    Directory.CreateDirectory(appFolder);
                }
                
#if DEBUG
                serilogConfig.MinimumLevel.Debug();
#else
                serilogConfig.MinimumLevel.Information();
#endif
                
                serilogConfig
                    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                    .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Warning)

                    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)

                    .MinimumLevel.Override("System.Net.Http", LogEventLevel.Warning)

                    .WriteTo.Console()
                    .WriteTo.File(
                        new CompactJsonFormatter(),
                        Path.Combine(appFolder, "logs-sms-console-app-.json"),
                        restrictedToMinimumLevel: LogEventLevel.Information,
                        rollingInterval: RollingInterval.Day,
                        retainedFileCountLimit: 5);
            });

    private static IServiceCollection AddDatabases(this IServiceCollection services, IConfiguration config)
    {
        services.AddOptions<DatabaseSettings>()
            .BindConfiguration(nameof(DatabaseSettings))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        return services
            .AddDbContext<SmsDbContext>((sp, options) =>
            {
                var dbSettings = sp
                    .GetRequiredService<IOptions<DatabaseSettings>>()
                    .Value;

                options.UseNpgsql(dbSettings.SmsDb);
            })
            .AddTransient<ISmsRepository, SmsRepository>()
            .AddTransient<IStartupTask, SmsDbInitializer>();
    }

    private static IServiceCollection AddHttpClient(this IServiceCollection services, IConfiguration config)
    {
        services.AddOptions<HttpClientSettings>()
            .BindConfiguration(nameof(HttpClientSettings))
            .ValidateDataAnnotations()
            .ValidateOnStart();
        
        services
            .AddHttpClient<ICafeService, CafeService>((sp, client) =>
            {
                var httpClientSettings = sp
                    .GetRequiredService<IOptions<HttpClientSettings>>()
                    .Value;

                var token = Convert.ToBase64String(
                    Encoding.UTF8.GetBytes(
                        $"{httpClientSettings.Username}:{httpClientSettings.Password}"));

                client.BaseAddress = new Uri(httpClientSettings.BaseAddress);
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Basic", token);
            });

        return services;
    }
}