using System.Collections;
using GemGrid.Configuration;
using GemGrid.Core;
using GemGrid.Gameplay;
using GemGrid.PowerUps;
using UnityEngine;
using UnityEngine.UI;

namespace GemGrid.UI
{
    /// <summary>
    /// Hint/Hammer/Shuffle/Undo buttons, working against whichever GameManager is
    /// currently active (Classic, Journey, or Daily — see GameManagerBehaviour.ReplaceGame)
    /// so power-ups are available in every mode, not just Journey. Legality (free-use-
    /// per-level vs. owned inventory vs. nothing owned) is delegated entirely to
    /// PowerUpLevelUsage/PowerUpInventory — this component never decides on its own
    /// whether a use is allowed.
    ///
    /// Hammer targets the first occupied cell it finds (a simple, deterministic choice)
    /// rather than an interactive tap-to-target flow — a scope trim for this milestone,
    /// noted in the implementation report; HammerAction itself supports targeting any
    /// cell, so wiring real cell-picking later is a small, isolated UI change.
    ///
    /// NOT verified in the Unity Editor/Play Mode in this environment — see UNITY_SETUP.md.
    /// </summary>
    public class PowerUpBar : MonoBehaviour
    {
        [SerializeField] private PlacementPreviewController placementPreview;
        [SerializeField] private Button hintButton;
        [SerializeField] private Button hammerButton;
        [SerializeField] private Button shuffleButton;
        [SerializeField] private Button undoButton;

        private const float HintFlashSeconds = 1f;

        private void Awake()
        {
            if (hintButton != null) hintButton.onClick.AddListener(OnHintClicked);
            if (hammerButton != null) hammerButton.onClick.AddListener(OnHammerClicked);
            if (shuffleButton != null) shuffleButton.onClick.AddListener(OnShuffleClicked);
            if (undoButton != null) undoButton.onClick.AddListener(OnUndoClicked);
        }

        private void OnHintClicked()
        {
            var game = GameManagerBehaviour.Instance.Game;

            for (int i = 0; i < game.Spawner.TraySize; i++)
            {
                var block = game.Spawner.GetSlot(i);
                if (block.IsEmpty) continue;

                var cell = HintFinder.FindValidPlacement(game.Grid, block.Shape);
                if (!cell.HasValue) continue;

                if (!CanUsePowerUp(PowerUpId.Hint)) return;
                ConsumePowerUp(PowerUpId.Hint);
                if (placementPreview != null) StartCoroutine(FlashHint(block.Shape, cell.Value));
                return;
            }
        }

        private IEnumerator FlashHint(BlockShapeDefinition shape, Int2 cell)
        {
            placementPreview.ShowPreview(shape, cell);
            float t = 0f;
            while (t < HintFlashSeconds)
            {
                t += Time.deltaTime;
                yield return null;
            }
            placementPreview.HidePreview();
        }

        private void OnHammerClicked()
        {
            if (!CanUsePowerUp(PowerUpId.Hammer)) return;

            var game = GameManagerBehaviour.Instance.Game;
            var target = FindFirstOccupiedCell(game.Grid);
            if (!target.HasValue) return; // nothing to clear.

            if (!ConsumePowerUp(PowerUpId.Hammer)) return;
            HammerAction.Apply(game, target.Value);
        }

        private void OnShuffleClicked()
        {
            if (!CanUsePowerUp(PowerUpId.Shuffle)) return;
            if (!ConsumePowerUp(PowerUpId.Shuffle)) return;
            ShuffleAction.Apply(GameManagerBehaviour.Instance.Game);
        }

        private void OnUndoClicked()
        {
            var game = GameManagerBehaviour.Instance.Game;
            if (!game.CanUndo) return; // legality: nothing to undo, regardless of power-up availability.
            if (!CanUsePowerUp(PowerUpId.Undo)) return;
            if (!ConsumePowerUp(PowerUpId.Undo)) return;
            UndoAction.Apply(game);
        }

        private static Int2? FindFirstOccupiedCell(IGridModel grid)
        {
            for (int y = 0; y < grid.Height; y++)
            {
                for (int x = 0; x < grid.Width; x++)
                {
                    var cell = new Int2(x, y);
                    if (grid.IsCellOccupied(cell)) return cell;
                }
            }
            return null;
        }

        private static bool CanUsePowerUp(PowerUpId id)
        {
            var meta = MetaProgressionService.Instance;
            return meta != null && meta.PowerUpLevelUsage.CanUse(id, meta.PowerUpInventory);
        }

        private static bool ConsumePowerUp(PowerUpId id)
        {
            var meta = MetaProgressionService.Instance;
            if (meta == null) return false;
            bool consumed = meta.PowerUpLevelUsage.Consume(id, meta.PowerUpInventory);
            if (consumed) meta.Save();
            return consumed;
        }
    }
}
