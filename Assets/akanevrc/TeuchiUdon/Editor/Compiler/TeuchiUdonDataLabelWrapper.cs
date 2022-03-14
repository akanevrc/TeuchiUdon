using System;
using System.Collections.Generic;
using System.Linq;

namespace akanevrc.TeuchiUdon.Editor.Compiler
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
            AssemblyLabels = CreateAssemblyLabels(label);
            Reverse        = reverse;
            LabelFunc      = labelFunc;
            ListFunc       = listFunc;
        }

        private Dictionary<string, IDataLabel> CreateAssemblyLabels(IDataLabel label)
        {
            if (label.Type.LogicalTypeEquals(TeuchiUdonType.Unit))
            {
                return new Dictionary<string, IDataLabel>();
            }
            else if (label.Type.RealType == null)
            {
                throw new InvalidOperationException("no real type");
            }
            else if (label.Type.LogicalTypeNameEquals(TeuchiUdonType.Tuple))
            {
                return label.Type.GetArgsAsTuple()
                    .Select((x, i) => CreateOneAssemblyLabel(label, i.ToString(), x))
                    .ToDictionary(x => x.name, x => x.label);
            }
            else if (label.Type.LogicalTypeNameEquals(TeuchiUdonType.List))
            {
                return new (string name, IDataLabel label)[] {
                    CreateOneAssemblyLabel(label, "buffer", TeuchiUdonType.Buffer),
                    CreateOneAssemblyLabel(label, "length", TeuchiUdonType.Int)
                }
                .ToDictionary(x => x.name, x => x.label);
            }
            else
            {
                return new Dictionary<string, IDataLabel>() { [""] = label };
            }
        }

        private (string name, IDataLabel label) CreateOneAssemblyLabel(IDataLabel label, string name, TeuchiUdonType type)
        {
            return
            (
                name,
                new TeuchiUdonDataLabelWrapper
                (
                    new TextDataLabel($"{label.GetFullLabel()}>{label.Type.LogicalName}[{name}]", type),
                    Reverse,
                    LabelFunc,
                    ListFunc
                )
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
                if (wrapper.Type.LogicalTypeNameEquals(TeuchiUdonType.List))
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
