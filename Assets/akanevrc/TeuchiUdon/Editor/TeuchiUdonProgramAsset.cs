using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;
using VRC.Udon.Editor.ProgramSources;
using VRC.Udon.Serialization.OdinSerializer;

namespace akanevrc.TeuchiUdon.Editor
{
    public class TeuchiUdonProgramAsset : UdonAssemblyProgramAsset
    {
        [NonSerialized, OdinSerialize]
        public Dictionary<string, (object value, Type type)> heapDefaultValues = new Dictionary<string, (object value, Type type)>();

        [SerializeField, HideInInspector]
        private SerializationData serializationData;

        [SerializeField, HideInInspector]
        private bool showAssembly = true;

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

        protected override void DrawProgramSourceGUI(UdonBehaviour udonBehaviour, ref bool dirty)
        {
            DrawInteractionArea(udonBehaviour);
            DrawPublicVariables(udonBehaviour, ref dirty);
            DrawAssemblyErrorTextArea();
            DrawAssemblyTextArea(false, ref dirty);
        }

        protected override void DrawAssemblyTextArea(bool allowEditing, ref bool dirty)
        {
            EditorGUI.BeginChangeCheck();

            var newShowAssembly = EditorGUILayout.Foldout(showAssembly, "Compiled TeuchiUdon Assembly");

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(this, "Toggle Assembly Foldout");
                showAssembly = newShowAssembly;
            }

            if (!showAssembly) return;

            EditorGUI.indentLevel++;
            base.DrawAssemblyTextArea(allowEditing, ref dirty);
            EditorGUI.indentLevel--;
        }

        protected override object DrawPublicVariableField(string symbol, object variableValue, Type variableType, ref bool dirty, bool enabled)
        {
            EditorGUILayout.BeginHorizontal();

            var fieldName = symbol.StartsWith("var[") && symbol.EndsWith("]") ? symbol.Substring(4, symbol.Length - 5) : symbol;
            var fieldType = variableType == typeof(IUdonEventReceiver) ? typeof(UdonBehaviour) : variableType;

            variableValue    = base.DrawPublicVariableField(fieldName, variableValue, fieldType, ref dirty, enabled);
            var defaultValue = (object)null;
            if (heapDefaultValues.ContainsKey(symbol))
            {
                defaultValue = heapDefaultValues[symbol].value;
            }

            if (variableValue == null || !variableValue.Equals(defaultValue))
            {
                if(defaultValue != null)
                {
                    if(!dirty && GUILayout.Button("Reset to Default Value"))
                    {
                        variableValue = defaultValue;
                        dirty = true;
                    }
                }
            }

            EditorGUILayout.EndHorizontal();

            return variableValue;
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

        protected override void OnBeforeSerialize()
        {
            UnitySerializationUtility.SerializeUnityObject(this, ref serializationData);
            base.OnBeforeSerialize();
        }

        protected override void OnAfterDeserialize()
        {
            UnitySerializationUtility.DeserializeUnityObject(this, ref serializationData);
            base.OnAfterDeserialize();
        }
    }
}
