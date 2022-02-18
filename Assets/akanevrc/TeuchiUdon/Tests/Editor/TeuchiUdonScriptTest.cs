using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
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
                        .Concat
                        (
                            Directory.GetDirectories(Path.Combine(Application.dataPath, SrcFolderPath))
                            .SelectMany(x =>
                                Directory.GetFiles(Path.Combine(Application.dataPath, SrcFolderPath, Path.GetFileName(x)), "*.teuchi")
                                .Select(y => new TestFixtureData($"{Path.GetFileName(x)}/{Path.GetFileName(y)}"))
                            )
                        )
                        .ToArray();
                }
            }
        }

        private static string SrcFolderPath = "akanevrc/TeuchiUdon/Tests/Editor/src";
        private static string BinSimpleFolderPath = "akanevrc/TeuchiUdon/Tests/Editor/bin/Simple";
        private static string BinBufferedFolderPath = "akanevrc/TeuchiUdon/Tests/Editor/bin/Buffered";
        private static string[] BinFolderPaths = new string[]
        {
            BinSimpleFolderPath,
            BinBufferedFolderPath
        };
        private static string[] SrcFileNames;

        public string srcAssetPath;
        public string[] binAssetPaths;

        public GameObject gameObject;
        public UdonBehaviour udonBehaviour;
        public string expected;

        public bool existingObjectIsActive;

        public TeuchiUdonScriptTest(string srcFileName)
        {
            srcAssetPath = $"Assets/{SrcFolderPath}/{srcFileName}";

            var splitted    = srcFileName.Split(new string[] { "/" }, System.StringSplitOptions.None);
            var binFileName = $"{Path.GetFileNameWithoutExtension(srcFileName)}.asset";;
            if (splitted.Length >= 2)
            {
                var folderPaths = new string[BinFolderPaths.Length];
                for (var i = 0; i <= splitted.Length - 2; i++)
                {
                    var checkedPath  = string.Join("", splitted.Take(i).Select(x => $"/{x}"));
                    folderPaths = BinFolderPaths.Select(x => $"Assets/{x}{checkedPath}").ToArray();
                    foreach (var f in folderPaths)
                    {
                        if (!AssetDatabase.IsValidFolder($"{f}/{splitted[i]}")) AssetDatabase.CreateFolder(f, splitted[i]);
                    }
                }
                binAssetPaths = folderPaths.Select(x => $"{x}/{splitted[splitted.Length - 2]}/{binFileName}").ToArray();
            }
            else
            {
                binAssetPaths = BinFolderPaths.Select(x => $"Assets/{x}/{binFileName}").ToArray();
            }
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
        public IEnumerator UdonAssemblyCompiledSimpleAndRunCorrectly()
        {
            return UdonAssemblyCompiledAndRunCorrectly<TeuchiUdonStrategySimple>();
        }

        [UnityTest]
        public IEnumerator UdonAssemblyCompiledBufferedAndRunCorrectly()
        {
            return UdonAssemblyCompiledAndRunCorrectly<TeuchiUdonStrategyBuffered>();
        }

        private IEnumerator UdonAssemblyCompiledAndRunCorrectly<T>() where T : TeuchiUdonStrategy, new()
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
                TeuchiUdonCompilerRunner.SaveTeuchiUdonAsset<T>(srcAssetPath, GetBinAssetPath<T>());
                yield break;
            }
            else if (expected == "error")
            {
                Assert.That
                (
                    () => TeuchiUdonCompilerRunner.SaveTeuchiUdonAsset<T>(srcAssetPath, GetBinAssetPath<T>()),
                    Throws.InvalidOperationException
                );
                yield break;
            }
            else if (expected.StartsWith("/") && expected.EndsWith("/"))
            {
                var regex = new Regex(expected.Substring(1, expected.Length - 2));
                program = TeuchiUdonCompilerRunner.SaveTeuchiUdonAsset<T>(srcAssetPath, GetBinAssetPath<T>());
            }
            else
            {
                program = TeuchiUdonCompilerRunner.SaveTeuchiUdonAsset<T>(srcAssetPath, GetBinAssetPath<T>());
            }
            udonBehaviour.programSource = program;

            yield return new EnterPlayMode();
            udonBehaviour.RunProgram("_start");
            yield return new ExitPlayMode();

            if (expected.StartsWith("/") && expected.EndsWith("/"))
            {
                var pattern = new Regex(expected.Substring(1, expected.Length - 2));
                LogAssert.Expect(LogType.Log, pattern);
            }
            else
            {
                LogAssert.Expect(LogType.Log, expected);
            }
        }

        private string GetBinAssetPath<T>() where T : TeuchiUdonStrategy
        {
            if (typeof(T) == typeof(TeuchiUdonStrategySimple))
            {
                return binAssetPaths[0];
            }
            else if (typeof(T) == typeof(TeuchiUdonStrategyBuffered))
            {
                return binAssetPaths[1];
            }
            else
            {
                return null;
            }
        }
    }
}
