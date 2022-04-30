using System.Collections.Generic;

namespace akanevrc.TeuchiUdon
{
    public class TeuchiUdonQualifierStack
    {
        public Stack<TeuchiUdonQualifier> Qualifiers { get; private set; }

        public TeuchiUdonQualifierStack()
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
            if (Qualifiers.Count == 0) return TeuchiUdonQualifier.Top;
            return Qualifiers.Pop();
        }

        public TeuchiUdonQualifier Peek()
        {
            return Qualifiers.Peek();
        }
    }
}
