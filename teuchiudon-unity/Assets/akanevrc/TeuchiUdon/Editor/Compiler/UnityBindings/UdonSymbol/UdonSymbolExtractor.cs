using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;
using VRC.Udon.Editor;
using VRC.Udon.Graph;

namespace akanevrc.TeuchiUdon.Editor.Compiler
{
    internal class UdonSymbolExtractor
    {
        private Dictionary<string, UdonTypeSymbol> Types { get; } = new Dictionary<string, UdonTypeSymbol>();
        private Dictionary<string, UdonArrayTypeSymbol> ArrayTypes { get; } = new Dictionary<string, UdonArrayTypeSymbol>();
        private Dictionary<string, UdonGenericBaseTypeSymbol> GenericBaseTypes { get; } = new Dictionary<string, UdonGenericBaseTypeSymbol>();
        private Dictionary<string, UdonMethodSymbol> Methods { get; } = new Dictionary<string, UdonMethodSymbol>();
        private Dictionary<string, UdonEventSymbol> Events { get; } = new Dictionary<string, UdonEventSymbol>();

        public void ExtractSymbols()
        {
            var udonBehaviourType = GetType(typeof(UdonBehaviour));
            var componentType = GetType(typeof(Component));
            var udonEventReceiverType = GetType(typeof(IUdonEventReceiver));

            var topRegistries = UdonEditorManager.Instance.GetTopRegistries();
            foreach (var topReg in topRegistries)
            {
                foreach (var reg in topReg.Value)
                {
                    foreach (var def in reg.Value.GetNodeDefinitions())
                    {
                        if (def.type == null) continue;

                        var type = GetType(def.type);

                        var methodName = def.name.Substring(def.name.LastIndexOf(' ') + 1);
                        var parameters = def.parameters;
                        var paramTypes =
                            parameters
                            .Select(x => GetType(x.type))
                            .ToArray();
                        var paramInOuts =
                            parameters
                            .Select(x => Enum.GetName(typeof(UdonNodeParameter.ParameterType), x.parameterType))
                            .ToArray();
                        var instanceType =
                            paramTypes
                            .Zip(parameters, (t, p) => (t, p))
                            .FirstOrDefault(x => x.p.parameterType == UdonNodeParameter.ParameterType.IN && x.p.name == "instance")
                            .t;
                        var paramUdonNames =
                            parameters
                            .Select(x => x.name)
                            .ToArray();

                        var method =
                            new UdonMethodSymbol
                            (
                                instanceType == null,
                                instanceType ?? type,
                                methodName,
                                paramTypes,
                                paramInOuts,
                                def.fullName,
                                paramUdonNames
                            );
                        AddMethod(method);

                        if (instanceType == componentType || instanceType == udonEventReceiverType)
                        {
                            var udonBehaviourMethod =
                                new UdonMethodSymbol
                                (
                                    false,
                                    udonBehaviourType,
                                    methodName,
                                    paramTypes,
                                    paramInOuts,
                                    def.fullName,
                                    paramUdonNames
                                );
                            AddMethod(udonBehaviourMethod);
                        }
                    }
                }
            }
        }

        private string GetType(Type type)
        {
            var realName = GetTypeRealName(type);

            if (type.IsArray && !ArrayTypes.ContainsKey(realName))
            {
                var elemType = GetType(type.GetElementType());

                var arrayType = new UdonArrayTypeSymbol(realName, elemType);
                ArrayTypes.Add(realName, arrayType);
            }
            else if (!Types.ContainsKey(realName))
            {
                var typeName = GetTypeSimpleName(type);
                var argTypes = type.GenericTypeArguments.Select(x => GetType(x)).ToArray();
                var scopes = GetQualScopes(type);

                var t = new UdonTypeSymbol(scopes, typeName, realName, realName, argTypes);
                Types.Add(realName, t);

                if (argTypes.Length != 0)
                {
                    var genericName = GetQualifiedName(scopes, typeName);

                    if (!GenericBaseTypes.ContainsKey(genericName))
                    {
                        var genericType = new UdonGenericBaseTypeSymbol(scopes, typeName, genericName);
                        GenericBaseTypes.Add(genericName, genericType);
                    }
                }
            }

            return realName;
        }

        private void AddMethod(UdonMethodSymbol method)
        {
            if (method.RealName.StartsWith("Const_")) return;
            if (method.RealName.StartsWith("Variable_")) return;

            if (method.RealName.StartsWith("Event_"))
            {
                if (!Events.ContainsKey(method.Name))
                {
                    var ev = new UdonEventSymbol(method.Name, method.ParamTypes, method.ParamInOuts, method.RealName, method.ParamRealNames);
                    Events.Add(ev.Name, ev);
                }
                return;
            }
            
            if (method.Type == "SystemVoid") return;

            if (method.RealName.EndsWith("__T")) return;

            var key = GetMethodKey(method);
            if (!Methods.ContainsKey(key))
            {
                Methods.Add(key, method);
            }
        }

        private string GetTypeRealName(Type type)
        {
            var genericIndex = type.FullName.IndexOf('`');
            var arrayIndex   = type.FullName.IndexOf('[');
            var index        = genericIndex == -1 ? arrayIndex : arrayIndex == -1 ? genericIndex : genericIndex < arrayIndex ? genericIndex : arrayIndex;
            var name         = index == -1 ? type.FullName : type.FullName.Substring(0, index);
            name = name.Replace(".", "");
            name = name.Replace("+", "");
            name = $"{name}{(type.IsGenericType ? string.Join("", type.GenericTypeArguments.Select(x => GetTypeRealName(x))) : "")}";
            name = $"{name}{(type.IsArray ? "Array" : "")}";
            return name;
        }

        private string GetTypeSimpleName(Type type)
        {
            var genericIndex = type.Name.IndexOf('`');
            var arrayIndex   = type.Name.IndexOf('[');
            var index        = genericIndex == -1 ? arrayIndex : arrayIndex == -1 ? genericIndex : genericIndex < arrayIndex ? genericIndex : arrayIndex;
            var name         = index == -1 ? type.Name : type.Name.Substring(0, index);
            name = $"{name}{(type.IsArray ? "Array" : "")}";
            return name;
        }

        private string[] GetQualScopes(Type type)
        {
            var genericIndex = type.FullName.IndexOf('`');
            var typeName     = genericIndex == -1 ? type.FullName : type.FullName.Substring(0, genericIndex);
            var qualNames    = typeName.Split(new char[] { '.', '+' });
            return qualNames.Take(qualNames.Length - 1).ToArray();
        }

        private string GetQualifiedName(IEnumerable<string> scopes, string name)
        {
            return string.Join("", scopes.Concat(new string[] { name }));
        }

        private string GetMethodKey(UdonMethodSymbol method)
        {
            return $"{method.Type}{(method.IsStatic ? "::" : "..")}{method.RealName}";
        }
    }
}
