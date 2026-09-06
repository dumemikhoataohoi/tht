using System;
using GemGrid.Configuration;
using GemGrid.Core;
using GemGrid.Journey;
using UnityEngine;

namespace GemGrid.Gameplay
{
    /// <summary>
    /// Guarantees every time the Gameplay scene loads, the player gets a fresh
    /// board in the correct mode: Classic (unchanged since M1.5/M2 — <see cref="GameManagerBehaviour.Restart"/>),
    /// or a Journey level / Daily Challenge attempt requested via
    /// <see cref="PendingGameplayIntent"/> (set by MainMenu/JourneyMap before loading
    /// this scene).
    ///
    /// Runs in <see cref="Awake"/>, not <see cref="Start"/>: Unity guarantees every
    /// Awake() in a newly loaded scene runs before any Start() in that same scene, so
    /// this is the only place a Journey/Daily attempt's freshly-built <see cref="GameManager"/>
    /// can be swapped into <see cref="GameManagerBehaviour.Instance"/> via
    /// <see cref="GameManagerBehaviour.ReplaceGame"/> before GridController/BlockTrayView/
    /// GameplayHud/GameOverScreen/AudioHookListener read <c>GameManagerBehaviour.Instance.Game</c>
    /// in their own Start() methods. Lives in the UI assembly (not Gameplay) purely so it
    /// can reference GemGrid.Journey without Gameplay depending back on it.
    ///
    /// NOT verified in the Unity Editor/Play Mode in this environment — see UNITY_SETUP.md.
    /// </summary>
    public class GameplaySessionStarter : MonoBehaviour
    {
        /// <summary>The active Journey/Daily attempt, or null while Classic Mode is running.</summary>
        public static JourneyLevelSession ActiveSession { get; private set; }

        private void Awake()
        {
            var gameManagerBehaviour = GameManagerBehaviour.Instance;
            if (gameManagerBehaviour == null)
                throw new InvalidOperationException(
                    $"{nameof(GameplaySessionStarter)} requires the game to be bootstrapped from the Boot scene first " +
                    "(GameManagerBehaviour.Instance is null) — see UNITY_SETUP.md.");

            ActiveSession?.Dispose();
            ActiveSession = null;

            GemGrid.UI.MetaProgressionService.Instance?.PowerUpLevelUsage.ResetForNewLevel();

            PendingGameplayIntent.Consume(out var journeyGlobalLevelIndex, out bool isDaily);

            if (journeyGlobalLevelIndex.HasValue)
            {
                var level = BuildJourneyLevel(journeyGlobalLevelIndex.Value);
                var game = BuildGameManager(gameManagerBehaviour, level.ComplexityBias);
                StartSession(new JourneyLevelSession(level, game), gameManagerBehaviour);
            }
            else if (isDaily)
            {
                var level = BuildDailyChallengeLevel();
                var game = BuildDailyGameManager(gameManagerBehaviour);
                StartSession(new JourneyLevelSession(level, game), gameManagerBehaviour);
            }
            else
            {
                gameManagerBehaviour.Restart(); // Classic Mode — unchanged since M1.5/M2.
            }
        }

        private static void StartSession(JourneyLevelSession session, GameManagerBehaviour gameManagerBehaviour)
        {
            ActiveSession = session;
            gameManagerBehaviour.ReplaceGame(session.Game);
            session.Start();
        }

        private static LevelDefinition BuildJourneyLevel(int globalIndex)
        {
            int chapterIndex = globalIndex / LevelDefinition.LevelsPerChapter;
            int levelIndexInChapter = globalIndex % LevelDefinition.LevelsPerChapter;
            var levels = ChapterBuilder.BuildChapter(chapterIndex, new DifficultyCurveConfig());
            return levels[levelIndexInChapter];
        }

        /// <summary>Daily Challenge reuses the same JourneyLevelSession machinery with a single Score objective and a date-seeded random — see GAME_DESIGN_V2.md §13.</summary>
        private static LevelDefinition BuildDailyChallengeLevel() =>
            new LevelDefinition(0, 0, new ObjectiveDefinition(ObjectiveType.Score, 300), null, moveBudget: 0, complexityBias: 0f, preFilledCellCount: 0);

        private static GameManager BuildGameManager(GameManagerBehaviour gameManagerBehaviour, float complexityBias)
        {
            var grid = new GridModel(8, 8);
            var score = new ScoreManager(gameManagerBehaviour.GameplayConfig.ScoreRules);
            var combo = new ComboManager(gameManagerBehaviour.GameplayConfig.ComboRules);
            IBlockShapeProvider provider = complexityBias > 0f
                ? new BiasedBlockShapeProvider(gameManagerBehaviour.BlockShapeSet.GetLibrary(), complexityBias)
                : gameManagerBehaviour.BlockShapeSet;
            var spawner = new BlockSpawner(provider, new SystemRandomSource(), traySize: 3);
            return new GameManager(grid, score, combo, spawner);
        }

        private static GameManager BuildDailyGameManager(GameManagerBehaviour gameManagerBehaviour)
        {
            var grid = new GridModel(8, 8);
            var score = new ScoreManager(gameManagerBehaviour.GameplayConfig.ScoreRules);
            var combo = new ComboManager(gameManagerBehaviour.GameplayConfig.ComboRules);
            int seed = DailyChallengeSeed.SeedForDate(DateTime.UtcNow);
            var spawner = new BlockSpawner(gameManagerBehaviour.BlockShapeSet, new SystemRandomSource(seed), traySize: 3);
            return new GameManager(grid, score, combo, spawner);
        }
    }
}
