using System.IO;
using UnityEditor;
using UnityEngine;

namespace akanevrc.TeuchiUdon.Editor.Compiler
{
    public class TeuchiUdonScript : ScriptableObject
    {
        public string text;

        public void Compile(string path)
        {
            var scriptPath = AssetDatabase.GetAssetPath(this);

            if (string.IsNullOrEmpty(scriptPath) || string.IsNullOrEmpty(path))
            {
                throw new FileNotFoundException("Asset file not found");
            }

            TeuchiUdonCompilerRunner.SaveTeuchiUdonAsset(scriptPath, path);
            Debug.Log("Compile succeeded");
        }
    }
}
