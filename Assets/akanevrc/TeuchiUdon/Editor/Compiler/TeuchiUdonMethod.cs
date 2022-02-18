using System;
using System.Collections.Generic;
using System.Linq;

namespace akanevrc.TeuchiUdon.Editor.Compiler
{
    public enum TeuchiUdonMethodParamInOut
    {
        In,
        InOut,
        Out
    }

    public class TeuchiUdonMethod : IEquatable<TeuchiUdonMethod>, ITeuchiUdonTypeArg
    {
        public TeuchiUdonType Type { get; }
        public string Name { get; }
        public TeuchiUdonType[] InTypes { get; }
        public TeuchiUdonType[] OutTypes { get; }
        public TeuchiUdonType[] AllParamTypes { get; }
        public TeuchiUdonMethodParamInOut[] AllParamInOuts { get; }
        public string UdonName { get; }

        public TeuchiUdonMethod(TeuchiUdonType type, string name, IEnumerable<TeuchiUdonType> inTypes)
            : this(type, name, inTypes, null, null, null, null)
        {
        }

        public TeuchiUdonMethod
        (
            TeuchiUdonType type,
            string name,
            IEnumerable<TeuchiUdonType> inTypes,
            IEnumerable<TeuchiUdonType> outTypes,
            IEnumerable<TeuchiUdonType> allParamTypes,
            IEnumerable<TeuchiUdonMethodParamInOut> allParamInOuts,
            string udonName
        )
        {
            Type           = type;
            Name           = name;
            InTypes        = inTypes       ?.ToArray();
            OutTypes       = outTypes      ?.ToArray();
            AllParamTypes  = allParamTypes ?.ToArray();
            AllParamInOuts = allParamInOuts?.ToArray();
            UdonName       = udonName;
        }

        public bool Equals(TeuchiUdonMethod obj)
        {
            return !object.ReferenceEquals(obj, null) && Type.LogicalTypeEquals(obj.Type) && Name == obj.Name && InTypes.SequenceEqual(obj.InTypes, TeuchiUdonTypeLogicalEqualityComparer.Instance);
        }

        public override bool Equals(object obj)
        {
            return obj is TeuchiUdonMethod method ? Equals(method) : base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }

        public static bool operator ==(TeuchiUdonMethod obj1, TeuchiUdonMethod obj2)
        {
            return
                 object.ReferenceEquals(obj1, null) &&
                 object.ReferenceEquals(obj2, null) ||
                !object.ReferenceEquals(obj1, null) &&
                !object.ReferenceEquals(obj2, null) && obj1.Equals(obj2);
        }

        public static bool operator !=(TeuchiUdonMethod obj1, TeuchiUdonMethod obj2)
        {
            return !(obj1 == obj2);
        }

        public override string ToString()
        {
            return $"{Type}.{Name}({string.Join<TeuchiUdonType>(", ", InTypes)})";
        }

        public IEnumerable<T> SortAlongParams<T>(IEnumerable<T> inTypes, IEnumerable<T> outTypes)
        {
            var inTypesArr  = inTypes .ToArray();
            var outTypesArr = outTypes.ToArray();

            if
            (
                inTypesArr .Length != AllParamInOuts.Count(x => x == TeuchiUdonMethodParamInOut.In || x == TeuchiUdonMethodParamInOut.InOut) ||
                outTypesArr.Length != AllParamInOuts.Count(x => x == TeuchiUdonMethodParamInOut.Out)
            )
            {
                TeuchiUdonLogicalErrorHandler.Instance.ReportError(null, $"invalid param count");
                yield break;
            }

            var i = 0;
            var o = 0;
            for (var p = 0; p < AllParamInOuts.Length; p++)
            {
                if (AllParamInOuts[p] == TeuchiUdonMethodParamInOut.In || AllParamInOuts[p] == TeuchiUdonMethodParamInOut.InOut)
                {
                    yield return inTypesArr[i++];
                }
                else
                {
                    yield return outTypesArr[o++];
                }
            }
        }
    }
}
