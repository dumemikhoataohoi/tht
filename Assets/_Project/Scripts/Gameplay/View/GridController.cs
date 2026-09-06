using GemGrid.Core;
using UnityEngine;

namespace GemGrid.Gameplay
{
    /// <summary>
    /// Minimal placeholder visual for the grid: converts between world space and grid
    /// coordinates, and renders one sprite per cell so state changes are visible.
    /// Falls back to an auto-generated white square (<see cref="PlaceholderSprite"/>)
    /// if no sprite is assigned in the Inspector, so the grid is visible on Play with
    /// zero manual setup — final art/animation is out of scope for M1 (see M2 - UI).
    ///
    /// NOT verified in the Unity Editor/Play Mode in this environment — see README_M1.md.
    /// </summary>
    public class GridController : MonoBehaviour
    {
        [SerializeField] private float cellSize = 1f;
        [SerializeField] private Sprite cellSprite;
        [SerializeField] private Color emptyColor = Color.gray;
        [SerializeField] private Color occupiedColor = Color.cyan;

        private GameManagerBehaviour _gameManagerBehaviour;
        private SpriteRenderer[,] _cellViews;

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
            _cellViews = new SpriteRenderer[grid.Width, grid.Height];

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
                    _cellViews[x, y] = renderer;
                }
            }
        }

        private void RefreshAllCells()
        {
            var grid = _gameManagerBehaviour.Game.Grid;
            for (int y = 0; y < grid.Height; y++)
            {
                for (int x = 0; x < grid.Width; x++)
                {
                    var cell = new Int2(x, y);
                    _cellViews[x, y].color = grid.IsCellOccupied(cell) ? occupiedColor : emptyColor;
                }
            }
        }
    }
}
