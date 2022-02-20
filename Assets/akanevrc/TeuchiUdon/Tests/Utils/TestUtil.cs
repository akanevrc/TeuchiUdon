using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using VRC.Udon;
using VRC.Udon.Common;
using akanevrc.TeuchiUdon.Editor.Compiler;

namespace akanevrc.TeuchiUdon.Tests.Utils
{
    public static class TestUtil
    {
        public static readonly string srcFolderPath = "akanevrc/TeuchiUdon/Tests/src";
        public static readonly string binFolderPath = "akanevrc/TeuchiUdon/Tests/bin";
        public static readonly string srcAddress = "Tests/src";
        public static readonly string binAddress = "Tests/bin";
        public static readonly string simpleStrategyFolderName = "Simple";
        public static readonly string bufferedStrategyFolderName = "Buffered";
        public static readonly string[] strategyFolderNames = new string[]
        {
            simpleStrategyFolderName,
            bufferedStrategyFolderName
        };

        public static string GetSrcAssetPath(string testName)
        {
            return $"Assets/{srcFolderPath}/{testName}.teuchi";
        }

        public static IEnumerable<string> GetBinAssetPaths(string testName)
        {
            return GetAssetPaths(testName, "Assets/", ".asset", binFolderPath);
        }

        public static string GetSrcAddress(string testName)
        {
            return $"{srcAddress}/{testName}.teuchi";
        }

        public static IEnumerable<string> GetBinAddresses(string testName)
        {
            return GetAssetPaths(testName, "", ".asset", binAddress);
        }

        private static IEnumerable<string> GetAssetPaths(string testName, string prefix, string ext, string folderPath)
        {
            var splitted = testName.Split(new string[] { "/" }, System.StringSplitOptions.None);
            var fileName = $"{Path.GetFileName(testName)}{ext}";
            if (splitted.Length >= 2)
            {
                var folderPaths = new string[strategyFolderNames.Length];
                for (var i = 0; i <= splitted.Length - 2; i++)
                {
                    var checkedPath = string.Join("", splitted.Take(i).Select(x => $"/{x}"));
                    folderPaths     = strategyFolderNames.Select(x => $"{prefix}{folderPath}/{x}{checkedPath}").ToArray();
                    foreach (var f in folderPaths)
                    {
                        if (!AssetDatabase.IsValidFolder($"{f}/{splitted[i]}")) AssetDatabase.CreateFolder(f, splitted[i]);
                    }
                }
                return folderPaths.Select(x => $"{x}/{splitted[splitted.Length - 2]}/{fileName}");
            }
            else
            {
                return strategyFolderNames.Select(x => $"{prefix}{folderPath}/{x}/{fileName}");
            }
        }

        public static string GetAssetPath<T>(string[] assetPaths) where T : TeuchiUdonStrategy
        {
            if (typeof(T) == typeof(TeuchiUdonStrategySimple))
            {
                return assetPaths[0];
            }
            else if (typeof(T) == typeof(TeuchiUdonStrategyBuffered))
            {
                return assetPaths[1];
            }
            else
            {
                return null;
            }
        }

        public static AsyncOperationHandle<T> LoadAsset<T>(string address)
        {
            return Addressables.LoadAssetAsync<T>(address);
        }

        public static void ReleaseAsset<T>(T asset)
        {
            Addressables.Release(asset);
        }

        public static void InitUdonBehaviour(UdonBehaviour udonBehaviour, AbstractSerializedUdonProgramAsset serializedProgram)
        {
            udonBehaviour.AssignProgramAndVariables(serializedProgram, new UdonVariableTable());
            udonBehaviour.InitializeUdonContent();
        }
    }

    public static class TeuchiUdonFixtureData
    {
        public static IEnumerable<TestFixtureData> FixtureParams
        {
            get
            {
                return
                    Directory.GetFiles(Path.Combine(Application.dataPath, TestUtil.srcFolderPath), "*.teuchi")
                        .Select(x => new TestFixtureData(Path.GetFileNameWithoutExtension(x)))
                    .Concat
                    (
                        Directory.GetDirectories(Path.Combine(Application.dataPath, TestUtil.srcFolderPath))
                        .SelectMany(x =>
                            Directory.GetFiles(Path.Combine(Application.dataPath, TestUtil.srcFolderPath, Path.GetFileName(x)), "*.teuchi")
                            .Select(y => new TestFixtureData($"{Path.GetFileName(x)}/{Path.GetFileNameWithoutExtension(y)}"))
                        )
                    )
                    .ToArray();
            }
        }
    }
}
