using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace akanevrc.TeuchiUdon.Server
{
    public class CompletionHandler : ICompletionHandler
    {
        private DocumentManager DocumentManager { get; }

        public CompletionHandler(DocumentManager documentManager)
        {
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

        public Task<CompletionList> Handle(CompletionParams request, CancellationToken cancellationToken)
        {
            var documentPath = request.TextDocument.Uri.ToString();
            var text         = DocumentManager.Get(documentPath);

            var lineHead = GetPosition(text, request.Position.Line, request.Position.Character);
            var wordHead = GetWordHead(text, lineHead);

            return Task.FromResult(new CompletionList
            (
                new CompletionItem[]
                {
                    new CompletionItem()
                    {
                        Label    = "helloTeuchiUdonLSP",
                        Kind     = CompletionItemKind.Text,
                        TextEdit = new TextEdit()
                        {
                            NewText = "helloTeuchiUdonLSP",
                            Range   = new
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
                            )
                        }
                    }
                }
            ));
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
