using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OmniSharp.Extensions.LanguageServer.Server;

namespace akanevrc.TeuchiUdon.Server
{
    public class Program
    {
        private static async Task Main(string[] args)
        {
            var server = await LanguageServer.From(options =>
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
            services.AddSingleton<DocumentManager>();
            services.AddTransient<TextDocumentSyncHandler>();
            services.AddTransient<SemanticTokensHandler>();
            services.AddTransient<CompletionHandler>();
        }
    }
}
