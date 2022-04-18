using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using OmniSharp.Extensions.LanguageServer.Protocol;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using OmniSharp.Extensions.LanguageServer.Protocol.Server.Capabilities;

namespace akanevrc.TeuchiUdon.Server
{
    public class TextDocumentSyncHandler : TextDocumentSyncHandlerBase
    {
        private DocumentManager DocumentManager { get; }

        public TextDocumentSyncHandler(DocumentManager documentManager)
        {
            DocumentManager = documentManager;
        }

        public override TextDocumentAttributes GetTextDocumentAttributes(DocumentUri uri)
        {
            return new(uri, "teuchiudon");
        }

        protected override TextDocumentSyncRegistrationOptions CreateRegistrationOptions
        (
            SynchronizationCapability capability,
            ClientCapabilities clientCapabilities
        )
        {
            return new()
            {
                DocumentSelector = DocumentSelector.ForLanguage("teuchiudon"),
                Change           = TextDocumentSyncKind.Full,
                Save             = new SaveOptions() { IncludeText = true }
            };
        }

        public override Task<Unit> Handle(DidChangeTextDocumentParams request, CancellationToken cancellationToken)
        {
            var documentPath = request.TextDocument.Uri.ToString();
            var text         = request.ContentChanges.FirstOrDefault()?.Text ?? "";

            DocumentManager.Update(documentPath, text);

            return Unit.Task;
        }

        public override Task<Unit> Handle(DidOpenTextDocumentParams request, CancellationToken cancellationToken)
        {
            DocumentManager.Update(request.TextDocument.Uri.ToString(), request.TextDocument.Text);
            return Unit.Task;
        }

        public override Task<Unit> Handle(DidCloseTextDocumentParams request, CancellationToken cancellationToken)
        {
            return Unit.Task;
        }

        public override Task<Unit> Handle(DidSaveTextDocumentParams request, CancellationToken cancellationToken)
        {
            return Unit.Task;
        }
    }
}
