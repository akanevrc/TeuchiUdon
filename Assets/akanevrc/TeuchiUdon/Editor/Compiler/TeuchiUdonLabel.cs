using System;

namespace akanevrc.TeuchiUdon.Editor.Compiler
{
    public interface ITeuchiUdonLabel
    {
        string GetLabel();
    }

    public static class TeuchiUdonLabel
    {
        public static TextLabel InvalidLabel { get; } = new TextLabel("_invalid");
    }

    public class TextLabel : ITeuchiUdonLabel, IEquatable<TextLabel>
    {
        public string Text { get; }

        public TextLabel(string text)
        {
            Text = text;
        }

        public bool Equals(TextLabel obj)
        {
            return !object.ReferenceEquals(obj, null) && Text == obj.Text;
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
    }
}
