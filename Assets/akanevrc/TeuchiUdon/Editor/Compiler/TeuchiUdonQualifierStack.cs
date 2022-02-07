using System.Collections.Generic;

namespace akanevrc.TeuchiUdon.Editor.Compiler
{
    public class TeuchiUdonQualifierStack
    {
        public static TeuchiUdonQualifierStack Instance { get; } = new TeuchiUdonQualifierStack();

        public Stack<TeuchiUdonQualifier> Qualifiers { get; private set; }

        protected TeuchiUdonQualifierStack()
        {
        }

        public void Init()
        {
            Qualifiers = new Stack<TeuchiUdonQualifier>();
        }

        public void Push(TeuchiUdonQualifier qualifier)
        {
            Qualifiers.Push(qualifier);
        }

        public void PushScope(TeuchiUdonScope scope)
        {
            Qualifiers.Push(Qualifiers.Peek().Append(scope, scope));
        }

        public void PushNewScope(TeuchiUdonScope scope)
        {
            Qualifiers.Push(TeuchiUdonQualifier.Top.Append(scope, scope));
        }

        public TeuchiUdonQualifier Pop()
        {
            return Qualifiers.Pop();
        }

        public TeuchiUdonQualifier Peek()
        {
            return Qualifiers.Peek();
        }
    }
}