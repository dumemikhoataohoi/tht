using GemGrid.Configuration;
using GemGrid.Core;
using GemGrid.Gameplay;
using NUnit.Framework;

namespace GemGrid.Tests.EditMode
{
    public class GameManagerFlowTests
    {
        private static GameManager CreateGame(int width = 8, int height = 8, int traySize = 3)
        {
            var grid = new GridModel(width, height);
            var score = new ScoreManager(new ScoreRules());
            var combo = new ComboManager(new ComboRules());
            var provider = new FakeBlockShapeProvider(BlockShapeTestFactory.SingleCell());
            var spawner = new BlockSpawner(provider, new FakeRandomSource(0), traySize);
            return new GameManager(grid, score, combo, spawner);
        }

        [Test]
        public void StartNewGame_FillsTrayAndEntersPlayingState()
        {
            var game = CreateGame();

            game.StartNewGame();

            Assert.AreEqual(GameStateType.Playing, game.State);
            foreach (var block in game.Spawner.Tray)
                Assert.IsFalse(block.IsEmpty);
        }

        [Test]
        public void TryPlaceBlock_ValidMove_ConsumesSlotAndAddsScore()
        {
            var game = CreateGame();
            game.StartNewGame();

            bool success = game.TryPlaceBlock(0, new Int2(0, 0));

            Assert.IsTrue(success);
            Assert.AreEqual(1, game.Score.TotalScore);
        }

        [Test]
        public void TryPlaceBlock_InvalidMove_ReturnsFalseAndDoesNotChangeScore()
        {
            var game = CreateGame();
            game.StartNewGame();
            game.TryPlaceBlock(0, new Int2(0, 0));

            bool success = game.TryPlaceBlock(1, new Int2(0, 0)); // cell already occupied

            Assert.IsFalse(success);
            Assert.AreEqual(1, game.Score.TotalScore);
        }

        [Test]
        public void TryPlaceBlock_WhenTrayFullyConsumed_TriggersAutomaticRefill()
        {
            var game = CreateGame(traySize: 1);
            game.StartNewGame();

            game.TryPlaceBlock(0, new Int2(0, 0));

            Assert.IsFalse(game.Spawner.GetSlot(0).IsEmpty);
        }

        [Test]
        public void RestartGame_ClearsGridAndScoreAndReturnsToPlaying()
        {
            var game = CreateGame();
            game.StartNewGame();
            game.TryPlaceBlock(0, new Int2(0, 0));

            game.RestartGame();

            Assert.AreEqual(GameStateType.Playing, game.State);
            Assert.AreEqual(0, game.Score.TotalScore);
            Assert.IsFalse(game.Grid.IsCellOccupied(new Int2(0, 0)));
        }

        [Test]
        public void GameOver_IsTriggered_WhenNewlySpawnedTrayShapeFitsNowhere()
        {
            var single = BlockShapeTestFactory.SingleCell();
            var domino = BlockShapeTestFactory.Horizontal(2);
            var provider = new FakeBlockShapeProvider(single, domino);
            var grid = new GridModel(2, 2);
            var score = new ScoreManager(new ScoreRules());
            var combo = new ComboManager(new ComboRules());
            // total weight 2: seq[0]=0 -> single, seq[1]=0 -> single, seq[2]=1 -> domino
            var spawner = new BlockSpawner(provider, new FakeRandomSource(0, 0, 1), traySize: 1);
            var game = new GameManager(grid, score, combo, spawner);

            game.StartNewGame(); // tray: single
            Assert.IsTrue(game.TryPlaceBlock(0, new Int2(0, 0))); // occupies (0,0); refills tray -> single
            Assert.IsTrue(game.TryPlaceBlock(0, new Int2(1, 1))); // occupies (1,1) (diagonal, no line clear); refills tray -> domino

            // Remaining free cells (1,0) and (0,1) are not horizontally adjacent,
            // so the freshly spawned 2-cell horizontal domino cannot be placed anywhere.
            Assert.AreEqual(GameStateType.GameOver, game.State);
        }

        [Test]
        public void TryPlaceBlock_AfterGameOver_IsRejected()
        {
            var single = BlockShapeTestFactory.SingleCell();
            var domino = BlockShapeTestFactory.Horizontal(2);
            var provider = new FakeBlockShapeProvider(single, domino);
            var grid = new GridModel(2, 2);
            var score = new ScoreManager(new ScoreRules());
            var combo = new ComboManager(new ComboRules());
            var spawner = new BlockSpawner(provider, new FakeRandomSource(0, 0, 1), traySize: 1);
            var game = new GameManager(grid, score, combo, spawner);
            game.StartNewGame();
            game.TryPlaceBlock(0, new Int2(0, 0));
            game.TryPlaceBlock(0, new Int2(1, 1)); // triggers game over

            bool success = game.TryPlaceBlock(0, new Int2(0, 1));

            Assert.IsFalse(success);
        }
    }
}
