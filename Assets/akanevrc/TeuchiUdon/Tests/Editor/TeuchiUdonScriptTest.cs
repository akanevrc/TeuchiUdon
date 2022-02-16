using System.Collections;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using VRC.Udon;
using akanevrc.TeuchiUdon.Editor.Compiler;

namespace Tests
{
    public class TeuchiUdonScriptTest
    {
        private static string SrcFolderPath = "akanevrc/TeuchiUdon/Tests/Editor/src";
        private static string BinFolderPath = "akanevrc/TeuchiUdon/Tests/Editor/bin";
        private static string[] SrcPaths;

        static TeuchiUdonScriptTest()
        {
            SrcPaths =
                Directory.GetFiles(Path.Combine(Application.dataPath, SrcFolderPath), "*.teuchi")
                .Select(x => Path.GetFileName(x))
                .ToArray();
        }

        public GameObject GameObject;
        public UdonBehaviour UdonBehaviour;

        [UnityTest]
        public IEnumerator UdonAssemblyRunCorrectly([ValueSource("SrcPaths")] string srcPath)
        {
            GameObject    = new GameObject();
            UdonBehaviour = GameObject.AddComponent<UdonBehaviour>();

            var binPath = $"{Path.GetFileNameWithoutExtension(srcPath)}.asset";
            var program = TeuchiUdonCompilerRunner.SaveTeuchiUdonAsset<TeuchiUdonStrategySimple>
            (
                $"Assets/{SrcFolderPath}/{srcPath}",
                $"Assets/{BinFolderPath}/{binPath}"
            );
            UdonBehaviour.programSource = program;

            yield return new EnterPlayMode();
            UdonBehaviour.RunProgram("_start");
            yield return new ExitPlayMode();

            var script   = AssetDatabase.LoadAssetAtPath<TeuchiUdonScript>($"Assets/{SrcFolderPath}/{srcPath}");
            var text     = script.text;
            var lineEnd  = text.IndexOf("\n");
            var line     = lineEnd == -1 ? text : text.Substring(0, lineEnd);
            var expected = line.Trim().Replace("//", "").Trim();

            LogAssert.Expect(LogType.Log, expected);
        }
    }
}
