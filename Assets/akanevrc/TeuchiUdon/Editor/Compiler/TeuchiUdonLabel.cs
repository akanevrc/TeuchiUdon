using System;

namespace akanevrc.TeuchiUdon.Editor.Compiler
{
    public interface ITeuchiUdonLabel
    {
        string GetLabel();
        string GetFullLabel();
    }

    public interface IIndexedLabel : ITeuchiUdonLabel
    {
        int Index { get; }
    }

    public class InvalidLabel : IIndexedLabel
    {
        public static InvalidLabel Instance = new InvalidLabel();
        public int Index { get; } = -1;
        public string GetLabel() => "[invalid]";
        public string GetFullLabel() => "[invalid]";
        protected InvalidLabel()
        {
        }
    }

    public class TextLabel : ITeuchiUdonLabel, IEquatable<TextLabel>
    {
        public TeuchiUdonQualifier Qualifier { get; }
        public string Text { get; }

        public TextLabel(string text)
            : this(TeuchiUdonQualifier.Top, text)
        {
        }

        public TextLabel(TeuchiUdonQualifier qualifier, string text)
        {
            Qualifier = qualifier;
            Text      = text;
        }

        public bool Equals(TextLabel obj)
        {
            return !object.ReferenceEquals(obj, null) && Qualifier == obj.Qualifier && Text == obj.Text;
        }

        public override bool Equals(object obj)
        {
            return obj is TextLabel label ? Equals(label) : base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return Text.GetHashCode();
        }

        public static bool operator ==(TextLabel obj1, TextLabel obj2)
        {
            return
                 object.ReferenceEquals(obj1, null) &&
                 object.ReferenceEquals(obj2, null) ||
                !object.ReferenceEquals(obj1, null) &&
                !object.ReferenceEquals(obj2, null) && obj1.Equals(obj2);
        }

        public static bool operator !=(TextLabel obj1, TextLabel obj2)
        {
            return !(obj1 == obj2);
        }

        public string GetLabel()
        {
            return Text;
        }

        public string GetFullLabel()
        {
            return Qualifier.Qualify(">", Text);
        }
    }
}
