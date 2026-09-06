using UnityEngine;

namespace GemGrid.Gameplay
{
    /// <summary>
    /// Guarantees every time the Gameplay scene loads — first time right after Boot,
    /// or a replay reached via Main Menu ▸ Start Game — the player gets a fresh board,
    /// not whatever state <see cref="GameManagerBehaviour"/> happened to already be in
    /// (e.g. still GameOver from a previous session). <see cref="GameManager.RestartGame"/>
    /// is idempotent/safe to call even on a just-booted fresh game.
    ///
    /// NOT verified in the Unity Editor/Play Mode in this environment — see README_M1.md.
    /// </summary>
    public class GameplaySessionStarter : MonoBehaviour
    {
        private void Start()
        {
            var gameManagerBehaviour = GameManagerBehaviour.Instance;
            if (gameManagerBehaviour == null)
                throw new System.InvalidOperationException(
                    $"{nameof(GameplaySessionStarter)} requires the game to be bootstrapped from the Boot scene first " +
                    "(GameManagerBehaviour.Instance is null) — see UNITY_SETUP.md.");

            gameManagerBehaviour.Restart();
        }
    }
}
