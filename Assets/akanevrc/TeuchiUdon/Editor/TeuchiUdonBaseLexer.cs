using System.Collections.Generic;
using System.IO;
using Antlr4.Runtime;

namespace akanevrc.TeuchiUdon.Editor
{
    public abstract class TeuchiUdonBaseLexer : Lexer
    {
        protected const int TAB_LENGTH = 4;

        protected bool pendingDent = true;
        protected int indentCount = 0;
        protected Queue<IToken> tokenQueue = new Queue<IToken>();
        protected Stack<(int keyword, int length)> indentStack = new Stack<(int, int)>();
        protected IToken initialIndentToken = null;
        protected int lastKeyWord = 0;

        protected bool prevWasEndl = false;
        protected bool prevWasKeyWord = false;
        protected bool ignoreIndent = false;
        protected bool moduleStartIndent = false;
        protected bool wasModuleExport = false;

        protected int startIndent = -1;
        protected int nestedLevel = 0;

        protected int commentLevel = 0;

        public TeuchiUdonBaseLexer(ICharStream input)
            : base(input)
        {
        }

        public TeuchiUdonBaseLexer(ICharStream input, TextWriter output, TextWriter errorOutput)
            : base(input, output, errorOutput)
        {
        }

        protected CommonToken CreateToken(int type, string text, IToken next)
        {
            var token = new CommonToken(type, text);
            if (initialIndentToken != null)
            {
                token.StartIndex = initialIndentToken.StartIndex;
                token.Line       = initialIndentToken.Line;
                token.Column     = initialIndentToken.Column;
                token.StopIndex  = next.StartIndex - 1;
            }
            return token;
        }

        protected void ProcessNewlineToken()
        {
            if (pendingDent) this.Channel = TokenConstants.HiddenChannel;
            indentCount = 0;
            initialIndentToken = null;
        }

        protected void ProcessWsToken()
        {
            this.Channel = TokenConstants.HiddenChannel;
            if (pendingDent) indentCount += this.Text.Length;
        }

        protected void ProcessTabToken()
        {
            this.Channel = TokenConstants.HiddenChannel;
            if (pendingDent) indentCount += this.Text.Length * TAB_LENGTH;
        }

        protected int GetSavedIndent()
        {
            return indentStack.Count == 0 ? startIndent : indentStack.Peek().length;
        }

