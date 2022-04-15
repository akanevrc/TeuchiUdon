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
    internal class TextDocumentSyncHandler : ITextDocumentSyncHandler
    {
        private DocumentManager DocumentManager { get; }

        private DocumentSelector DocumentSelector =
            new
            (
                new DocumentFilter()
                {
                    Language = "teuchiudon"
                }
            );

        private SynchronizationCapability? Capability;

        public TextDocumentSyncHandler(DocumentManager documentManager)
        {
            DocumentManager = documentManager;
        }

        public TextDocumentSyncKind Change { get; } = TextDocumentSyncKind.Full;

        public TextDocumentAttributes GetTextDocumentAttributes(DocumentUri uri)
        {
            return new(uri, "teuchiudon");
        }

        public Task<Unit> Handle(DidOpenTextDocumentParams request, CancellationToken cancellationToken)
        {
            DocumentManager.Update(request.TextDocument.Uri.ToString(), request.TextDocument.Text);
            return Unit.Task;
        }

        public Task<Unit> Handle(DidCloseTextDocumentParams request, CancellationToken cancellationToken)
        {
            return Unit.Task;
        }

        public Task<Unit> Handle(DidChangeTextDocumentParams request, CancellationToken cancellationToken)
        {
            var documentPath = request.TextDocument.Uri.ToString();
            var text         = request.ContentChanges.FirstOrDefault()?.Text ?? "";

            DocumentManager.Update(documentPath, text);

            return Unit.Task;
        }

        public Task<Unit> Handle(DidSaveTextDocumentParams request, CancellationToken cancellationToken)
        {
            return Unit.Task;
        }

        public void SetCapability(SynchronizationCapability capability)
        {
            Capability = capability;
        }

        TextDocumentOpenRegistrationOptions IRegistration<TextDocumentOpenRegistrationOptions, SynchronizationCapability>.GetRegistrationOptions
        (
            SynchronizationCapability synchronizationCapability,
            ClientCapabilities        clientCapabilities
        )
        {
            return new()
            {
                DocumentSelector = DocumentSelector,
            };
        }

        TextDocumentCloseRegistrationOptions IRegistration<TextDocumentCloseRegistrationOptions, SynchronizationCapability>.GetRegistrationOptions
        (
            SynchronizationCapability synchronizationCapability,
            ClientCapabilities        clientCapabilities
        )
        {
            return new()
            {
                DocumentSelector = DocumentSelector,
            };
        }

        TextDocumentChangeRegistrationOptions IRegistration<TextDocumentChangeRegistrationOptions, SynchronizationCapability>.GetRegistrationOptions
        (
            SynchronizationCapability synchronizationCapability,
            ClientCapabilities        clientCapabilities
        )
        {
            return new()
            {
                DocumentSelector = DocumentSelector,
                SyncKind         = Change
            };
        }

        TextDocumentSaveRegistrationOptions IRegistration<TextDocumentSaveRegistrationOptions, SynchronizationCapability>.GetRegistrationOptions
        (
            SynchronizationCapability synchronizationCapability,
            ClientCapabilities        clientCapabilities
        )
        {
            return new()
            {
                DocumentSelector = DocumentSelector,
                IncludeText      = true
            };
        }
    }
}
