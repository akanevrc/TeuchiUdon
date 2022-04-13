using System.Collections.Generic;
using System.IO;
using Antlr4.Runtime;

namespace akanevrc.TeuchiUdon
{
    public abstract class TeuchiUdonBaseLexer : Lexer
    {
        public TeuchiUdonBaseLexer(ICharStream input)
            : base(input)
        {
        }

        public TeuchiUdonBaseLexer(ICharStream input, TextWriter output, TextWriter errorOutput)
            : base(input, output, errorOutput)
        {
        }

        protected int InterpolatedStringLevel { get; set; }
        protected Stack<int> CurlyLevels { get; } = new Stack<int>();

        protected void OnInterpolatedRegularStringStart()
        {
            InterpolatedStringLevel++;
        }

        protected void OnOpenBrace()
        {
            if (InterpolatedStringLevel > 0)
            {
                CurlyLevels.Push(CurlyLevels.Pop() + 1);
            }
        }

        protected void OnCloseBrace()
        {
            if (InterpolatedStringLevel > 0)
            {
                CurlyLevels.Push(CurlyLevels.Pop() - 1);
                if (CurlyLevels.Peek() == 0)
                {
                    CurlyLevels.Pop();
                    Skip();
                    PopMode();
                }
            }
        }

        protected void OpenBraceInside()
        {
            CurlyLevels.Push(1);
        }

        protected void OnDoubleQuoteInside()
        {
            InterpolatedStringLevel--;
        }
    }
}
