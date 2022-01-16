using Antlr4.Runtime;

namespace akanevrc.TeuchiUdon.Editor.Compiler
{
    public class TeuchiUdonLogicalErrorHandler
    {
        public static TeuchiUdonLogicalErrorHandler Instance { get; } = new TeuchiUdonLogicalErrorHandler();

        private Parser Parser { get; set; }

        public void Init(Parser parser)
        {
            Parser = parser;
        }

        public void ReportError(IToken token, string message)
        {
            Parser.NotifyErrorListeners(token, message, null);
        }
    }
}
