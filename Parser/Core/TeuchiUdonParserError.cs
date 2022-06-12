using Antlr4.Runtime;

namespace akanevrc.TeuchiUdon
{
    public class TeuchiUdonParserError
    {
        public IToken Start { get; }
        public IToken Stop { get; }
        public string Message { get; }

        public TeuchiUdonParserError(IToken start, IToken stop, string message)
        {
            Start   = start;
            Stop    = stop;
            Message = message;
        }

        public override string ToString()
        {
            return $"({Start?.Line.ToString() ?? "?"}, {Start?.Column.ToString() ?? "?"}): {Message}";
        }
    }
}
