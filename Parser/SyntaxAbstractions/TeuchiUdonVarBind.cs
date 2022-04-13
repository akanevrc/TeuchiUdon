using System;
using System.Collections.Generic;
using System.Linq;

namespace akanevrc.TeuchiUdon
{
    public class TeuchiUdonVarBind : IIndexedLabel, IEquatable<TeuchiUdonVarBind>
    {
        public int Index { get; }
        public TeuchiUdonQualifier Qualifier { get; }
        public string[] VarNames { get; }

        public TeuchiUdonVarBind(int index, TeuchiUdonQualifier qualifier, IEnumerable<string> varNames)
        {
            Index     = index;
            Qualifier = qualifier;
            VarNames  = varNames.ToArray();
        }

        public bool Equals(TeuchiUdonVarBind obj)
        {
            return !object.ReferenceEquals(obj, null) && Index == obj.Index;
        }

        public override bool Equals(object obj)
        {
            return obj is TeuchiUdonVarBind varBind ? Equals(varBind) : base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return Index.GetHashCode();
        }

        public static bool operator ==(TeuchiUdonVarBind obj1, TeuchiUdonVarBind obj2)
        {
            return
                 object.ReferenceEquals(obj1, null) &&
                 object.ReferenceEquals(obj2, null) ||
                !object.ReferenceEquals(obj1, null) &&
                !object.ReferenceEquals(obj2, null) && obj1.Equals(obj2);
        }

        public static bool operator !=(TeuchiUdonVarBind obj1, TeuchiUdonVarBind obj2)
        {
            return !(obj1 == obj2);
        }

        public string GetLabel()
        {
            return GetJoinedVarNames();
        }

        public string GetFullLabel()
        {
            return $"bind[{Qualifier.Qualify(">", GetJoinedVarNames())}]";
        }

        private string GetJoinedVarNames()
        {
            return
                VarNames.Length == 0 ? Index.ToString() :
                VarNames.Length == 1 ? GetVarName(VarNames[0]) :
                $"[{string.Join(">", VarNames.Select(x => GetVarName(x)))}]";
        }

        private string GetVarName(string name)
        {
            return TeuchiUdonTables.Instance.Events.ContainsKey(name) ? TeuchiUdonTables.GetEventName(name) : name;
        }
    }
}
