using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace akanevrc.TeuchiUdon.Server
{
    public class SemanticTokensHandler : SemanticTokensHandlerBase
    {
        private DocumentManager DocumentManager { get; }

        public SemanticTokensHandler(DocumentManager documentManager)
        {
            DocumentManager = documentManager;
        }

        protected override SemanticTokensRegistrationOptions CreateRegistrationOptions
        (
            SemanticTokensCapability capability,
            ClientCapabilities clientCapabilities
        )
        {
            return new()
            {
                DocumentSelector = DocumentSelector.ForLanguage("teuchiudon"),
                Legend           = new SemanticTokensLegend()
                {
                    TokenModifiers = capability.TokenModifiers,
                    TokenTypes     = capability.TokenTypes
                },
                Full = new SemanticTokensCapabilityRequestFull
                {
                    Delta = true
                },
                Range = true
            };
        }

        protected override async Task Tokenize(SemanticTokensBuilder builder, ITextDocumentIdentifierParams request, CancellationToken cancellationToken)
        {
            using var typesEnumerator     = RotateIter(SemanticTokenType    .Defaults).GetEnumerator();
            using var modifiersEnumerator = RotateIter(SemanticTokenModifier.Defaults).GetEnumerator();

            var documentPath = request.TextDocument.Uri.ToString();
            var text         = DocumentManager.Get(documentPath);

            await Task.Yield();
            foreach (var (t, l) in text.Split('\n').Select((t, l) => (t, l)))
            {
                var parts = t.TrimEnd().Split(';', ' ', '.', '"', '(', ')');
                var index = 0;
                foreach (var part in parts)
                {
                    typesEnumerator    .MoveNext();
                    modifiersEnumerator.MoveNext();
                    if (string.IsNullOrWhiteSpace(part)) continue;
                    index = t.IndexOf(part, index, StringComparison.Ordinal);
                    builder.Push(l, index, part.Length, typesEnumerator.Current, modifiersEnumerator.Current);
                }
            }
        }

        private IEnumerable<T> RotateIter<T>(IEnumerable<T> values)
        {
            for (;;) foreach (var item in values) yield return item;
        }

        protected override Task<SemanticTokensDocument> GetSemanticTokensDocument(ITextDocumentIdentifierParams request, CancellationToken cancellationToken)
        {
            return Task.FromResult(new SemanticTokensDocument(RegistrationOptions.Legend));
        }
    }
}
