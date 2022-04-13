using System.IO;
using Antlr4.Runtime;

namespace akanevrc.TeuchiUdon
{
    public class TeuchiUdonBaseParser : Parser
    {
        public override string[] RuleNames => throw new System.NotImplementedException();
        public override IVocabulary Vocabulary => throw new System.NotImplementedException();
        public override string GrammarFileName => throw new System.NotImplementedException();

        public new TextWriter Output => base.Output;

        public TeuchiUdonBaseParser(ITokenStream input)
            : base(input)
        {
        }

        public TeuchiUdonBaseParser(ITokenStream input, TextWriter output, TextWriter errorOutput)
            : base(input, output, errorOutput)
        {
        }
    }
}
