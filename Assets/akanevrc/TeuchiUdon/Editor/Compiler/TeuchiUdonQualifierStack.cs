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

        public void PushName(string name)
        {
            Qualifiers.Push(Qualifiers.Peek().Append(name, name));
        }

        public void PushNewName(string name)
        {
            Qualifiers.Push(TeuchiUdonQualifier.Top.Append(name, name));
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
