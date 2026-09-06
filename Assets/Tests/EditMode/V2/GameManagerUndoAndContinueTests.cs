using GemGrid.Configuration;
using GemGrid.Core;
using GemGrid.Gameplay;
using GemGrid.Tests.EditMode;
using NUnit.Framework;

namespace GemGrid.Tests.EditMode.V2
{
    public class GameManagerUndoAndContinueTests
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
        public void CanUndo_IsFalse_BeforeAnyMove()
        {
            var game = CreateGame();
            game.StartNewGame();

            Assert.IsFalse(game.CanUndo);
        }

        [Test]
        public void TryUndoLastMove_RevertsGridScoreAndTray()
        {
            var game = CreateGame();
            game.StartNewGame();
            var trayBeforeMove = game.Spawner.GetSlot(0);

            game.TryPlaceBlock(0, new Int2(0, 0));
            Assert.AreEqual(1, game.Score.TotalScore);
            Assert.IsTrue(game.CanUndo);

            bool undone = game.TryUndoLastMove();

            Assert.IsTrue(undone);
            Assert.AreEqual(0, game.Score.TotalScore);
            Assert.IsFalse(game.Grid.IsCellOccupied(new Int2(0, 0)));
            Assert.AreEqual(trayBeforeMove.InstanceId, game.Spawner.GetSlot(0).InstanceId);
            Assert.IsFalse(game.CanUndo, "Undo is single-step only.");
        }

        [Test]
        public void TryUndoLastMove_WithNoPriorMove_ReturnsFalse()
        {
            var game = CreateGame();
            game.StartNewGame();

            Assert.IsFalse(game.TryUndoLastMove());
        }

        [Test]
        public void TryUndoLastMove_CanOnlyBeUsedOnce_SecondCallFails()
        {
            var game = CreateGame();
            game.StartNewGame();
            game.TryPlaceBlock(0, new Int2(0, 0));

            Assert.IsTrue(game.TryUndoLastMove());
            Assert.IsFalse(game.TryUndoLastMove());
        }

        [Test]
        public void RestartGame_ClearsUndoHistory()
        {
            var game = CreateGame();
            game.StartNewGame();
            game.TryPlaceBlock(0, new Int2(0, 0));

            game.RestartGame();

            Assert.IsFalse(game.CanUndo);
        }

        [Test]
        public void CanContinue_IsFalse_WhileStillPlaying()
        {
            var game = CreateGame();
            game.StartNewGame();

            Assert.IsFalse(game.CanContinue);
        }

        [Test]
        public void TryContinueAfterGameOver_ResumesPlayingWithLegalMoveAvailable()
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
            game.TryPlaceBlock(0, new Int2(1, 1)); // triggers Game Over (domino cannot fit diagonally-remaining cells)
            Assert.AreEqual(GameStateType.GameOver, game.State);

            bool continued = game.TryContinueAfterGameOver();

            Assert.IsTrue(continued);
            Assert.AreEqual(GameStateType.Playing, game.State);
        }

        [Test]
        public void TryContinueAfterGameOver_CanOnlyBeUsedOncePerSession()
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
            game.TryPlaceBlock(0, new Int2(1, 1));
            Assert.IsTrue(game.TryContinueAfterGameOver());
            Assert.AreEqual(GameStateType.Playing, game.State);

            // Force a *second*, fully controlled Game Over deterministically (no
            // simulated play loop — repeated block placement on a small deterministic
            // grid can cycle through line-clears indefinitely instead of ever reaching
            // a genuine Game Over, which previously hung this test). Directly fill every
            // cell — the current tray (whatever RefillTray dealt during Continue) cannot
            // possibly fit on a fully-occupied grid — then ask GameManager to notice.
            grid.RestoreOccupancy(new[,] { { true, true }, { true, true } });
            Assert.IsTrue(game.CheckGameOverNow());

            Assert.IsFalse(game.CanContinue, "The single-use lock must persist across a second Game Over in the same session.");
            Assert.IsFalse(game.TryContinueAfterGameOver());
        }

        [Test]
        public void TryContinueAfterGameOver_WhileStillPlaying_ReturnsFalse()
        {
            var game = CreateGame();
            game.StartNewGame();

            Assert.IsFalse(game.TryContinueAfterGameOver());
        }

        [Test]
        public void CheckGameOverNow_ReflectsCurrentState()
        {
            var game = CreateGame();
            game.StartNewGame();

            Assert.IsFalse(game.CheckGameOverNow());
        }
    }
}
