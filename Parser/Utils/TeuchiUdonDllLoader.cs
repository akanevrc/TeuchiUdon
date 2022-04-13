using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace akanevrc.TeuchiUdon
{
    public class TeuchiUdonDllLoader
    {
        public static TeuchiUdonDllLoader Instance { get; } = new TeuchiUdonDllLoader();

        private Dictionary<string, Assembly> Assemblies { get; set; }

        public void Init(IEnumerable<string> dllPaths)
        {
            Assemblies = dllPaths.ToDictionary(x => Path.GetFileName(x), x => Assembly.LoadFrom(x));
        }

        public Type GetTypeFromAssembly(string asmName, string typeName)
        {
            return Assemblies[asmName].GetType(typeName, true);
        }
    }
}
