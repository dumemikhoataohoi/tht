using GemGrid.Configuration;
using GemGrid.Core;
using UnityEngine;

namespace GemGrid.Gameplay
{
    /// <summary>
    /// App-level composition root: wires ScriptableObject configuration into the pure
    /// C# gameplay classes (<see cref="GridModel"/>, <see cref="ScoreManager"/>,
    /// <see cref="ComboManager"/>, <see cref="BlockSpawner"/>) and exposes the resulting
    /// <see cref="GameManager"/> for other MonoBehaviours (GridController,
    /// BlockDragController, hook listeners) to use.
    ///
    /// Lives on a GameObject in the Boot scene and survives the transition to the
    /// Gameplay scene via <see cref="Object.DontDestroyOnLoad"/> — see
    /// UNITY_SETUP.md for the Boot ▸ Gameplay scene flow. Other MonoBehaviours find it
    /// through <see cref="Instance"/> rather than requiring it as a sibling component,
    /// since it no longer lives in the same scene as them.
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

        public static GameManagerBehaviour Instance { get; private set; }

        public GameManager Game { get; private set; }

        /// <summary>
        /// The original, persistent Classic Mode GameManager built at Boot — kept
        /// separately from <see cref="Game"/> so <see cref="Restart"/> can always get back
        /// to real Classic Mode even after a Journey/Daily attempt has pointed
        /// <see cref="Game"/> at a different, throwaway GameManager via <see cref="ReplaceGame"/>.
        /// </summary>
        private GameManager _classicGame;

        /// <summary>Exposed so Journey/Daily Challenge attempts (see GameplaySessionStarter) can build their own GameManager reusing the same shapes/rules as Classic Mode.</summary>
        public BlockShapeSet BlockShapeSet => blockShapeSet;

        /// <summary>See <see cref="BlockShapeSet"/> above.</summary>
        public GameplayConfig GameplayConfig => gameplayConfig;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning("A second GameManagerBehaviour was found; destroying the duplicate. " +
                                  "Only the one bootstrapped from the Boot scene should exist.");
                Destroy(gameObject);
                return;
            }

            if (blockShapeSet == null)
                throw new System.InvalidOperationException("GameManagerBehaviour requires a BlockShapeSet reference.");
            if (gameplayConfig == null)
                throw new System.InvalidOperationException("GameManagerBehaviour requires a GameplayConfig reference.");

            Instance = this;
            DontDestroyOnLoad(gameObject);

            var grid = new GridModel(gridWidth, gridHeight);
            var score = new ScoreManager(gameplayConfig.ScoreRules);
            var combo = new ComboManager(gameplayConfig.ComboRules);
            var random = new SystemRandomSource();
            var spawner = new BlockSpawner(blockShapeSet, random, traySize);

            _classicGame = new GameManager(grid, score, combo, spawner);
            Game = _classicGame;
        }

        private void Start()
        {
            Game.StartNewGame();
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        /// <summary>
        /// Classic Mode restart. Always (re-)points <see cref="Game"/> at the original
        /// persistent <see cref="_classicGame"/> first — necessary because a prior
        /// Journey/Daily attempt may have left <see cref="Game"/> pointed at a different,
        /// throwaway GameManager via <see cref="ReplaceGame"/>.
        /// </summary>
        public void Restart()
        {
            Game = _classicGame;
            Game.RestartGame();
        }

        /// <summary>
        /// Swaps in a different <see cref="GameManager"/> instance — used to point the
        /// shared view layer (GridController, BlockTrayView, GameplayHud, GameOverScreen,
        /// AudioHookListener, ...) at a Journey/Daily Challenge attempt's own GameManager
        /// instead of the persistent Classic Mode one. Must be called from Awake() (see
        /// GameplaySessionStarter) — those view components read <see cref="Game"/> in
        /// their own Start(), and Unity guarantees every Awake() in a scene runs before
        /// any Start() in that same scene.
        /// </summary>
        public void ReplaceGame(GameManager newGame)
        {
            Game = newGame ?? throw new System.ArgumentNullException(nameof(newGame));
        }
    }
}
