using System.Collections;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using VRC.Udon;
using akanevrc.TeuchiUdon.Editor;
using akanevrc.TeuchiUdon.Editor.Compiler;
using akanevrc.TeuchiUdon.Tests.Utils;

namespace akanevrc.TeuchiUdon.Tests
{
    [TestFixtureSource(typeof(TeuchiUdonFixtureData), nameof(TeuchiUdonFixtureData.FixtureParams))]
    public class RunTeuchiUdonAssemblyTest
    {
        public readonly string testName;
        public readonly string srcAddress;
        public readonly string binAddress;
        public GameObject testObject;
        public TeuchiUdonProgramAsset program;
        public TeuchiUdonScript script;

        public RunTeuchiUdonAssemblyTest(string testName)
        {
            this.testName = testName;
            srcAddress    = TestUtil.GetSrcAddress(testName);
            binAddress    = TestUtil.GetBinAddress(testName);
        }

        [TearDown]
        public void TearDown()
        {
            if (testObject != null)
            {
                Object.Destroy(testObject);
            }
            if (program != null)
            {
                TestUtil.ReleaseAsset(program);
                program = null;
            }
            if (script != null)
            {
                TestUtil.ReleaseAsset(script);
                script = null;
            }
        }

        [UnityTest]
        public IEnumerator TeuchiUdonCodeCompiledAndAssemblyRunCorrectly()
        {
            var srcHandle = TestUtil.LoadAsset<TeuchiUdonScript>(srcAddress);
            var binHandle = TestUtil.LoadAsset<TeuchiUdonProgramAsset>(binAddress);
            yield return srcHandle;
            yield return binHandle;
            script  = srcHandle.Result;
            program = binHandle.Result;

            testObject = new GameObject();
            SceneManager.MoveGameObjectToScene(testObject, SceneManager.GetActiveScene());
            var udonBehaviour = testObject.AddComponent<UdonBehaviour>();
            TestUtil.InitUdonBehaviour(udonBehaviour, program.SerializedProgramAsset);

            var text    = script.text;
            var lineEnd = text.IndexOf("\n");
            var line    = lineEnd == -1 ? text : text.Substring(0, lineEnd);

            if (!line.Contains("//")) Assert.Fail("expected value not set");
            var expected = line.Replace("//", "").Trim();

            udonBehaviour.RunProgram("_start");

            if (expected == "")
            {
                Assert.That(testObject, Is.Not.Null);
            }
            else if (expected == "error")
            {
                Assert.That(testObject, Is.Null);
            }
            else if (expected.StartsWith("/") && expected.EndsWith("/"))
            {
                var pattern = new Regex(expected.Substring(1, expected.Length - 2));
                LogAssert.Expect(LogType.Log, pattern);
            }
            else
            {
                LogAssert.Expect(LogType.Log, expected);
            }
        }
    }
}
