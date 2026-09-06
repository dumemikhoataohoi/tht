using System.Collections;
using GemGrid.Core;
using GemGrid.Gameplay;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace GemGrid.Tests.PlayMode
{
    /// <summary>
    /// NOT VERIFIED in this environment — there is no Unity Editor/Player available to
    /// actually run these tests. They are written against my best understanding of the
    /// Unity Test Framework's PlayMode APIs and the Boot ▸ Gameplay flow this milestone
    /// introduces, but have never been executed. See README_M1.md / UNITY_SETUP.md.
    ///
    /// Requires Boot.unity and Gameplay.unity to exist (GemGrid ▸ Setup ▸ 5. Create Boot
    /// And Gameplay Scenes) and both to be added to
    /// File ▸ Build Settings ▸ Scenes In Build (Boot first) so SceneManager can load
    /// them by name from a test.
    ///
    /// A dedicated "natural Game Over via Boot" PlayMode test is intentionally NOT
    /// included: reaching Game Over organically depends on which shapes the real
    /// BlockShapeSet asset happens to spawn (random, weighted), which would make such a
    /// test flaky in a way I have no way to tune or verify without a running Editor.
    /// Game Over detection itself is already covered by 6 EditMode tests in
    /// GameManagerFlowTests.cs, which run against the exact same GameManager class.
    /// </summary>
    public class BootFlowTests
    {
        [UnityTearDown]
        public IEnumerator TearDown()
        {
            // The Bootstrap GameObject uses DontDestroyOnLoad, so it survives scene
            // loads within a test but must be torn down explicitly between tests to
            // avoid leaking state/duplicate singletons into the next one.
            if (GameManagerBehaviour.Instance != null)
                Object.Destroy(GameManagerBehaviour.Instance.gameObject);
            yield return null;
        }

        [UnityTest]
        public IEnumerator Boot_LoadsGameplayScene_AndInitializesGameManagerIntoPlayingState()
        {
            SceneManager.LoadScene("Boot", LoadSceneMode.Single);
            yield return null; // Boot scene objects' Awake/Start
            yield return null; // BootLoader.Start() triggers LoadScene("Gameplay")
            yield return null; // Gameplay scene objects' Awake/Start

            Assert.IsNotNull(GameManagerBehaviour.Instance,
                "Boot should create a persistent GameManagerBehaviour (DontDestroyOnLoad).");
            Assert.AreEqual(GameStateType.Playing, GameManagerBehaviour.Instance.Game.State);
            Assert.AreEqual("Gameplay", SceneManager.GetActiveScene().name);
        }

        [UnityTest]
        public IEnumerator Restart_ResetsScoreAndGrid_WhileStayingInPlayingState()
        {
            SceneManager.LoadScene("Boot", LoadSceneMode.Single);
            yield return null;
            yield return null;
            yield return null;

            var game = GameManagerBehaviour.Instance.Game;
            Assume.That(game.Spawner.GetSlot(0).IsEmpty, Is.False, "Boot should have refilled the tray already.");

            game.TryPlaceBlock(0, new Int2(0, 0));
            Assert.Greater(game.Score.TotalScore, 0, "Placing a block should have scored at least the placement points.");

            GameManagerBehaviour.Instance.Restart();

            Assert.AreEqual(0, game.Score.TotalScore);
            Assert.AreEqual(GameStateType.Playing, game.State);
            Assert.IsFalse(game.Grid.IsCellOccupied(new Int2(0, 0)));
        }
    }
}
