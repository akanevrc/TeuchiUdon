using System.IO;
using UnityEditor.Experimental.AssetImporters;
using UnityEngine;

namespace akanevrc.TeuchiUdon.Editor.Compiler
{
    [ScriptedImporter(1, "teuchi")]
    public class TeuchiUdonScriptImporter : ScriptedImporter
    {
        public override void OnImportAsset(AssetImportContext ctx)
        {
            var script = ScriptableObject.CreateInstance<TeuchiUdonScript>();
            script.text = File.ReadAllText(ctx.assetPath);
            ctx.AddObjectToAsset("TeuchiUdonScript", script);
            ctx.SetMainObject(script);
        }
    }
}
