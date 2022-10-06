using System.IO;
using UnityEditor.Experimental.AssetImporters;
using UnityEditor;
using UnityEngine;

namespace akanevrc.TeuchiUdon.Editor.Compiler
{
    [CustomEditor(typeof(TeuchiUdonScriptImporter))]
    public class TeuchiUdonScriptImporterEditor : ScriptedImporterEditor
    {
        public override void OnInspectorGUI()
        {
            var oldFontStyle = EditorStyles.label.fontStyle;
            EditorStyles.label.fontStyle = FontStyle.Bold;
            EditorGUILayout.LabelField("TeuchiUdon Script Import Settings");
            EditorStyles.label.fontStyle = oldFontStyle;

            EditorGUILayout.Space();

            if (GUILayout.Button("Compile TeuchiUdon Script"))
            {
                var scriptPath = AssetDatabase.GetAssetPath(assetTarget);
                var assetPath  = (string)null;
                if (string.IsNullOrEmpty(scriptPath))
                {
                    assetPath = EditorUtility.SaveFilePanelInProject
                    (
                        "Compile TeuchiUdon Script",
                        assetTarget.name,
                        "asset",
                        "Save Udon Assembly Asset"
                    );
                }
                else
                {
                    assetPath = EditorUtility.SaveFilePanelInProject
                    (
                        "Compile TeuchiUdon Script",
                        assetTarget.name,
                        "asset",
                        "Save Udon Assembly Asset",
                        Path.GetDirectoryName(scriptPath)
                    );
                }

                if (!string.IsNullOrEmpty(assetPath))
                {
                    ((TeuchiUdonScript)assetTarget).Compile(assetPath);
                }
            }

            base.ApplyRevertGUI();
        }
    }
}
