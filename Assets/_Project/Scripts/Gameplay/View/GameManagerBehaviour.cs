using GemGrid.Configuration;
using GemGrid.Core;
using UnityEngine;

namespace GemGrid.Gameplay
{
    /// <summary>
    /// Composition root for the gameplay scene: wires ScriptableObject configuration into
    /// the pure C# gameplay classes (<see cref="GridModel"/>, <see cref="ScoreManager"/>,
    /// <see cref="ComboManager"/>, <see cref="BlockSpawner"/>) and exposes the resulting
    /// <see cref="GameManager"/> for other MonoBehaviours (GridController,
    /// BlockDragController, hook listeners) to use.
    ///
    /// NOT verified in the Unity Editor/Play Mode in this environment (no Unity Editor
    /// available in this sandbox) — see README_M1.md.
    /// </summary>
    public class GameManagerBehaviour : MonoBehaviour
    {
        [SerializeField] private BlockShapeSet blockShapeSet;
        [SerializeField] private GameplayConfig gameplayConfig;
        [SerializeField] private int gridWidth = 8;
        [SerializeField] private int gridHeight = 8;
        [SerializeField] private int traySize = 3;

        public GameManager Game { get; private set; }

        private void Awake()
        {
            if (blockShapeSet == null)
                throw new System.InvalidOperationException("GameManagerBehaviour requires a BlockShapeSet reference.");
            if (gameplayConfig == null)
                throw new System.InvalidOperationException("GameManagerBehaviour requires a GameplayConfig reference.");

            var grid = new GridModel(gridWidth, gridHeight);
            var score = new ScoreManager(gameplayConfig.ScoreRules);
            var combo = new ComboManager(gameplayConfig.ComboRules);
            var random = new SystemRandomSource();
            var spawner = new BlockSpawner(blockShapeSet, random, traySize);

            Game = new GameManager(grid, score, combo, spawner);
        }

        private void Start()
        {
            Game.StartNewGame();
        }

        public void Restart()
        {
            Game.RestartGame();
        }
    }
}
