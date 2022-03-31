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
        public readonly string binAssetPath;

        public CompileTeuchiUdonScriptTest(string testName)
        {
            this.testName = testName;
            srcAssetPath  = TestUtil.GetSrcAssetPath(testName);
            binAssetPath  = TestUtil.GetBinAssetPath(testName);
        }

        [Test]
        public void TeuchiUdonCodeCompiled()
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
                program = TeuchiUdonCompilerRunner.SaveTeuchiUdonAsset(srcAssetPath, binAssetPath);
            }
            else if (expected == "error")
            {
                Assert.That
                (
                    () => TeuchiUdonCompilerRunner.SaveTeuchiUdonAsset(srcAssetPath, binAssetPath),
                    Throws.TypeOf<TeuchiUdonCompilerException>()
                );
            }
            else
            {
                program = TeuchiUdonCompilerRunner.SaveTeuchiUdonAsset(srcAssetPath, binAssetPath);
            }
        }
    }
}
