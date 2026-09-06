using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace GemGrid.Gameplay
{
    /// <summary>
    /// Top HUD for the Gameplay scene: current score, best score, and a combo badge
    /// that pulses when the combo increases. Real UGUI (Canvas), not IMGUI — this
    /// replaces the M1.5 debug overlay. Built by
    /// <see cref="GemGrid.EditorTools.GemGridSceneSetup"/>; placeholder colors/layout
    /// only, real art is a later polish pass.
    ///
    /// NOT verified in the Unity Editor/Play Mode in this environment — see README_M1.md.
    /// </summary>
    public class GameplayHud : MonoBehaviour
    {
        [SerializeField] private Text scoreText;
        [SerializeField] private Text bestScoreText;
        [SerializeField] private Text comboText;
        [SerializeField] private RectTransform comboBadge;

        private GameManagerBehaviour _gameManagerBehaviour;
        private int _bestScore;

        private void Awake()
        {
            _gameManagerBehaviour = GameManagerBehaviour.Instance;
            if (_gameManagerBehaviour == null)
                throw new System.InvalidOperationException(
                    $"{nameof(GameplayHud)} requires the game to be bootstrapped from the Boot scene first " +
                    "(GameManagerBehaviour.Instance is null) — see UNITY_SETUP.md.");
        }

        private void Start()
        {
            var game = _gameManagerBehaviour.Game;
            game.Score.ScoreChanged += OnScoreChanged;
            game.Combo.ComboChanged += OnComboChanged;
            game.GameRestarted += OnRestarted;

            _bestScore = BestScoreStore.Get();
            RefreshAll();
        }

        private void OnDestroy()
        {
            var game = _gameManagerBehaviour != null ? _gameManagerBehaviour.Game : null;
            if (game == null) return;
            game.Score.ScoreChanged -= OnScoreChanged;
            game.Combo.ComboChanged -= OnComboChanged;
            game.GameRestarted -= OnRestarted;
        }

        private void OnScoreChanged(int total)
        {
            if (scoreText != null)
            {
                scoreText.text = $"Score {total}";
                StartCoroutine(PulseTransform(scoreText.rectTransform));
            }

            if (total > _bestScore)
            {
                _bestScore = total;
                if (bestScoreText != null) bestScoreText.text = $"Best {_bestScore}";
            }
        }

        private void OnComboChanged(int combo)
        {
            if (comboText != null)
                comboText.text = combo > 0 ? $"Combo x{combo}" : string.Empty;

            if (combo > 1 && comboBadge != null)
                StartCoroutine(PulseTransform(comboBadge));
        }

        private void OnRestarted()
        {
            _bestScore = BestScoreStore.Get();
            RefreshAll();
        }

        private void RefreshAll()
        {
            var game = _gameManagerBehaviour.Game;
            if (scoreText != null) scoreText.text = $"Score {game.Score.TotalScore}";
            if (bestScoreText != null) bestScoreText.text = $"Best {_bestScore}";
            if (comboText != null) comboText.text = string.Empty;
        }

        /// <summary>Small scale "pop" used for both the score text and the combo badge on change.</summary>
        private static IEnumerator PulseTransform(RectTransform rect)
        {
            Vector3 baseScale = Vector3.one;
            const float duration = 0.15f;
            float t = 0f;
            while (t < duration)
            {
                t += Time.deltaTime;
                if (rect == null) yield break;
                float k = Mathf.Lerp(1.25f, 1f, t / duration);
                rect.localScale = baseScale * k;
                yield return null;
            }
            if (rect != null) rect.localScale = baseScale;
        }
    }
}
