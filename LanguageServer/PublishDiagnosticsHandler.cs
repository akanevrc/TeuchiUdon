using System.Threading;
using System.Threading.Tasks;
using MediatR;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using OmniSharp.Extensions.LanguageServer.Protocol.Server;

namespace akanevrc.TeuchiUdon.Server
{
    public class PublishDiagnosticsHandler : PublishDiagnosticsHandlerBase
    {
        private ILanguageServerFacade LanguageServerFacade { get; }

        public PublishDiagnosticsHandler(ILanguageServerFacade languageServerFacade)
        {
            LanguageServerFacade = languageServerFacade;
        }

        public override async Task<Unit> Handle(PublishDiagnosticsParams request, CancellationToken cancellationToken)
        {
            await Task.Yield();

            LanguageServerFacade.TextDocument.PublishDiagnostics(request);

            return Unit.Value;
        }
    }
}
