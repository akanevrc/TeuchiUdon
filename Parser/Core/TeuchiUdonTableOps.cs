using System;
using System.Collections.Generic;
using System.Linq;

namespace akanevrc.TeuchiUdon
{
    public class TeuchiUdonTableOps
    {
        private TeuchiUdonPrimitives Primitives { get; }
        private TeuchiUdonStaticTables StaticTables { get; }
        private TeuchiUdonTables Tables { get; }
        private TeuchiUdonTypeOps TypeOps { get; }
        private TeuchiUdonLabelOps LabelOps { get; }

        public TeuchiUdonTableOps
        (
            TeuchiUdonPrimitives primitives,
            TeuchiUdonStaticTables staticTables,
            TeuchiUdonTables tables,
            TeuchiUdonTypeOps typeOps,
            TeuchiUdonLabelOps labelOps
        )
        {
            Primitives   = primitives;
            StaticTables = staticTables;
            Tables       = tables;
            TypeOps      = typeOps;
            LabelOps     = labelOps;
        }

        public IEnumerable<TeuchiUdonMethod> GetMostCompatibleMethods(TeuchiUdonMethod query)
        {
            return GetMostCompatibleMethodsCore(query, query.InTypes.Length, false);
        }

        public IEnumerable<TeuchiUdonMethod> GetMostCompatibleMethodsWithoutInTypes(TeuchiUdonMethod query, int inTypeCount)
        {
            return GetMostCompatibleMethodsCore(query, inTypeCount, true);
        }

        private IEnumerable<TeuchiUdonMethod> GetMostCompatibleMethodsCore(TeuchiUdonMethod query, int inTypeCount, bool withoutInTypes)
        {
            if (!StaticTables.TypeToMethods.ContainsKey(query.Type)) return Enumerable.Empty<TeuchiUdonMethod>();
            var methodToMethods = StaticTables.TypeToMethods[query.Type];

            if (!methodToMethods.ContainsKey(query.Name)) return Enumerable.Empty<TeuchiUdonMethod>();
            var methods = methodToMethods[query.Name]
                .Where(x => x.InTypes.Length == inTypeCount)
                .ToArray();

            if (methods.Length == 0) return Enumerable.Empty<TeuchiUdonMethod>();
            if (withoutInTypes) return methods;

            var justCountToMethods = new Dictionary<int, List<TeuchiUdonMethod>>();
            foreach (var method in methods)
            {
                var isCompatible = true;
                var justCount    = 0;
                foreach (var (m, q) in method.InTypes.Zip(query.InTypes, (m, q) => (m, q)))
                {
                    if (!TypeOps.IsAssignableFrom(m, q))
                    {
                        isCompatible = false;
                        break;
                    }

                    if (m.LogicalTypeEquals(q)) justCount++;
                }

                if (isCompatible)
                {
                    if (!justCountToMethods.ContainsKey(justCount))
                    {
                        justCountToMethods.Add(justCount, new List<TeuchiUdonMethod>());
                    }
                    justCountToMethods[justCount].Add(method);
                }
            }

            for (var i = query.InTypes.Length; i >= 0; i--)
            {
                if (justCountToMethods.ContainsKey(i)) return justCountToMethods[i];
            }

            return Enumerable.Empty<TeuchiUdonMethod>();
        }

        public static string GetGenericTypeName(TeuchiUdonType rootType, IEnumerable<TeuchiUdonType> argTypes)
        {
            return $"{rootType.LogicalName}{string.Join("", argTypes.Select(x => x.LogicalName))}";
        }

        public static string GetGetterName(string name)
        {
            return $"get_{name}";
        }

        public static string GetSetterName(string name)
        {
            return $"set_{name}";
        }

        public static string GetEventName(string name)
        {
            return name.Length <= 1 ? name : $"_{char.ToLower(name[0])}{name.Substring(1)}";
        }

        public static string GetEventParamName(string eventName, string paramName)
        {
            var ev    = eventName.Length == 0 ? eventName : $"{char.ToLower(eventName[0])}{eventName.Substring(1)}";
            var param = paramName.Length == 0 ? paramName : $"{char.ToUpper(paramName[0])}{paramName.Substring(1)}";
            return $"{ev}{param}";
        }

        public static bool IsValidVarName(string name)
        {
            return name.Length >= 1 && !name.StartsWith("_");
        }

        public IEnumerable<(string name, object value, Type type)> GetDefaultValues()
        {
            return
                Tables.PublicVars             .Where(x => Tables.UsedData.Contains(x.Key)).Select(x => (LabelOps.GetFullLabel(x.Key), x.Value.Value  , x.Key.Type.RealType))
                .Concat(Tables.Literals.Values.Where(x => Tables.UsedData.Contains(x    )).Select(x => (LabelOps.GetFullLabel(x    ), x.Value        , x.Type    .RealType)))
                .Concat(Tables.Indirects      .Where(x => Tables.UsedData.Contains(x.Key)).Select(x => (LabelOps.GetFullLabel(x.Key), (object)x.Value, Primitives.UInt.RealType)));
        }
    }
}
