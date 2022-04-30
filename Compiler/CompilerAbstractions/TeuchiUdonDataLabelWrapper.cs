using System.Collections.Generic;

namespace akanevrc.TeuchiUdon.Compiler
{
    public class TeuchiUdonDataLabelWrapper : IDataLabel
    {
        public delegate IEnumerable<TeuchiUdonAssembly> LabelToAssembly(IDataLabel label);
        public delegate IEnumerable<TeuchiUdonAssembly> ListToAssembly(TeuchiUdonDataLabelWrapper label, TeuchiUdonDataLabelWrapper thisObj);

        public IDataLabel Label { get; }
        public Dictionary<string, IDataLabel> AssemblyLabels { get; set; }
        public TeuchiUdonType Type => Label.Type;
        public bool Reverse { get; }
        public LabelToAssembly LabelFunc { get; }
        public ListToAssembly ListFunc { get; }

        public TeuchiUdonDataLabelWrapper
        (
            IDataLabel label,
            bool reverse,
            LabelToAssembly labelFunc,
            ListToAssembly  listFunc
        )
        {
            Label     = label;
            Reverse   = reverse;
            LabelFunc = labelFunc;
            ListFunc  = listFunc;
        }
    }
}
