using System.IO;
using Antlr4.Runtime;

namespace akanevrc.TeuchiUdon.Editor.Compiler
{
    public abstract class TeuchiUdonBaseLexer : Lexer
    {
        protected int commentLevel = 0;

        public TeuchiUdonBaseLexer(ICharStream input)
            : base(input)
        {
        }

        public TeuchiUdonBaseLexer(ICharStream input, TextWriter output, TextWriter errorOutput)
            : base(input, output, errorOutput)
        {
        }
    }
}
