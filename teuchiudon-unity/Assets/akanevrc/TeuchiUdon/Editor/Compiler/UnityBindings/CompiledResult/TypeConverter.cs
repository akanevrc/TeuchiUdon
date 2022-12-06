using System;
using System.IO;
using System.Text;

namespace akanevrc.TeuchiUdon.Editor.Compiler
{
    public static class TypeConverter
    {
        public static (Type type, object value) ConvertLiteral(string typeName, string literal)
        {
            switch (typeName)
            {
                case "SystemByte":
                {
                    var (lit, basis) = ParseInteger(literal);
                    return (typeof(byte), Convert.ToByte(lit, basis));
                }
                case "SystemSByte":
                {
                    var (lit, basis) = ParseInteger(literal);
                    return (typeof(sbyte), Convert.ToSByte(lit, basis));
                }
                case "SystemInt16":
                {
                    var (lit, basis) = ParseInteger(literal);
                    return (typeof(short), Convert.ToInt16(lit, basis));
                }
                case "SystemUInt16":
                {
                    var (lit, basis) = ParseInteger(literal);
                    return (typeof(ushort), Convert.ToUInt16(lit, basis));
                }
                case "SystemInt32":
                {
                    var (lit, basis) = ParseInteger(literal);
                    return (typeof(int), Convert.ToInt32(lit, basis));
                }
                case "SystemUInt32":
                {
                    var (lit, basis) = ParseInteger(literal);
                    return (typeof(uint), Convert.ToUInt32(lit, basis));
                }
                case "SystemInt64":
                {
                    var (lit, basis) = ParseInteger(literal);
                    return (typeof(long), Convert.ToInt64(lit, basis));
                }
                case "SystemUInt64":
                {
                    var (lit, basis) = ParseInteger(literal);
                    return (typeof(ulong), Convert.ToUInt64(lit, basis));
                }
                case "SystemSingle":
                    return (typeof(float), Convert.ToSingle(literal));
                case "SystemDouble":
                    return (typeof(double), Convert.ToDouble(literal));
                case "SystemDecimal":
                    return (typeof(decimal), Convert.ToDecimal(literal));
                case "SystemChar":
                    return (typeof(char), EscapeRegularString(literal)[0]);
                case "SystemString":
                {
                    var (lit, kind) = ParseString(literal);
                    switch (kind)
                    {
                        case "regular":
                            return (typeof(string), EscapeRegularString(lit));
                        case "verbatium":
                            return (typeof(string), EscapeVerbatiumString(lit));
                        default:
                            throw new NotSupportedException();
                    }
                }
                default:
                    throw new NotImplementedException();
            }
        }

        private static (string literal, int basis) ParseInteger(string literal)
        {
            if (literal.StartsWith("0x"))
            {
                return (literal.Substring(2), 16);
            }
            else if (literal.StartsWith("0b"))
            {
                return (literal.Substring(2), 2);
            }
            else
            {
                return (literal, 10);
            }
        }

        private static (string literal, string kind) ParseString(string literal)
        {
            if (literal.StartsWith("@"))
            {
                return (literal.Substring(2, literal.Length - 3), "verbatium");
            }
            else
            {
                return (literal.Substring(1, literal.Length - 2), "regular");
            }
        }

        private static string EscapeRegularString(string text)
        {
            return EscapeString(text, reader =>
            {
                var ch = reader.Read();
                if (ch == -1) return null;
                if (ch == '\\')
                {
                    var escaped = reader.Read();
                    switch (escaped)
                    {
                        case '\'':
                            return "'";
                        case '"':
                            return "\"";
                        case '\\':
                            return "\\";
                        case '0':
                            return "\0";
                        case 'a':
                            return "\a";
                        case 'b':
                            return "\b";
                        case 'f':
                            return "\f";
                        case 'n':
                            return "\n";
                        case 'r':
                            return "\r";
                        case 't':
                            return "\t";
                        case 'v':
                            return "\v";
                        case 'x':
                        {
                            var d0 = CharNumberToInt(reader.Peek());
                            if (d0 == -1)
                            {
                                throw new InvalidOperationException("invalid char detected");
                            }
                            reader.Read();
                            var u = d0;

                            for (var i = 1; i < 4; i++)
                            {
                                var d = CharNumberToInt(reader.Peek());
                                if (d == -1)
                                {
                                    return ((char)u).ToString();
                                }
                                reader.Read();
                                u = u * 16 + d;
                            }

                            return ((char)u).ToString();
                        }
                        case 'u':
                        {
                            var u = 0L;
                            for (var i = 0; i < 4; i++)
                            {
                                var d = CharNumberToInt(reader.Read());
                                if (d == -1)
                                {
                                    throw new InvalidOperationException("invalid char detected");
                                }
                                u = u * 16 + d;
                            }
                            return ((char)u).ToString();
                        }
                        case 'U':
                        {
                            var u0 = 0L;
                            for (var i = 0; i < 4; i++)
                            {
                                var d = CharNumberToInt(reader.Read());
                                if (d == -1)
                                {
                                    throw new InvalidOperationException("invalid char detected");
                                }
                                u0 = u0 * 16 + d;
                            }
                            var u1 = 0L;
                            for (var i = 0; i < 4; i++)
                            {
                                var d = CharNumberToInt(reader.Read());
                                if (d == -1)
                                {
                                    throw new InvalidOperationException("invalid char detected");
                                }
                                u1 = u1 * 16 + d;
                            }
                            return ((char)u0).ToString() + ((char)u1).ToString();
                        }
                        default:
                            throw new InvalidOperationException("invalid char detected");
                    }
                }
                return ((char)ch).ToString();
            });
        }

        private static long CharNumberToInt(int ch)
        {
            if ('0' <= ch && ch <= '9') return ch - '0';
            if ('A' <= ch && ch <= 'F') return ch - 'A' + 10;
            if ('a' <= ch && ch <= 'f') return ch - 'a' + 10;
            return -1;
        }

        private static string EscapeVerbatiumString(string text)
        {
            return EscapeString(text, reader =>
            {
                var ch = reader.Read();
                if (ch == -1) return null;
                if (ch == '"')
                {
                    var escaped = reader.Read();
                    if (escaped == '"')
                    {
                        return "\"";
                    }
                    else
                    {
                        throw new InvalidOperationException("invalid char detected");
                    }
                }
                return ((char)ch).ToString();
            });
        }

        private static string EscapeString(string text, Func<StringReader, string> consumeFunc)
        {
            var result = new StringBuilder();
            using (var reader = new StringReader(text))
            {
                for (;;)
                {
                    var ch = consumeFunc(reader);
                    if (ch == null) return result.ToString();
                    result.Append(ch);
                }
            }
        }
    }
}