        public override IToken NextToken()
        {
            if (tokenQueue.Count != 0) return tokenQueue.Dequeue();

            var next = base.NextToken();
            var type = next.Type;
            
            if
            (
                startIndent == -1 &&
                type != TeuchiUdonLexer.NEWLINE &&
                type != TeuchiUdonLexer.WS      &&
                type != TeuchiUdonLexer.TAB
            )
            {
                if (type == TeuchiUdonLexer.MODULE)
                {
                    moduleStartIndent = true;
                    wasModuleExport   = true;
                }
                else if (!moduleStartIndent)
                {
                    startIndent = next.Column;
                }
                else if (lastKeyWord == TeuchiUdonLexer.WHERE && moduleStartIndent)
                {
                    lastKeyWord       = 0;
                    prevWasEndl       = false;
                    prevWasKeyWord    = false;
                    moduleStartIndent = false;
                    startIndent       = next.Column;
                    nestedLevel       = 0;
                    tokenQueue.Enqueue(CreateToken(TeuchiUdonLexer.V_OPEN, "#{", next));
                    tokenQueue.Enqueue(CreateToken(type, next.Text, next));
                    return tokenQueue.Dequeue();
                }
            }
            
            if (type == TeuchiUdonLexer.OPEN_BRACE)
            {
                if (moduleStartIndent)
                {
                    moduleStartIndent = false;
                    wasModuleExport   = false;
                }

                prevWasEndl  = false;
                ignoreIndent = true;
            }

            if
            (
                prevWasKeyWord     &&
                !prevWasEndl       &&
                !moduleStartIndent &&
                type != TeuchiUdonLexer.NEWLINE &&
                type != TeuchiUdonLexer.WS      &&
                type != TeuchiUdonLexer.TAB
            )
            {
                prevWasKeyWord = false;
                indentStack.Push((lastKeyWord, next.Column));
                if (lastKeyWord != TeuchiUdonLexer.OPEN_BRACE)
                {
                    tokenQueue.Enqueue(CreateToken(TeuchiUdonLexer.V_OPEN, "#{", next));
                }
            }

            if
            (
                ignoreIndent &&
                (
                    type == TeuchiUdonLexer.WHERE ||
                    type == TeuchiUdonLexer.LET   ||
                    type == TeuchiUdonLexer.OF    ||
                    type == TeuchiUdonLexer.CLOSE_BRACE
                )
            )
            {
                ignoreIndent = false;
            }

            if
            (
                pendingDent    &&
                prevWasKeyWord &&
                !ignoreIndent  &&
                indentCount <= GetSavedIndent() &&
                type != TeuchiUdonLexer.NEWLINE &&
                type != TeuchiUdonLexer.WS
            )
            {
                if (lastKeyWord != TeuchiUdonLexer.OPEN_BRACE)
                {
                    tokenQueue.Enqueue(CreateToken(TeuchiUdonLexer.V_OPEN, "#{", next));
                }
                prevWasEndl    = true;
                prevWasKeyWord = false;
            }

            if
            (
                pendingDent   &&
                prevWasEndl   &&
                !ignoreIndent &&
                indentCount <= GetSavedIndent()     &&
                type != TeuchiUdonLexer.NEWLINE     &&
                type != TeuchiUdonLexer.WS          &&
                type != TeuchiUdonLexer.WHERE       &&
                type != TeuchiUdonLexer.IN          &&
                type != TeuchiUdonLexer.OF          &&
                type != TeuchiUdonLexer.CLOSE_BRACE &&
                type != TokenConstants.EOF
            )
            {
                while (nestedLevel > indentStack.Count)
                {
                    tokenQueue.Enqueue(CreateToken(TeuchiUdonLexer.V_END  , "#;", next));
                    tokenQueue.Enqueue(CreateToken(TeuchiUdonLexer.V_CLOSE, "#}", next));
                    nestedLevel--;
                }

                while (indentCount < GetSavedIndent())
                {
                    tokenQueue.Enqueue(CreateToken(TeuchiUdonLexer.V_END  , "#;", next));
                    tokenQueue.Enqueue(CreateToken(TeuchiUdonLexer.V_CLOSE, "#}", next));
                    indentStack.Pop();
                    nestedLevel--;
                }

                if (indentCount == GetSavedIndent())
                {
                    tokenQueue.Enqueue(CreateToken(TeuchiUdonLexer.V_END, "#;", next));
                }

                if (indentCount == startIndent)
                {
                    pendingDent = false;
                }

                prevWasEndl = false;
            }

            if
            (
                pendingDent        &&
                prevWasKeyWord     &&
                !moduleStartIndent &&
                !ignoreIndent      &&
                indentCount > GetSavedIndent()  &&
                type != TeuchiUdonLexer.NEWLINE &&
                type != TeuchiUdonLexer.WS      &&
                type != TokenConstants.EOF
            )
            {
                prevWasKeyWord = false;

                if (prevWasEndl)
                {
                    indentStack.Push((lastKeyWord, indentCount));
                    prevWasEndl = false;
                }

                if (lastKeyWord != TeuchiUdonLexer.OPEN_BRACE)
                {
                    tokenQueue.Enqueue(CreateToken(TeuchiUdonLexer.V_OPEN, "#{", next));
                }
            }

            if
            (
                pendingDent &&
                initialIndentToken == null &&
                type != TeuchiUdonLexer.NEWLINE
            )
            {
                initialIndentToken = next;
            }

            if (next != null && type == TeuchiUdonLexer.NEWLINE)
            {
                prevWasEndl = true;
            }

            if
            (   type == TeuchiUdonLexer.WHERE ||
                type == TeuchiUdonLexer.LET   ||
                type == TeuchiUdonLexer.OF    ||
                type == TeuchiUdonLexer.OPEN_BRACE
            )
            {
                lastKeyWord    = type;
                prevWasEndl    = false;
                prevWasKeyWord = true;
                nestedLevel++;
            }

            if (next == null || next.Channel == TokenConstants.HiddenChannel || type == TeuchiUdonLexer.NEWLINE)
            {
                return next;
            }

            if (type == TeuchiUdonLexer.CLOSE_BRACE)
            {
                while (indentStack.Count != 0 && indentStack.Peek().keyword != TeuchiUdonLexer.OPEN_BRACE)
                {
                    tokenQueue.Enqueue(CreateToken(TeuchiUdonLexer.V_END  , "#;", next));
                    tokenQueue.Enqueue(CreateToken(TeuchiUdonLexer.V_CLOSE, "#}", next));
                    indentStack.Pop();
                    nestedLevel--;
                }

                if (indentStack.Count != 0 && indentStack.Peek().keyword == TeuchiUdonLexer.OPEN_BRACE)
                {
                    tokenQueue.Enqueue(CreateToken(TeuchiUdonLexer.V_END, "#;", next));
                    indentStack.Pop();
                    nestedLevel--;
                }
            }

            if (type == TeuchiUdonLexer.IN)
            {
                while (indentStack.Count != 0 && indentStack.Peek().keyword != TeuchiUdonLexer.LET)
                {
                    tokenQueue.Enqueue(CreateToken(TeuchiUdonLexer.V_END  , "#;", next));
                    tokenQueue.Enqueue(CreateToken(TeuchiUdonLexer.V_CLOSE, "#}", next));
                    indentStack.Pop();
                    nestedLevel--;
                }

                if (indentStack.Count != 0 && indentStack.Peek().keyword == TeuchiUdonLexer.LET)
                {
                    tokenQueue.Enqueue(CreateToken(TeuchiUdonLexer.V_END  , "#;", next));
                    tokenQueue.Enqueue(CreateToken(TeuchiUdonLexer.V_CLOSE, "#}", next));
                    indentStack.Pop();
                    nestedLevel--;
                }
            }

            if (type == TokenConstants.EOF)
            {
                indentCount = startIndent;

                if (!pendingDent) initialIndentToken = next;

                while (nestedLevel > indentStack.Count)
                {
                    tokenQueue.Enqueue(CreateToken(TeuchiUdonLexer.V_END  , "#;", next));
                    tokenQueue.Enqueue(CreateToken(TeuchiUdonLexer.V_CLOSE, "#}", next));
                    nestedLevel--;
                }

                while (indentCount < GetSavedIndent())
                {
                    tokenQueue.Enqueue(CreateToken(TeuchiUdonLexer.V_END  , "#;", next));
                    tokenQueue.Enqueue(CreateToken(TeuchiUdonLexer.V_CLOSE, "#}", next));
                    indentStack.Pop();
                    nestedLevel--;
                }

                if (indentCount == GetSavedIndent())
                {
                    tokenQueue.Enqueue(CreateToken(TeuchiUdonLexer.V_END, "#;", next));
                }

                if (wasModuleExport)
                {
                    tokenQueue.Enqueue(CreateToken(TeuchiUdonLexer.V_CLOSE, "#}", next));
                }

                startIndent = -1;
            }
            
            pendingDent = true;
            tokenQueue.Enqueue(next);
            return tokenQueue.Dequeue();
        }
    }
}
