using System.Linq;
using NUnit.Framework;
using UnityEditor;
using akanevrc.TeuchiUdon.Editor;
using akanevrc.TeuchiUdon.Editor.Compiler;
using akanevrc.TeuchiUdon.Tests.Utils;

namespace akanevrc.TeuchiUdon.Tests.Editor
{
    [TestFixtureSource(typeof(TeuchiUdonFixtureData), nameof(TeuchiUdonFixtureData.FixtureParams))]
    public class CompileTeuchiUdonScriptTest
    {
        public readonly string testName;
        public readonly string srcAssetPath;
        public readonly string[] binAssetPaths;

        public CompileTeuchiUdonScriptTest(string testName)
        {
            this.testName = testName;
            srcAssetPath  = TestUtil.GetSrcAssetPath (testName);
            binAssetPaths = TestUtil.GetBinAssetPaths(testName).ToArray();
        }

        [Test]
        public void TeuchiUdonCodeCompiledSimple()
        {
            TeuchiUdonCodeCompiled<TeuchiUdonStrategySimple>();
        }

        [Test]
        public void TeuchiUdonCodeCompiledBuffered()
        {
            TeuchiUdonCodeCompiled<TeuchiUdonStrategyBuffered>();
        }

        private void TeuchiUdonCodeCompiled<T>() where T : TeuchiUdonStrategy, new()
        {
            var script  = AssetDatabase.LoadAssetAtPath<TeuchiUdonScript>(srcAssetPath);
            var text    = script.text;
            var lineEnd = text.IndexOf("\n");
            var line    = lineEnd == -1 ? text : text.Substring(0, lineEnd);

            if (!line.Contains("//")) Assert.Fail("expected value not set");
            var expected = line.Replace("//", "").Trim();

            var program = (TeuchiUdonProgramAsset)null;
            if (expected == "")
            {
                program = TeuchiUdonCompilerRunner.SaveTeuchiUdonAsset<T>(srcAssetPath, TestUtil.GetAssetPath<T>(binAssetPaths));
            }
            else if (expected == "error")
            {
                Assert.That
                (
                    () => TeuchiUdonCompilerRunner.SaveTeuchiUdonAsset<T>(srcAssetPath, TestUtil.GetAssetPath<T>(binAssetPaths)),
                    Throws.InvalidOperationException
                );
            }
            else
            {
                program = TeuchiUdonCompilerRunner.SaveTeuchiUdonAsset<T>(srcAssetPath, TestUtil.GetAssetPath<T>(binAssetPaths));
            }
        }
    }
}
