using System.Collections.Generic;

namespace akanevrc.TeuchiUdon.Editor.Compiler
{
    public class TeuchiUdonTypeStack
    {
        public static TeuchiUdonTypeStack Instance { get; } = new TeuchiUdonTypeStack();

        public Stack<TeuchiUdonType> Types { get; private set; }
        public Stack<bool> Transients { get; private set; }

        protected TeuchiUdonTypeStack()
        {
        }

        public void Init()
        {
            Types      = new Stack<TeuchiUdonType>();
            Transients = new Stack<bool>();
        }

        public void Push(TeuchiUdonType type, bool transient)
        {
            Types     .Push(type);
            Transients.Push(transient);
        }

        public TeuchiUdonType Pop()
        {
            Transients.Pop();
            return Types.Pop();
        }

        public TeuchiUdonType PopIfTransient()
        {
            var type = (TeuchiUdonType)null;
            while (Transients.Peek())
            {
                Transients.Pop();
                type = Types.Pop();
            }
            return type;
        }

        public TeuchiUdonType Peek()
        {
            return Types.Peek();
        }
    }
}
