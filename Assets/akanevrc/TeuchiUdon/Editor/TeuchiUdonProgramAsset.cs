using System;
using System.Collections.Generic;
using VRC.Udon.Editor.ProgramSources;
using VRC.Udon.Serialization.OdinSerializer;

namespace akanevrc.TeuchiUdon.Editor
{
    public class TeuchiUdonProgramAsset : UdonAssemblyProgramAsset
    {
        [NonSerialized, OdinSerialize]
        public Dictionary<string, (object value, Type type)> heapDefaultValues = new Dictionary<string, (object value, Type type)>();

        public void SetUdonAssembly(string assembly)
        {
            udonAssembly = assembly;
        }

        protected override void RefreshProgramImpl()
        {
            base.RefreshProgramImpl();
            ApplyDefaultValuesToHeap();
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

                if (val != null)
                {
                    if (!type.IsInstanceOfType(val))
                    {
                        val = type.IsValueType ? Activator.CreateInstance(type) : null;
                    }
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
