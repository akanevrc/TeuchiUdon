using System;
using System.Collections.Generic;
using System.Linq;

namespace akanevrc.TeuchiUdon.Compiler
{
    public class TeuchiUdonDataLabelWrapper : IDataLabel
    {
        public delegate IEnumerable<TeuchiUdonAssembly> LabelToAssembly(IDataLabel label);
        public delegate IEnumerable<TeuchiUdonAssembly> ListToAssembly(TeuchiUdonDataLabelWrapper label, TeuchiUdonDataLabelWrapper thisObj);

        public IDataLabel Label { get; }
        public Dictionary<string, IDataLabel> AssemblyLabels { get; }
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
            Label          = label;
            AssemblyLabels = CreateAssemblyLabels(label).ToDictionary(x => x.name, x => x.label);
            Reverse        = reverse;
            LabelFunc      = labelFunc;
            ListFunc       = listFunc;
        }

        private IEnumerable<(string name, IDataLabel label)> CreateAssemblyLabels(IDataLabel label)
        {
            return
                label.Type.GetMembers()
                .SelectMany
                (
                    x => x.name == "" ?
                    new (string, IDataLabel)[] { (x.name, label) } :
                    CreateAssemblyLabels(CreateOneAssemblyLabel(label, x.name, x.type))
                );
        }

        private IDataLabel CreateOneAssemblyLabel(IDataLabel label, string name, TeuchiUdonType type)
        {
            return new TeuchiUdonDataLabelWrapper
            (
                new TextDataLabel($"{label.GetFullLabel()}>{label.Type.LogicalName}[{name}]", type),
                Reverse,
                LabelFunc,
                ListFunc
            );
        }

        public IEnumerable<TeuchiUdonAssembly> Compile()
        {
            return VisitDataLabel(this);
        }

        public IEnumerable<TeuchiUdonAssembly> VisitDataLabel(IDataLabel label)
        {
            if (label is TeuchiUdonDataLabelWrapper wrapper)
            {
                if (wrapper.Type.LogicalTypeNameEquals(PrimitiveTypes.Instance.List))
                {
                    return ListFunc(wrapper, this);
                }
                else
                {
                    return wrapper.IterateAssemblyLabels().SelectMany(x => VisitDataLabel(x));
                }
            }
            else
            {
                return LabelFunc(label);
            }
        }

        public IEnumerable<IDataLabel> IterateAssemblyLabels()
        {
            return Reverse ? AssemblyLabels.Values.Reverse() : AssemblyLabels.Values;
        }

        public string GetLabel()
        {
            throw new NotSupportedException();
        }

        public string GetFullLabel()
        {
            throw new NotSupportedException();
        }
    }
}
