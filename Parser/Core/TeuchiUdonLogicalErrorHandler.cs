using Antlr4.Runtime;

namespace akanevrc.TeuchiUdon
{
    public class TeuchiUdonLogicalErrorHandler
    {
        private Parser Parser { get; set; }

        public void SetParser(Parser parser)
        {
            Parser = parser;
        }

        public void ReportError(IToken token, string message)
        {
            Parser.NotifyErrorListeners(token, message, null);
        }
    }
}
