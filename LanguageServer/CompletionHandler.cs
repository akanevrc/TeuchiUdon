using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace akanevrc.TeuchiUdon.Server
{
    public class CompletionHandler : ICompletionHandler
    {
        private IServiceProvider Services { get; }
        private DocumentManager DocumentManager { get; }

        public CompletionHandler(IServiceProvider services, DocumentManager documentManager)
        {
            Services        = services;
            DocumentManager = documentManager;
        }

        public CompletionRegistrationOptions GetRegistrationOptions
        (
            CompletionCapability capability,
            ClientCapabilities clientCapabilities
        )
        {
            return new()
            {
                DocumentSelector = DocumentSelector.ForLanguage("teuchiudon"),
                ResolveProvider  = false
            };
        }

        public async Task<CompletionList> Handle(CompletionParams request, CancellationToken cancellationToken)
        {
            var documentPath = request.TextDocument.Uri.ToString();
            var text         = DocumentManager.Get(documentPath);

            var position = GetPosition(text, request.Position.Line, request.Position.Character);
            var wordHead = GetWordHead(text, position);
            var word     = text.Substring(position - wordHead, wordHead);
            var range    =
                new OmniSharp.Extensions.LanguageServer.Protocol.Models.Range
                (
                    new Position
                    {
                        Line      = request.Position.Line,
                        Character = request.Position.Character - wordHead
                    },
                    new Position
                    {
                        Line      = request.Position.Line,
                        Character = request.Position.Character
                    }
                );

            CompletionItem completionItem(string name, CompletionItemKind kind) =>
                new CompletionItem()
                {
                    Label    = name,
                    Kind     = kind,
                    TextEdit = new TextEdit()
                    {
                        NewText = name,
                        Range   = range
                    }
                };

            await Task.Yield();

            using var scope = Services.CreateScope();
            var provider    = scope.ServiceProvider;

            var staticTables = provider.GetService<TeuchiUdonStaticTables>()!;
            var labelOps     = provider.GetService<TeuchiUdonLabelOps    >()!;

            return new CompletionList
            (
                        staticTables.Qualifiers.Values.Select(x => completionItem(labelOps.GetDescription(x), CompletionItemKind.Module))
                .Concat(staticTables.Types     .Values.Select(x => completionItem(labelOps.GetDescription(x), CompletionItemKind.Class)))
                .Concat(staticTables.Methods   .Values.Select(x => completionItem(labelOps.GetDescription(x), CompletionItemKind.Method)))
            );
        }

        private int GetPosition(string text, int line, int col)
        {
            var position = 0;
            for (var i = 0; i < line; i++)
            {
                position = text.IndexOf('\n', position) + 1;
            }
            return position + col;
        }

        private int GetWordHead(string text, int position)
        {
            return text.Take(position).Reverse().TakeWhile(x => !char.IsWhiteSpace(x)).Count();
        }
    }
}
