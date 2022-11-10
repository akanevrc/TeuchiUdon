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
        private bool Initialized { get; set; } = false;
        private Dictionary<string, BaseTySymbol> BaseTys { get; set; }
        private Dictionary<string, TySymbol> Tys { get; set; }
        private Dictionary<string, MethodSymbol> Methods { get; set; }
        private Dictionary<string, EvSymbol> Evs { get; set; }

        public UdonSymbols ExtractSymbols()
        {
            if (!Initialized)
            {
                BaseTys = new Dictionary<string, BaseTySymbol>();
                Tys = new Dictionary<string, TySymbol>();
                Methods = new Dictionary<string, MethodSymbol>();
                Evs = new Dictionary<string, EvSymbol>();

                var udonBehaviourTy = GetTy(typeof(UdonBehaviour));
                var componentTy = GetTy(typeof(Component));
                var udonEventReceiverTy = GetTy(typeof(IUdonEventReceiver));

                var topRegistries = UdonEditorManager.Instance.GetTopRegistries();
                foreach (var topReg in topRegistries)
                {
                    foreach (var reg in topReg.Value)
                    {
                        foreach (var def in reg.Value.GetNodeDefinitions())
                        {
                            if (def.type == null) continue;

                            var ty = GetTy(def.type);

                            var methodName = def.name.Substring(def.name.LastIndexOf(' ') + 1);
                            var parameters = def.parameters;
                            var paramTys =
                                parameters
                                .Select(x => GetTy(x.type))
                                .ToArray();
                            var paramInOuts =
                                parameters
                                .Select(x => Enum.GetName(typeof(UdonNodeParameter.ParameterType), x.parameterType))
                                .ToArray();
                            var instanceTy =
                                paramTys
                                .Zip(parameters, (t, p) => (t, p))
                                .FirstOrDefault(x => x.p.parameterType == UdonNodeParameter.ParameterType.IN && x.p.name == "instance")
                                .t;
                            var paramUdonNames =
                                parameters
                                .Select(x => x.name)
                                .ToArray();

                            var method =
                                new MethodSymbol
                                (
                                    instanceTy == null,
                                    instanceTy ?? ty,
                                    methodName,
                                    paramTys,
                                    paramInOuts,
                                    def.fullName,
                                    paramUdonNames
                                );
                            AddMethod(method);

                            if (instanceTy == componentTy || instanceTy == udonEventReceiverTy)
                            {
                                var udonBehaviourMethod =
                                    new MethodSymbol
                                    (
                                        false,
                                        udonBehaviourTy,
                                        methodName,
                                        paramTys,
                                        paramInOuts,
                                        def.fullName,
                                        paramUdonNames
                                    );
                                AddMethod(udonBehaviourMethod);
                            }
                        }
                    }
                }
                Initialized = true;
            }

            return new UdonSymbols
            (
                BaseTys.Values.ToArray(),
                Tys.Values.ToArray(),
                Methods.Values.ToArray(),
                Evs.Values.ToArray()
            );
        }

        private string GetTy(Type type)
        {
            var realName = GetTyRealName(type);

            if (type.IsArray)
            {
                var elemTy = GetTy(type.GetElementType());

                if (!Tys.ContainsKey(realName))
                {
                    var arrayTy = new TySymbol(new string[0], "array", realName, realName, new string[] { elemTy });
                    Tys.Add(realName, arrayTy);
                }
            }
            else if (!Tys.ContainsKey(realName))
            {
                var tyName = GetTySimpleName(type);
                var argTys = type.GenericTypeArguments.Select(x => GetTy(x)).ToArray();
                var scopes = GetQualScopes(type);

                var t = new TySymbol(scopes, tyName, realName, realName, argTys);
                Tys.Add(realName, t);

                if (argTys.Length == 0)
                {
                    if (!BaseTys.ContainsKey(realName))
                    {
                        var baseTy = new BaseTySymbol(scopes, tyName, realName);
                        BaseTys.Add(realName, baseTy);
                    }
                }
                else
                {
                    var genericName = GetQualifiedName(scopes, tyName);

                    if (!BaseTys.ContainsKey(genericName))
                    {
                        var baseTy = new BaseTySymbol(scopes, tyName, genericName);
                        BaseTys.Add(genericName, baseTy);
                    }
                }
            }

            return realName;
        }

        private void AddMethod(MethodSymbol method)
        {
            if (method.real_name.StartsWith("Const_")) return;
            if (method.real_name.StartsWith("Variable_")) return;

            if (method.real_name.StartsWith("Event_"))
            {
                if (!Evs.ContainsKey(method.name))
                {
                    var ev = new EvSymbol(method.name, method.param_tys, method.param_in_outs, method.real_name, method.param_real_names);
                    Evs.Add(ev.name, ev);
                }
                return;
            }
            
            if (method.ty == "SystemVoid") return;

            if (method.real_name.EndsWith("__T")) return;

            var key = GetMethodKey(method);
            if (!Methods.ContainsKey(key))
            {
                Methods.Add(key, method);
            }
        }

        private string GetTyRealName(Type type)
        {
            var genericIndex = type.FullName.IndexOf('`');
            var arrayIndex = type.FullName.IndexOf('[');
            var index = genericIndex == -1 ? arrayIndex : arrayIndex == -1 ? genericIndex : genericIndex < arrayIndex ? genericIndex : arrayIndex;
            var name = index == -1 ? type.FullName : type.FullName.Substring(0, index);
            name = name.Replace(".", "");
            name = name.Replace("+", "");
            name = $"{name}{(type.IsGenericType ? string.Join("", type.GenericTypeArguments.Select(x => GetTyRealName(x))) : "")}";
            name = $"{name}{(type.IsArray ? "Array" : "")}";
            return name;
        }

        private string GetTySimpleName(Type type)
        {
            var genericIndex = type.Name.IndexOf('`');
            var arrayIndex = type.Name.IndexOf('[');
            var index = genericIndex == -1 ? arrayIndex : arrayIndex == -1 ? genericIndex : genericIndex < arrayIndex ? genericIndex : arrayIndex;
            var name = index == -1 ? type.Name : type.Name.Substring(0, index);
            name = $"{name}{(type.IsArray ? "Array" : "")}";
            return name;
        }

        private string[] GetQualScopes(Type type)
        {
            var genericIndex = type.FullName.IndexOf('`');
            var tyName = genericIndex == -1 ? type.FullName : type.FullName.Substring(0, genericIndex);
            var qualNames = tyName.Split(new char[] { '.', '+' });
            return qualNames.Take(qualNames.Length - 1).ToArray();
        }

        private string GetQualifiedName(IEnumerable<string> scopes, string name)
        {
            return string.Join("", scopes.Concat(new string[] { name }));
        }

        private string GetMethodKey(MethodSymbol method)
        {
            return $"{method.ty}{(method.is_static ? "::" : "..")}{method.real_name}";
        }
    }
}
