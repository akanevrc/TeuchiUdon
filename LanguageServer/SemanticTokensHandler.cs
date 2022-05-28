using System;
using System.Threading;
using System.Threading.Tasks;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;

using Microsoft.Extensions.Logging;

namespace akanevrc.TeuchiUdon.Server
{
    public class SemanticTokensHandler : SemanticTokensHandlerBase
    {
        private IServiceProvider Services { get; }
        private DocumentManager DocumentManager { get; }

        public SemanticTokensHandler(IServiceProvider services, DocumentManager documentManager)
        {
            Services        = services;
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

        protected override Task<SemanticTokensDocument> GetSemanticTokensDocument(ITextDocumentIdentifierParams request, CancellationToken cancellationToken)
        {
            return Task.FromResult(new SemanticTokensDocument(RegistrationOptions.Legend));
        }

        protected override async Task Tokenize(SemanticTokensBuilder builder, ITextDocumentIdentifierParams request, CancellationToken cancellationToken)
        {
            var documentPath = request.TextDocument.Uri.ToString();
            var text         = DocumentManager.Get(documentPath);

            await Task.Yield();

            var (result, error) = TeuchiUdonParserRunner.ParseFromString(Services, text);

            if (result != null)
            {
                new ResultVisitor(builder).VisitTarget(result);
            }
        }

        private class ResultVisitor
        {
            private SemanticTokensBuilder Builder { get; }

            public ResultVisitor(SemanticTokensBuilder builder)
            {
                Builder = builder;
            }

            public void VisitTarget(TargetResult result)
            {
                VisitBody(result.Body);
            }

            private void VisitBody(BodyResult result)
            {
                foreach (var topStatement in result.TopStatements) VisitTopStatement(topStatement);
            }

            private void VisitTopStatement(TopStatementResult result)
            {
                switch (result)
                {
                    case TopBindResult topBind:
                        VisitTopBind(topBind);
                        return;
                    default:
                        return;
                }
            }

            private void VisitTopBind(TopBindResult result)
            {
                VisitVarBind(result.VarBind);
            }

            private void VisitVarBind(VarBindResult result)
            {
                VisitVarDecl(result.VarDecl);
                VisitExpr(result.Expr);
            }

            private void VisitVarDecl(VarDeclResult result)
            {
                foreach (var qualifiedVar in result.QualifiedVars) VisitQualifiedVar(qualifiedVar);
            }

            private void VisitQualifiedVar(QualifiedVarResult result)
            {
                VisitIdentifier(result.Identifier);
            }

            private void VisitIdentifier(IdentifierResult result)
            {
                PushToken(result, SemanticTokenType.Variable, SemanticTokenModifier.Definition);
            }

            private void VisitExpr(ExprResult result)
            {
                VisitTyped(result.Inner);
            }

            private void VisitTyped(TypedResult result)
            {
                switch (result)
                {
                    case LiteralResult literal:
                        VisitLiteral(literal);
                        return;
                    default:
                        return;
                }
            }

            private void VisitLiteral(LiteralResult result)
            {
                switch (result.Type.LogicalName)
                {
                    case "SystemInt32":
                        PushToken(result, SemanticTokenType.Number, SemanticTokenModifier.Definition);
                        return;
                    case "SystemString":
                        PushToken(result, SemanticTokenType.String, SemanticTokenModifier.Definition);
                        return;
                    default:
                        return;
                }
            }

            private void PushToken(TeuchiUdonParserResult result, SemanticTokenType type, SemanticTokenModifier modifier)
            {
                var range =
                    new OmniSharp.Extensions.LanguageServer.Protocol.Models.Range
                    (
                        result.Start.Line - 1,
                        result.Start.Column,
                        result.Stop .Line - 1,
                        result.Stop .Column + result.Stop.Text.Length
                    );
                Builder.Push(range, type, modifier);
            }
        }
    }
}
