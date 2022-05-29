using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OmniSharp.Extensions.LanguageServer.Server;

namespace akanevrc.TeuchiUdon.Server
{
    public static class Program
    {
        private static async Task Main(string[] args)
        {
            using var server =
                await LanguageServer.From(options =>
                    options
                    .WithInput(Console.OpenStandardInput())
                    .WithOutput(Console.OpenStandardOutput())
                    .WithLoggerFactory(new LoggerFactory())
                    .AddDefaultLoggingProvider()
                    .WithServices(x => x.AddLogging(b => b.SetMinimumLevel(LogLevel.Trace)))
                    .WithServices(ConfigureServices)
                    .WithHandler<TextDocumentSyncHandler>()
                    .WithHandler<SemanticTokensHandler>()
                    .WithHandler<CompletionHandler>()
                )
                .ConfigureAwait(false);

            await server.WaitForExit.ConfigureAwait(false);
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            services
            .AddParserServices()
            .AddSingleton<DocumentManager>()
            .AddSingleton<TextDocumentSyncHandler>()
            .AddSingleton<SemanticTokensHandler>(services => new SemanticTokensHandler(services, services.GetService<DocumentManager>()!))
            .AddSingleton<CompletionHandler>    (services => new CompletionHandler    (services, services.GetService<DocumentManager>()!));
        }
    }
}
