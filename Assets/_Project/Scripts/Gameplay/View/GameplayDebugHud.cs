using GemGrid.Core;
using UnityEngine;

namespace GemGrid.Gameplay
{
    /// <summary>
    /// Bare IMGUI (<c>OnGUI</c>) overlay showing Score/Combo/State and a Restart
    /// button, so this M1.5 playable build is actually observable and testable by a
    /// human without attaching a debugger. Deliberately NOT Canvas/UGUI — that is real
    /// UI and belongs to M2; IMGUI here is a dev/debug overlay only.
    ///
    /// NOT verified in the Unity Editor/Play Mode in this environment — see README_M1.md.
    /// </summary>
    public class GameplayDebugHud : MonoBehaviour
    {
        private GameManagerBehaviour _gameManagerBehaviour;

        private void Awake()
        {
            _gameManagerBehaviour = GameManagerBehaviour.Instance;
        }

        private void OnGUI()
        {
            if (_gameManagerBehaviour == null || _gameManagerBehaviour.Game == null) return;
            var game = _gameManagerBehaviour.Game;

            GUI.Label(new Rect(10, 10, 320, 24), $"Score: {game.Score.TotalScore}");
            GUI.Label(new Rect(10, 34, 320, 24), $"Combo: {game.Combo.CurrentCombo} (best {game.Combo.BestCombo})");
            GUI.Label(new Rect(10, 58, 320, 24), $"State: {game.State}");

            if (game.State == GameStateType.GameOver)
            {
                GUI.Label(new Rect(10, 82, 320, 24), "GAME OVER");
                if (GUI.Button(new Rect(10, 106, 120, 30), "Restart"))
                    _gameManagerBehaviour.Restart();
            }
        }
    }
}
