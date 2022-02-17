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

        public string SrcAssetPath { get; }
        public string BinAssetPath { get; }

        public GameObject GameObject;
        public UdonBehaviour UdonBehaviour;
        public string Expected;

        public GameObject ExistingObject;
        public bool ExistingObjectIsActive;

        public TeuchiUdonScriptTest()
        {
        }

        public TeuchiUdonScriptTest(string srcFileName)
        {
            var binFileName = $"{Path.GetFileNameWithoutExtension(srcFileName)}.asset";
            SrcAssetPath    = $"Assets/{SrcFolderPath}/{srcFileName}";
            BinAssetPath    = $"Assets/{BinFolderPath}/{binFileName}";
        }

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            ExistingObject         = GameObject.Find("TeuchiUdon");
            ExistingObjectIsActive = ExistingObject != null && ExistingObject.activeSelf;
            ExistingObject?.SetActive(false);
        }

        [TearDown]
        public void OneTimeTearDown()
        {
            ExistingObject?.SetActive(ExistingObjectIsActive);
        }

        [UnityTest]
        public IEnumerator UdonAssemblyRunCorrectly()
        {
            GameObject    = new GameObject();
            UdonBehaviour = GameObject.AddComponent<UdonBehaviour>();

            var script  = AssetDatabase.LoadAssetAtPath<TeuchiUdonScript>(SrcAssetPath);
            var text    = script.text;
            var lineEnd = text.IndexOf("\n");
            var line    = lineEnd == -1 ? text : text.Substring(0, lineEnd);

            if (!line.Contains("//")) Assert.Fail("expected value not set");
            Expected = line.Replace("//", "").Trim();

            var program = (TeuchiUdonProgramAsset)null;
            if (Expected == "")
            {
                TeuchiUdonCompilerRunner.SaveTeuchiUdonAsset<TeuchiUdonStrategySimple>(SrcAssetPath, BinAssetPath);
                yield break;
            }
            else if (Expected == "error")
            {
                Assert.That
                (
                    () => TeuchiUdonCompilerRunner.SaveTeuchiUdonAsset<TeuchiUdonStrategySimple>(SrcAssetPath, BinAssetPath),
                    Throws.InvalidOperationException
                );
                yield break;
            }
            else
            {
                program = TeuchiUdonCompilerRunner.SaveTeuchiUdonAsset<TeuchiUdonStrategySimple>(SrcAssetPath, BinAssetPath);
            }
            UdonBehaviour.programSource = program;

            yield return new EnterPlayMode();
            UdonBehaviour.RunProgram("_start");
            yield return new ExitPlayMode();

            LogAssert.Expect(LogType.Log, Expected);
        }
    }
}
