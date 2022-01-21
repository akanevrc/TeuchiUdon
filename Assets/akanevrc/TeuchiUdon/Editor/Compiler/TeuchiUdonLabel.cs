using System;

namespace akanevrc.TeuchiUdon.Editor.Compiler
{
    public interface ITeuchiUdonLabel
    {
        string GetLabel();
        string GetFullLabel();
    }

    public static class TeuchiUdonLabel
    {
        public static TextLabel InvalidLabel { get; } = new TextLabel("_invalid");
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
