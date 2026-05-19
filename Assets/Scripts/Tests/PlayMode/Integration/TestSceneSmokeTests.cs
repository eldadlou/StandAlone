using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace MyGame.Tests.Integration
{
    [TestFixture]
    [Category("Integration")]
    public class TestSceneSmokeTests
    {
        [UnityTest]
        public IEnumerator Test_Bootstrap_Scene_Loads()
        {
            if (!Application.CanStreamedLevelBeLoaded("Test_Bootstrap"))
            {
                Assert.Ignore("Add Assets/Scripts/Tests/TestScenes/Test_Bootstrap.unity to Build Settings to run this test.");
                yield break;
            }

            var load = SceneManager.LoadSceneAsync("Test_Bootstrap", LoadSceneMode.Single);

            while (!load.isDone)
                yield return null;

            Assert.AreEqual("Test_Bootstrap", SceneManager.GetActiveScene().name);
        }
    }
}
