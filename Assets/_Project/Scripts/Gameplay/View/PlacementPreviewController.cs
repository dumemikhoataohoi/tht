using System.Collections.Generic;
using GemGrid.Configuration;
using GemGrid.Core;
using UnityEngine;

namespace GemGrid.Gameplay
{
    /// <summary>
    /// Ghost/preview overlay shown while dragging a block: highlights the exact cells the
    /// block would occupy if dropped at the current pointer position, colored green when
    /// the placement is legal and red when it isn't. Purely a view concern — it only
    /// queries <see cref="IGridModel.CanPlace"/> and <see cref="BlockPlacement.GetOccupiedCells"/>
    /// (both pre-existing, non-mutating), never changes the grid/score/combo logic.
    ///
    /// NOT verified in the Unity Editor/Play Mode in this environment — see UNITY_SETUP.md.
    /// </summary>
    public class PlacementPreviewController : MonoBehaviour
    {
        [SerializeField] private GridController gridController;
        [SerializeField] private Color validColor = new Color(0.35f, 0.95f, 0.55f, 0.55f);
        [SerializeField] private Color invalidColor = new Color(0.95f, 0.30f, 0.30f, 0.55f);

        private GameManagerBehaviour _gameManagerBehaviour;
        private readonly List<SpriteRenderer> _pool = new List<SpriteRenderer>();

        private void Awake()
        {
            _gameManagerBehaviour = GameManagerBehaviour.Instance;
            if (_gameManagerBehaviour == null)
                throw new System.InvalidOperationException(
                    $"{nameof(PlacementPreviewController)} requires the game to be bootstrapped from the Boot scene first " +
                    "(GameManagerBehaviour.Instance is null) — see UNITY_SETUP.md.");
        }

        /// <summary>Shows the preview for <paramref name="shape"/> anchored at <paramref name="origin"/>.</summary>
        public void ShowPreview(BlockShapeDefinition shape, Int2 origin)
        {
            if (shape == null)
            {
                HidePreview();
                return;
            }

            bool valid = _gameManagerBehaviour.Game.Grid.CanPlace(shape, origin);
            Color color = valid ? validColor : invalidColor;

            int index = 0;
            foreach (var cell in BlockPlacement.GetOccupiedCells(shape, origin))
            {
                var cellRenderer = GetOrCreatePreviewCell(index);
                cellRenderer.transform.position = gridController.GridToWorld(cell);
                cellRenderer.color = color;
                cellRenderer.gameObject.SetActive(true);
                index++;
            }

            for (int i = index; i < _pool.Count; i++)
                _pool[i].gameObject.SetActive(false);
        }

        public void HidePreview()
        {
            foreach (var cellRenderer in _pool)
                if (cellRenderer != null) cellRenderer.gameObject.SetActive(false);
        }

        private SpriteRenderer GetOrCreatePreviewCell(int index)
        {
            if (index < _pool.Count) return _pool[index];

            var go = new GameObject($"PreviewCell_{index}");
            go.transform.SetParent(transform, false);
            go.transform.localScale = Vector3.one * (gridController.CellSize * 0.9f);

            var cellRenderer = go.AddComponent<SpriteRenderer>();
            cellRenderer.sprite = PlaceholderSprite.White;
            cellRenderer.sortingOrder = 2; // above cells (1) and board backdrop (0).
            _pool.Add(cellRenderer);
            return cellRenderer;
        }
    }
}
