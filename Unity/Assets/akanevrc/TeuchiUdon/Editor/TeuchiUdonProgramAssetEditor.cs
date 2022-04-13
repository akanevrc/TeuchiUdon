using UnityEditor;
using VRC.Udon.Editor.ProgramSources;

namespace akanevrc.TeuchiUdon.Editor
{
    [CustomEditor(typeof(TeuchiUdonProgramAsset))]
    public class TeuchiUdonProgramAssetEditor : UdonAssemblyProgramAssetEditor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
        }
    }
}
