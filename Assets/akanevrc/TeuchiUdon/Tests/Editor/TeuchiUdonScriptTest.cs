using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using VRC.Udon;
using akanevrc.TeuchiUdon.Editor;
using akanevrc.TeuchiUdon.Editor.Compiler;

namespace akanevrc.TeuchiUdon.Tests.Editor
{
    [TestFixtureSource(typeof(FixtureData), nameof(FixtureData.FixtureParams))]
    public class TeuchiUdonScriptTest
    {
        public class FixtureData
        {
            public static IEnumerable<TestFixtureData> FixtureParams
            {
                get
                {
                    return
                        Directory.GetFiles(Path.Combine(Application.dataPath, SrcFolderPath), "*.teuchi")
                        .Select(x => new TestFixtureData(Path.GetFileName(x)))
                        .ToArray();
                }
            }
        }

        private static string SrcFolderPath = "akanevrc/TeuchiUdon/Tests/Editor/src";
        private static string BinFolderPath = "akanevrc/TeuchiUdon/Tests/Editor/bin";
        private static string[] SrcFileNames;

        public string srcAssetPath;
        public string binAssetPath;

        public GameObject gameObject;
        public UdonBehaviour udonBehaviour;
        public string expected;

        public bool existingObjectIsActive;

        public TeuchiUdonScriptTest(string srcFileName)
        {
            var binFileName = $"{Path.GetFileNameWithoutExtension(srcFileName)}.asset";
            srcAssetPath    = $"Assets/{SrcFolderPath}/{srcFileName}";
            binAssetPath    = $"Assets/{BinFolderPath}/{binFileName}";
        }

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            var existingObject     = GameObject.Find("TeuchiUdon");
            existingObjectIsActive = existingObject != null && existingObject.activeSelf;
            existingObject?.SetActive(false);

            new GameObject("__Test__").AddComponent<UdonBehaviour>();
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            GameObject.Find("TeuchiUdon")?.SetActive(existingObjectIsActive);
            Object.DestroyImmediate(GameObject.Find("__Test__"));
        }

        [UnityTest]
        public IEnumerator UdonAssemblyRunCorrectly()
        {
            gameObject    = GameObject.Find("__Test__");
            udonBehaviour = gameObject.GetComponent<UdonBehaviour>();

            var script  = AssetDatabase.LoadAssetAtPath<TeuchiUdonScript>(srcAssetPath);
            var text    = script.text;
            var lineEnd = text.IndexOf("\n");
            var line    = lineEnd == -1 ? text : text.Substring(0, lineEnd);

            if (!line.Contains("//")) Assert.Fail("expected value not set");
            expected = line.Replace("//", "").Trim();

            var program = (TeuchiUdonProgramAsset)null;
            if (expected == "")
            {
                TeuchiUdonCompilerRunner.SaveTeuchiUdonAsset<TeuchiUdonStrategySimple>(srcAssetPath, binAssetPath);
                yield break;
            }
            else if (expected == "error")
            {
                Assert.That
                (
                    () => TeuchiUdonCompilerRunner.SaveTeuchiUdonAsset<TeuchiUdonStrategySimple>(srcAssetPath, binAssetPath),
                    Throws.InvalidOperationException
                );
                yield break;
            }
            else
            {
                program = TeuchiUdonCompilerRunner.SaveTeuchiUdonAsset<TeuchiUdonStrategySimple>(srcAssetPath, binAssetPath);
            }
            udonBehaviour.programSource = program;

            yield return new EnterPlayMode();
            udonBehaviour.RunProgram("_start");
            yield return new ExitPlayMode();

            LogAssert.Expect(LogType.Log, expected);
        }
    }
}
