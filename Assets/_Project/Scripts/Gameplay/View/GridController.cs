using System.Collections;
using GemGrid.Core;
using UnityEngine;

namespace GemGrid.Gameplay
{
    /// <summary>
    /// Minimal placeholder visual for the grid: converts between world space and grid
    /// coordinates, and renders one sprite per cell so state changes are visible.
    /// Falls back to an auto-generated white square (<see cref="PlaceholderSprite"/>)
    /// if no sprite is assigned in the Inspector, so the grid is visible on Play with
    /// zero manual setup. Plays a small "pop" when a cell becomes newly occupied and a
    /// "flash" when a cell is cleared, detected by diffing occupancy against the
    /// previous frame's state — no changes to the tested Gameplay logic layer needed.
    /// Real art/polish belongs to a later pass; this is placeholder-quality per M2 scope.
    ///
    /// NOT verified in the Unity Editor/Play Mode in this environment — see README_M1.md.
    /// </summary>
    public class GridController : MonoBehaviour
    {
        [SerializeField] private float cellSize = 1f;
        [SerializeField] private Sprite cellSprite;
        [SerializeField] private Color emptyColor = GemPalette.CellEmpty;
        [SerializeField] private Color occupiedColor = GemPalette.CellFilledFallback;
        [SerializeField] private Color clearFlashColor = GemPalette.ClearFlash;
        [SerializeField] private Color boardBackdropColor = GemPalette.Surface;

        private GameManagerBehaviour _gameManagerBehaviour;
        private SpriteRenderer[,] _cellViews;
        private bool[,] _wasOccupied;

        private void Awake()
        {
            _gameManagerBehaviour = GameManagerBehaviour.Instance;
            if (_gameManagerBehaviour == null)
                throw new System.InvalidOperationException(
                    $"{nameof(GridController)} requires the game to be bootstrapped from the Boot scene first " +
                    "(GameManagerBehaviour.Instance is null) — see UNITY_SETUP.md.");
        }

        private void Start()
        {
            BuildCellViews();
            _gameManagerBehaviour.Game.Grid.GridChanged += RefreshAllCells;
            RefreshAllCells();
        }

        private void OnDestroy()
        {
            if (_gameManagerBehaviour != null && _gameManagerBehaviour.Game != null)
                _gameManagerBehaviour.Game.Grid.GridChanged -= RefreshAllCells;
        }

        public Vector3 GridToWorld(Int2 cell) =>
            transform.position + new Vector3(cell.X * cellSize, cell.Y * cellSize, 0f);

        public Int2 WorldToGrid(Vector3 worldPosition)
        {
            var local = worldPosition - transform.position;
            int x = Mathf.RoundToInt(local.x / cellSize);
            int y = Mathf.RoundToInt(local.y / cellSize);
            return new Int2(x, y);
        }

        private void BuildCellViews()
        {
            var grid = _gameManagerBehaviour.Game.Grid;
            CreateBoardBackdrop(grid.Width, grid.Height);

            _cellViews = new SpriteRenderer[grid.Width, grid.Height];
            _wasOccupied = new bool[grid.Width, grid.Height];

            for (int y = 0; y < grid.Height; y++)
            {
                for (int x = 0; x < grid.Width; x++)
                {
                    var cellObject = new GameObject($"Cell_{x}_{y}");
                    cellObject.transform.SetParent(transform, false);
                    cellObject.transform.localPosition = new Vector3(x * cellSize, y * cellSize, 0f);
                    // Slightly smaller than a full cell so adjacent squares show a visible gap.
                    cellObject.transform.localScale = Vector3.one * (cellSize * 0.9f);

                    var renderer = cellObject.AddComponent<SpriteRenderer>();
                    renderer.sprite = cellSprite != null ? cellSprite : PlaceholderSprite.White;
                    renderer.sortingOrder = 1;
                    _cellViews[x, y] = renderer;
                }
            }
        }

        /// <summary>A single backdrop panel behind the whole 8x8 board, giving it a
        /// framed "board" presentation instead of cells floating on the bare scene background.</summary>
        private void CreateBoardBackdrop(int width, int height)
        {
            const float margin = 0.3f;
            var backdropGo = new GameObject("BoardBackdrop");
            backdropGo.transform.SetParent(transform, false);
            backdropGo.transform.localPosition = new Vector3((width - 1) * cellSize * 0.5f, (height - 1) * cellSize * 0.5f, 0f);
            backdropGo.transform.localScale = new Vector3(width * cellSize + margin, height * cellSize + margin, 1f);

            var renderer = backdropGo.AddComponent<SpriteRenderer>();
            renderer.sprite = PlaceholderSprite.White;
            renderer.color = boardBackdropColor;
            renderer.sortingOrder = 0;
        }

        private void RefreshAllCells()
        {
            var grid = _gameManagerBehaviour.Game.Grid;
            for (int y = 0; y < grid.Height; y++)
            {
                for (int x = 0; x < grid.Width; x++)
                {
                    bool occupied = grid.IsCellOccupied(new Int2(x, y));
                    bool wasOccupied = _wasOccupied[x, y];
                    var renderer = _cellViews[x, y];
                    renderer.color = occupied ? occupiedColor : emptyColor;

                    if (occupied && !wasOccupied)
                        StartCoroutine(PopAnimation(renderer.transform));
                    else if (!occupied && wasOccupied)
                        StartCoroutine(FlashAnimation(renderer));

                    _wasOccupied[x, y] = occupied;
                }
            }
        }

        private IEnumerator PopAnimation(Transform cellTransform)
        {
            Vector3 baseScale = Vector3.one * (cellSize * 0.9f);
            const float duration = 0.12f;
            float t = 0f;
            while (t < duration)
            {
                t += Time.deltaTime;
                float k = Mathf.Lerp(1.35f, 1f, t / duration);
                cellTransform.localScale = baseScale * k;
                yield return null;
            }
            cellTransform.localScale = baseScale;
        }

        private IEnumerator FlashAnimation(SpriteRenderer renderer)
        {
            Color target = emptyColor;
            const float duration = 0.18f;
            float t = 0f;
            while (t < duration)
            {
                t += Time.deltaTime;
                renderer.color = Color.Lerp(clearFlashColor, target, t / duration);
                yield return null;
            }
            renderer.color = target;
        }
    }
}
