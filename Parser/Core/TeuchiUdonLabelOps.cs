using System.Linq;

namespace akanevrc.TeuchiUdon
{
    public class TeuchiUdonLabelOps
    {
        private TeuchiUdonStaticTables StaticTables { get; }
        private TeuchiUdonTables Tables { get; }

        public TeuchiUdonLabelOps(TeuchiUdonStaticTables staticTables, TeuchiUdonTables tables)
        {
            StaticTables = staticTables;
            Tables       = tables;
        }

        public string GetDescription(object obj)
        {
            switch (obj)
            {
                case TeuchiUdonFunc func:
                    return Qualify(func.Qualifier, ".", $"func[{func.Index}]({string.Join(", ", func.Vars.Select(x => GetDescription(x.Type)))})");
                case TextLabel text:
                    return text.Text;
                case TeuchiUdonMethod method:
                    return $"{GetDescription(method.Type)}.{method.Name}({string.Join(", ", method.InTypes.Select(x => GetDescription(x)))})";
                case TeuchiUdonQualifier qual:
                    return string.Join(".", qual.Logical.Select(x => GetDescription(x)));
                case TeuchiUdonScope scope:
                    return GetLabel(scope.Label);
                case TeuchiUdonThis this_:
                    return "this";
                case TeuchiUdonType type:
                    return $"{Qualify(type.Qualifier, ".", type.Name)}{(type.Args.Length == 0 ? "" : $"[{string.Join(", ", type.Args.Select(x => GetDescription(x)))}]")}";
                case TeuchiUdonVar var_:
                    return Qualify(var_.Qualifier, ".", var_.Name);
                default:
                    return "";
            }
        }

        public string GetLabel(ITeuchiUdonLabel label)
        {
            switch (label)
            {
                case TeuchiUdonBlock block:
                    return $"block[{block.Index}]";
                case TeuchiUdonBranch branch:
                    return $"branch[{branch.Index}]";
                case TeuchiUdonEvalFunc evalFunc:
                    return $"evalfunc[{evalFunc.Index}]";
                case TeuchiUdonFor for_:
                    return $"for[{for_.Index}]";
                case TeuchiUdonFunc func:
                    return $"func[{func.Index}]";
                case TeuchiUdonIndirect indirect:
                    return $"indirect[{GetLabel(indirect.Label)}]";
                case InvalidLabel invalid:
                    return "[invalid]";
                case TextLabel text:
                    return text.Text;
                case TeuchiUdonLetIn letIn:
                    return $"let[{letIn.Index}]";
                case TeuchiUdonLiteral literal:
                    return $"literal[{literal.Index}]";
                case TeuchiUdonLoop loop:
                    return $"loop[{loop.Index}]";
                case TeuchiUdonOutValue outValue:
                    return $"out[{GetLogicalName(outValue.Type)}>{outValue.Index}]";
                case TeuchiUdonReturn ret:
                    return $"return[{GetLabel(ret.Func)}]";
                case TeuchiUdonThis this_:
                    return "literal[this]";
                case TeuchiUdonVar var_:
                    return
                        StaticTables.Events.ContainsKey(var_.Name) && var_.Qualifier == TeuchiUdonQualifier.Top ?
                            TeuchiUdonTableOps.GetEventName(var_.Name) :
                        var_.IsSystemVar ?
                            var_.Name :
                        Tables.EventFuncs.ContainsKey(var_) ?
                            $"event[{var_.Name}]" :
                            $"var[{var_.Name}]";
                case TeuchiUdonVarBind varBind:
                    return GetJoinedVarNames(varBind);
                default:
                    return "";
            }
        }

        public string GetFullLabel(ITeuchiUdonLabel label)
        {
            switch (label)
            {
                case TeuchiUdonBlock block:
                    return $"block[{Qualify(block.Qualifier, ">", block.Index.ToString())}]";
                case TeuchiUdonBranch branch:
                    return $"branch[{branch.Index}]";
                case TeuchiUdonEvalFunc evalFunc:
                    return $"evalfunc[{Qualify(evalFunc.Qualifier, ">", evalFunc.Index.ToString())}]";
                case TeuchiUdonFor for_:
                    return $"for[{for_.Index}]";
                case TeuchiUdonFunc func:
                    return $"func[{Qualify(func.Qualifier, ">", func.Index.ToString())}]";
                case TeuchiUdonIndirect indirect:
                    return $"indirect[{GetFullLabel(indirect.Label)}]";
                case InvalidLabel invalid:
                    return "[invalid]";
                case TextLabel text:
                    return Qualify(text.Qualifier, ">", text.Text);
                case TeuchiUdonLetIn letIn:
                    return $"let[{Qualify(letIn.Qualifier, ">", letIn.Index.ToString())}]";
                case TeuchiUdonLiteral literal:
                    return $"literal[{literal.Index}]";
                case TeuchiUdonLoop loop:
                    return $"loop[{loop.Index}]";
                case TeuchiUdonOutValue outValue:
                    return $"out[{Qualify(outValue.Qualifier, ">", $"{GetLogicalName(outValue.Type)}>{outValue.Index}")}]";
                case TeuchiUdonReturn ret:
                    return $"return[{GetFullLabel(ret.Func)}]";
                case TeuchiUdonThis this_:
                    return "literal[this]";
                case TeuchiUdonVar var_:
                    return
                        StaticTables.Events.ContainsKey(var_.Name) && var_.Qualifier == TeuchiUdonQualifier.Top ?
                            TeuchiUdonTableOps.GetEventName(var_.Name) :
                        var_.IsSystemVar ?
                            var_.Name :
                        Tables.EventFuncs.ContainsKey(var_) ?
                            $"event[{Qualify(var_.Qualifier, ">", var_.Name)}]" :
                            $"var[{Qualify(var_.Qualifier, ">", var_.Name)}]";
                case TeuchiUdonVarBind varBind:
                    return $"bind[{Qualify(varBind.Qualifier, ">", GetJoinedVarNames(varBind))}]";
                default:
                    return "";
            }
        }

        private string GetJoinedVarNames(TeuchiUdonVarBind varBind)
        {
            return
                varBind.VarNames.Length == 0 ? varBind.Index.ToString() :
                varBind.VarNames.Length == 1 ? GetVarName(varBind.VarNames[0]) :
                $"[{string.Join(">", varBind.VarNames.Select(x => GetVarName(x)))}]";
        }

        private string GetVarName(string name)
        {
            return StaticTables.Events.ContainsKey(name) ? TeuchiUdonTableOps.GetEventName(name) : name;
        }

        public string GetLogicalName(ITeuchiUdonTypeArg typeArg)
        {
            switch (typeArg)
            {
                case TeuchiUdonType type:
                    return $"{type.LogicalName}{string.Join("", type.Args.Select(x => GetLogicalName(x)))}";
                case TeuchiUdonQualifier qual:
                    return string.Join("", qual.Logical.Select(x => GetLabel(x.Label)));
                case TeuchiUdonMethod method:
                    return $"{GetLogicalName(method.Type)}{method.Name}{string.Join("", method.InTypes.Select(x => GetLogicalName(x)))}";
                default:
                    return "";
            }
        }

        public string Qualify(TeuchiUdonQualifier qualifier, string separator, string text)
        {
            return string.Join(separator, qualifier.Logical.Select(x => GetLabel(x.Label)).Concat(new string[] { text }));
        }
    }
}
