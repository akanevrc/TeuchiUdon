using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.DependencyInjection;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;

namespace akanevrc.TeuchiUdon.Server
{
    public static class TeuchiUdonParserRunner
    {
        public static IServiceCollection AddParserServices(this IServiceCollection services, IEnumerable<string> dllPaths)
        {
            var dllLoader    = new TeuchiUdonDllLoader(dllPaths);
            var primitives   = new TeuchiUdonPrimitives(dllLoader);
            var staticTables = new TeuchiUdonStaticTables(dllLoader, primitives);

            return
                services
                .AddSingleton(dllLoader)
                .AddSingleton(primitives)
                .AddSingleton(staticTables)
                .AddSingleton<TeuchiUdonInvalids>()
                .AddScoped<TeuchiUdonLogicalErrorHandler>()
                .AddScoped<TeuchiUdonTables>()
                .AddScoped<TeuchiUdonTypeOps>()
                .AddScoped<TeuchiUdonLabelOps>()
                .AddScoped<TeuchiUdonTableOps>()
                .AddScoped<TeuchiUdonQualifierStack>()
                .AddScoped<TeuchiUdonOutValuePool>()
                .AddScoped<TeuchiUdonSyntaxOps>()
                .AddScoped<TeuchiUdonParserResultOps>()
                .AddScoped<TeuchiUdonListener>();
        }

        public static void Init(IServiceProvider provider)
        {
            provider.GetService<TeuchiUdonDllLoader   >()!.Init();
            provider.GetService<TeuchiUdonPrimitives  >()!.Init();
            provider.GetService<TeuchiUdonStaticTables>()!.Init();
            provider.GetService<TeuchiUdonInvalids    >()!.Init();
        }

        public static (TargetResult? result, string error) ParseFromString(IServiceProvider provider, string input)
        {
            using var inputReader = new StringReader(input);
            return ParseFromReader(provider, inputReader);
        }

        public static (TargetResult? result, string error) ParseFromReader(IServiceProvider provider, TextReader inputReader)
        {
            using var outputWriter = new StringWriter();
            using var errorWriter  = new StringWriter();
            
            var inputStream = CharStreams.fromTextReader(inputReader);
            var lexer       = new TeuchiUdonLexer(inputStream, outputWriter, errorWriter);
            var tokenStream = new CommonTokenStream(lexer);
            var parser      = new TeuchiUdonParser(tokenStream, outputWriter, errorWriter);

            provider.GetService<TeuchiUdonLogicalErrorHandler>()!.SetParser(parser);
            var listener = provider.GetService<TeuchiUdonListener>()!;

            Init(provider);

            try
            {
                ParseTreeWalker.Default.Walk(listener, parser.target());
            }
            catch (Exception ex)
            {
                return (null, $"{ex}\n{errorWriter}");
            }

            return (listener.TargetResult, errorWriter.ToString());
        }
    }
}
