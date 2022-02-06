using System;
using System.Collections.Generic;
using System.Linq;
using VRC.Udon.Editor.ProgramSources;
using VRC.Udon.Serialization.OdinSerializer;

namespace akanevrc.TeuchiUdon.Editor
{
    public class TeuchiUdonProgramAsset : UdonAssemblyProgramAsset
    {
        [NonSerialized, OdinSerialize]
        public Dictionary<string, (object value, Type type)> heapDefaultValues = new Dictionary<string, (object value, Type type)>();

        public void SetUdonAssembly(string assembly, IEnumerable<(string name, object value, Type type)> defaultValues)
        {
            udonAssembly      = assembly;
            heapDefaultValues = defaultValues.ToDictionary(x => x.name, x => (x.value, x.type));
        }

        protected override void RefreshProgramImpl()
        {
            base.RefreshProgramImpl();
            ApplyDefaultValuesToHeap();
        }

        protected override object GetPublicVariableDefaultValue(string symbol, Type type)
        {
            var symbolTable = program?.SymbolTable;
            var heap        = program?.Heap;
            if (symbolTable == null || heap == null) return null;

            if (!heapDefaultValues.ContainsKey(symbol)) return null;

            var (val, t) = heapDefaultValues[symbol];
            if (typeof(UnityEngine.Object).IsAssignableFrom(t))
            {
                if (val != null && !t.IsInstanceOfType(val)) return null;
                if ((UnityEngine.Object)val == null)         return null;
            }

            if (val != null && !t.IsInstanceOfType(val))
            {
                return t.IsValueType ? Activator.CreateInstance(t) : null;
            }

            return val;
        }

        protected void ApplyDefaultValuesToHeap()
        {
            var symbolTable = program?.SymbolTable;
            var heap        = program?.Heap;
            if (symbolTable == null || heap == null) return;

            foreach (var defaultValue in heapDefaultValues)
            {
                if (!symbolTable.HasAddressForSymbol(defaultValue.Key)) continue;

                var symbolAddress = symbolTable.GetAddressFromSymbol(defaultValue.Key);
                var (val, type) = defaultValue.Value;
                if (typeof(UnityEngine.Object).IsAssignableFrom(type))
                {
                    if (val != null && !type.IsInstanceOfType(val))
                    {
                        heap.SetHeapVariable(symbolAddress, null, type);
                        continue;
                    }

                    if ((UnityEngine.Object)val == null)
                    {
                        heap.SetHeapVariable(symbolAddress, null, type);
                        continue;
                    }
                }

                if (val != null && !type.IsInstanceOfType(val))
                {
                    val = type.IsValueType ? Activator.CreateInstance(type) : null;
                }

                if(type == null)
                {
                    type = typeof(object);
                }

                heap.SetHeapVariable(symbolAddress, val, type);
            }
        }
    }
}
