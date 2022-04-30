using System;
using System.Collections.Generic;
using System.Linq;

namespace akanevrc.TeuchiUdon.Compiler
{
    public class TeuchiUdonDataLabelWrapperOps
    {
        private TeuchiUdonPrimitives Primitives { get; }
        private TeuchiUdonTypeOps TypeOps { get; }
        private TeuchiUdonLabelOps LabelOps { get; }

        public TeuchiUdonDataLabelWrapperOps
        (
            TeuchiUdonPrimitives primitives,
            TeuchiUdonTypeOps typeOps,
            TeuchiUdonLabelOps labelOps
        )
        {
            Primitives = primitives;
            TypeOps    = typeOps;
            LabelOps   = labelOps;
        }

        public TeuchiUdonDataLabelWrapper CreateWrapper
        (
            IDataLabel label,
            bool reverse,
            TeuchiUdonDataLabelWrapper.LabelToAssembly labelFunc,
            TeuchiUdonDataLabelWrapper.ListToAssembly  listFunc
        )
        {
            var wrapper = new TeuchiUdonDataLabelWrapper(label, reverse, labelFunc, listFunc);
            wrapper.AssemblyLabels = CreateAssemblyLabels(wrapper, label).ToDictionary(x => x.name, x => x.label);
            return wrapper;
        }

        private IEnumerable<(string name, IDataLabel label)> CreateAssemblyLabels(TeuchiUdonDataLabelWrapper wrapper, IDataLabel label)
        {
            return
                TypeOps.GetMembers(label.Type)
                .SelectMany
                (
                    x => x.name == "" ?
                    new (string, IDataLabel)[] { (x.name, label) } :
                    CreateAssemblyLabels(wrapper, CreateOneAssemblyLabel(wrapper, label, x.name, x.type))
                );
        }

        private IDataLabel CreateOneAssemblyLabel(TeuchiUdonDataLabelWrapper wrapper, IDataLabel label, string name, TeuchiUdonType type)
        {
            return new TeuchiUdonDataLabelWrapper
            (
                new TextDataLabel($"{LabelOps.GetFullLabel(label)}>{label.Type.LogicalName}[{name}]", type),
                wrapper.Reverse,
                wrapper.LabelFunc,
                wrapper.ListFunc
            );
        }

        public IEnumerable<TeuchiUdonAssembly> Compile(TeuchiUdonDataLabelWrapper wrapper)
        {
            return VisitDataLabel(wrapper, wrapper);
        }

        public IEnumerable<TeuchiUdonAssembly> VisitDataLabel(TeuchiUdonDataLabelWrapper wrapper, IDataLabel label)
        {
            if (label is TeuchiUdonDataLabelWrapper wr)
            {
                if (wrapper.Type.LogicalTypeNameEquals(Primitives.List))
                {
                    return wrapper.ListFunc(wr, wrapper);
                }
                else
                {
                    return IterateAssemblyLabels(wr).SelectMany(x => VisitDataLabel(wrapper, x));
                }
            }
            else
            {
                return wrapper.LabelFunc(label);
            }
        }

        public IEnumerable<IDataLabel> IterateAssemblyLabels(TeuchiUdonDataLabelWrapper wrapper)
        {
            return wrapper.Reverse ? wrapper.AssemblyLabels.Values.Reverse() : wrapper.AssemblyLabels.Values;
        }
    }
}
