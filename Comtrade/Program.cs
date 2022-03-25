using Comtrade.Handlers;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Hosting;
using System.CommandLine.NamingConventionBinder;
using System.CommandLine.Parsing;
using System.Threading;
using System.Threading.Tasks;

namespace Comtrade
{
    public static class Program
    {
        public static async Task<int> Main(string[] args)
            => await BuildCommandLine()
                .UseHost(CreateHostBuilder)
                .UseDefaults()
                .Build()
                .InvokeAsync(args);

        private static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .ConfigureServices(services =>
                {
                    services.AddOptions<FileDistributedCacheOptions>();
                    services.AddSingleton<IDistributedCache, FileDistributedCache>();

                    services.AddHttpClient<ComtradeClient>(client =>
                    {
                        client.BaseAddress = new Uri("https://comtrade.un.org/");
                    });

                    services.AddTransient<TopExportsHandler>();
                    services.AddTransient<TopExportersHandler>();
                    services.AddTransient<TopDominatedHandler>();
                });
        }

        private static CommandLineBuilder BuildCommandLine()
            => new(new RootCommand()
            {
                BuildSelect("reporters", (client, cancellationToken) => client.Reporters(cancellationToken)),
                BuildSelect("partners", (client, cancellationToken) => client.Partners(cancellationToken)),
                BuildSelect("flows", (client, cancellationToken) => client.Flows(cancellationToken)),
                BuildCommodities("commodities"),
                BuildTopExports("top-exports"),
                BuildTopExporters("top-exporters"),
                BuildTopDominated("top-dominated"),
            });

        private static Command BuildSelect(string name, Func<ComtradeClient, CancellationToken, Task<ParameterResponse>> action)
        {
            var command = new Command(name)
            {
            };

            command.Handler = CommandHandler.Create(
                async (IHost host, CancellationToken cancellationToken)
                    =>
                    {
                        var client = host.Services.GetRequiredService<ComtradeClient>();
                        var select = await action(client, cancellationToken);

                        foreach (var item in select.Results)
                            Console.WriteLine(item);
                    });

            return command;
        }

        private static Command BuildCommodities(string name)
        {
            var command = new Command(name)
            {
                new Option<string>(new[] {"-x", "--classification" }, () => "HS"),
            };

            command.Handler = CommandHandler.Create(
                async (IHost host, string classification, CancellationToken cancellationToken)
                    =>
                {
                    var client = host.Services.GetRequiredService<ComtradeClient>();
                    var select = await client.Commodities(classification, cancellationToken);

                    foreach (var item in select.Results)
                        Console.WriteLine(item);
                });

            return command;
        }

        private static Command BuildTopExports(string name)
        {
            var command = new Command(name)
            {
                new Option<string>(new[] {"-x", "--classification" }, () => "HS"),
                new Option<int>(new[] {"-r", "--reporter" }),
            };

            command.Handler = CommandHandler.Create(
                (IHost host, string x, int r, CancellationToken cancellationToken)
                    => host.Services.GetRequiredService<TopExportsHandler>().Run(x, r, cancellationToken));

            return command;
        }

        private static Command BuildTopExporters(string name)
        {
            var command = new Command(name)
            {
                new Option<string>(new[] {"-x", "--classification" }, () => "HS"),
                new Option<string>(new[] {"-c", "--commodity" }) { IsRequired = true },
                new Option<double>(new[] {"-m", "--min-share" }, () => 0.1),
            };

            command.Handler = CommandHandler.Create(
                (IHost host, string x, string c, double m, CancellationToken cancellationToken)
                    => host.Services.GetRequiredService<TopExportersHandler>().Run(x, c, m, cancellationToken));

            return command;
        }

        private static Command BuildTopDominated(string name)
        {
            var command = new Command(name)
            {
                new Option<string>(new[] {"-x", "--classification" }, () => "HS"),
                new Option<double>(new[] {"-m", "--min-share" }, () => 0.1),
            };

            command.Handler = CommandHandler.Create(
                (IHost host, string x, double m, CancellationToken cancellationToken)
                    => host.Services.GetRequiredService<TopDominatedHandler>().Run(x, m, cancellationToken));

            return command;
        }
    }
}