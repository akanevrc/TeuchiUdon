using Antlr4.Runtime;

namespace akanevrc.TeuchiUdon
{
    public class TeuchiUdonParserErrorOps
    {
        private TeuchiUdonTables Tables { get; }

        public TeuchiUdonParserErrorOps(TeuchiUdonTables tables)
        {
            Tables = tables;
        }

        public void AppendError(IToken start, IToken stop, string message)
        {
            Tables.ParserErrors.Add(new TeuchiUdonParserError(start, stop, message));
        }
    }
}
