using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace akanevrc.TeuchiUdon
{
    public class TeuchiUdonDllLoader
    {
        private string[] DllPaths { get; }
        private Dictionary<string, Assembly> Assemblies { get; set; }

        private bool IsInitialized { get; set; } = false;

        public TeuchiUdonDllLoader(IEnumerable<string> dllPaths)
        {
            DllPaths = dllPaths.ToArray();
        }

        public void Init()
        {
            if (!IsInitialized)
            {
                Assemblies = DllPaths.ToDictionary(x => Path.GetFileName(x), x => Assembly.LoadFrom(x));
                IsInitialized = true;
            }
        }

        public Type GetTypeFromAssembly(string asmName, string typeName)
        {
            return Assemblies[asmName].GetType(typeName, true);
        }
    }
}
