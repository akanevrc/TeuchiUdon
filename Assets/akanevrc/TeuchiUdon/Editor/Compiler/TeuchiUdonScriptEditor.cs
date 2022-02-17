using UnityEditor;
using UnityEngine;

namespace akanevrc.TeuchiUdon.Editor.Compiler
{
    [CustomEditor(typeof(TeuchiUdonScript))]
    public class TeuchiUdonScriptEditor : UnityEditor.Editor
    {
        [SerializeField, HideInInspector]
        private bool showText = true;

        public override void OnInspectorGUI()
        {
            var script = (TeuchiUdonScript)target;
            EditorGUI.BeginChangeCheck();

            var newShowText = EditorGUILayout.Foldout(showText, "TeuchiUdon Code");

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(this, "Toggle Text Foldout");
                showText = newShowText;
            }

            if (!showText) return;

            EditorGUI.indentLevel++;
            EditorGUI.BeginChangeCheck();

            var oldFontStyle = EditorStyles.label.fontStyle;
            EditorStyles.label.fontStyle = FontStyle.Bold;
            EditorGUILayout.LabelField("TeuchiUdon Code");
            EditorStyles.label.fontStyle = oldFontStyle;

            var newText = EditorGUILayout.TextArea(script.text);

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(this, "Change Text");
                script.text = newText;
            }
            EditorGUI.indentLevel--;
        }
    }
}
