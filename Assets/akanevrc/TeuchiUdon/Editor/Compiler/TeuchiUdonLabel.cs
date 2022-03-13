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

    public interface IDataLabel : ITeuchiUdonLabel
    {
        TeuchiUdonType Type { get; }
    }

    public interface ICodeLabel : ITeuchiUdonLabel
    {
    }

    public class InvalidLabel : IIndexedLabel, IDataLabel, ICodeLabel, IEquatable<InvalidLabel>
    {
        public static InvalidLabel Instance = new InvalidLabel();
        public int Index { get; } = -1;
        public TeuchiUdonType Type { get; } = TeuchiUdonType.Bottom;

        protected InvalidLabel()
        {
        }

        public bool Equals(InvalidLabel obj)
        {
            return !object.ReferenceEquals(obj, null) && Index == obj.Index;
        }

        public override bool Equals(object obj)
        {
            return obj is InvalidLabel label ? Equals(label) : base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return Index.GetHashCode();
        }

        public static bool operator ==(InvalidLabel obj1, InvalidLabel obj2)
        {
            return
                 object.ReferenceEquals(obj1, null) &&
                 object.ReferenceEquals(obj2, null) ||
                !object.ReferenceEquals(obj1, null) &&
                !object.ReferenceEquals(obj2, null) && obj1.Equals(obj2);
        }

        public static bool operator !=(InvalidLabel obj1, InvalidLabel obj2)
        {
            return !(obj1 == obj2);
        }

        public string GetLabel() => "[invalid]";
        public string GetFullLabel() => "[invalid]";
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

    public class TextDataLabel : TextLabel, IDataLabel, IEquatable<TextDataLabel>
    {
        public TeuchiUdonType Type { get; }

        public TextDataLabel(string text, TeuchiUdonType type)
            : this(TeuchiUdonQualifier.Top, text, type)
        {
        }

        public TextDataLabel(TeuchiUdonQualifier qualifier, string text, TeuchiUdonType type)
            : base(qualifier, text)
        {
            Type = type;
        }

        public bool Equals(TextDataLabel obj)
        {
            return !object.ReferenceEquals(obj, null) && Qualifier == obj.Qualifier && Text == obj.Text;
        }

        public override bool Equals(object obj)
        {
            return obj is TextDataLabel label ? Equals(label) : base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return Text.GetHashCode();
        }

        public static bool operator ==(TextDataLabel obj1, TextDataLabel obj2)
        {
            return
                 object.ReferenceEquals(obj1, null) &&
                 object.ReferenceEquals(obj2, null) ||
                !object.ReferenceEquals(obj1, null) &&
                !object.ReferenceEquals(obj2, null) && obj1.Equals(obj2);
        }

        public static bool operator !=(TextDataLabel obj1, TextDataLabel obj2)
        {
            return !(obj1 == obj2);
        }
    }

    public class TextCodeLabel : TextLabel, ICodeLabel, IEquatable<TextCodeLabel>
    {
        public TextCodeLabel(string text)
            : this(TeuchiUdonQualifier.Top, text)
        {
        }

        public TextCodeLabel(TeuchiUdonQualifier qualifier, string text)
            : base(qualifier, text)
        {
        }

        public bool Equals(TextCodeLabel obj)
        {
            return !object.ReferenceEquals(obj, null) && Qualifier == obj.Qualifier && Text == obj.Text;
        }

        public override bool Equals(object obj)
        {
            return obj is TextCodeLabel label ? Equals(label) : base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return Text.GetHashCode();
        }

        public static bool operator ==(TextCodeLabel obj1, TextCodeLabel obj2)
        {
            return
                 object.ReferenceEquals(obj1, null) &&
                 object.ReferenceEquals(obj2, null) ||
                !object.ReferenceEquals(obj1, null) &&
                !object.ReferenceEquals(obj2, null) && obj1.Equals(obj2);
        }

        public static bool operator !=(TextCodeLabel obj1, TextCodeLabel obj2)
        {
            return !(obj1 == obj2);
        }
    }
}
