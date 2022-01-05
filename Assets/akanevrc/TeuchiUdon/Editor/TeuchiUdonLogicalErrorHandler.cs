using Antlr4.Runtime;

namespace akanevrc.TeuchiUdon.Editor
{
    public class TeuchiUdonLogicalErrorHandler
    {
        private Parser Parser { get; }

        public TeuchiUdonLogicalErrorHandler(Parser parser)
        {
            Parser = parser;
        }

        public void ReportError(IToken token, string message)
        {
            Parser.NotifyErrorListeners(token, message, null);
        }
    }
}
