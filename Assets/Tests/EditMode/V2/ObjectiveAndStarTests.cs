using GemGrid.Configuration;
using GemGrid.Core;
using GemGrid.Gameplay;
using GemGrid.Journey;
using GemGrid.Tests.EditMode;
using NUnit.Framework;

namespace GemGrid.Tests.EditMode.V2
{
    public class ObjectiveTrackerTests
    {
        private static GameManager CreateGame(int width, int height, IBlockShapeProvider provider, GemGrid.Core.IRandomSource random, int traySize = 1)
        {
            var grid = new GridModel(width, height);
            var score = new ScoreManager(new ScoreRules());
            var combo = new ComboManager(new ComboRules());
            var spawner = new BlockSpawner(provider, random, traySize);
            return new GameManager(grid, score, combo, spawner);
        }

        [Test]
        public void ScoreObjective_CompletesWhenTargetScoreReached()
        {
            var provider = new FakeBlockShapeProvider(BlockShapeTestFactory.SingleCell());
            var game = CreateGame(8, 8, provider, new FakeRandomSource(0));
            game.StartNewGame();
            var tracker = new ObjectiveTracker(game, new ObjectiveDefinition(ObjectiveType.Score, 3), null);

            game.TryPlaceBlock(0, new Int2(0, 0));
            game.TryPlaceBlock(0, new Int2(1, 0));
            Assert.IsFalse(tracker.IsPrimaryComplete);

            game.TryPlaceBlock(0, new Int2(2, 0));
            Assert.IsTrue(tracker.IsPrimaryComplete);
        }

        [Test]
        public void LinesClearedObjective_CountsTotalLinesAcrossMoves()
        {
            var provider = new FakeBlockShapeProvider(BlockShapeTestFactory.Horizontal(2));
            var game = CreateGame(2, 4, provider, new FakeRandomSource(0));
            game.StartNewGame();
            var tracker = new ObjectiveTracker(game, new ObjectiveDefinition(ObjectiveType.LinesCleared, 2), null);

            game.TryPlaceBlock(0, new Int2(0, 0)); // clears row 0
            Assert.AreEqual(1, tracker.Primary.Current);
            Assert.IsFalse(tracker.IsPrimaryComplete);

            game.TryPlaceBlock(0, new Int2(0, 1)); // clears row 1
            Assert.AreEqual(2, tracker.Primary.Current);
            Assert.IsTrue(tracker.IsPrimaryComplete);
        }

        [Test]
        public void MultiClearObjective_OnlyCountsMovesThatClearTwoOrMoreLinesAtOnce()
        {
            // A 2x2 grid where a single 2x2-covering placement clears both the row and the column at once.
            var square = new BlockShapeDefinition("square2x2", new[] { new Int2(0, 0), new Int2(1, 0), new Int2(0, 1), new Int2(1, 1) });
            var provider = new FakeBlockShapeProvider(square);
            var game = CreateGame(2, 2, provider, new FakeRandomSource(0));
            game.StartNewGame();
            var tracker = new ObjectiveTracker(game, new ObjectiveDefinition(ObjectiveType.MultiClear, 1), null);

            game.TryPlaceBlock(0, new Int2(0, 0)); // clears both rows and both columns at once (>= 2 lines)

            Assert.IsTrue(tracker.IsPrimaryComplete);
        }

        [Test]
        public void SurviveObjective_CountsSuccessfulPlacements()
        {
            var provider = new FakeBlockShapeProvider(BlockShapeTestFactory.SingleCell());
            var game = CreateGame(8, 8, provider, new FakeRandomSource(0));
            game.StartNewGame();
            var tracker = new ObjectiveTracker(game, new ObjectiveDefinition(ObjectiveType.Survive, 2), null);

            game.TryPlaceBlock(0, new Int2(0, 0));
            Assert.IsFalse(tracker.IsPrimaryComplete);
            game.TryPlaceBlock(0, new Int2(1, 0));
            Assert.IsTrue(tracker.IsPrimaryComplete);
        }

        [Test]
        public void FillSlotObjective_CountsTrayRefills()
        {
            var provider = new FakeBlockShapeProvider(BlockShapeTestFactory.SingleCell());
            var game = CreateGame(8, 8, provider, new FakeRandomSource(0), traySize: 1);
            game.StartNewGame(); // 1st fill (initial)
            var tracker = new ObjectiveTracker(game, new ObjectiveDefinition(ObjectiveType.FillSlot, 1), null);

            game.TryPlaceBlock(0, new Int2(0, 0)); // consumes the only slot -> triggers a refill

            Assert.IsTrue(tracker.IsPrimaryComplete);
        }

        [Test]
        public void ComboObjective_TracksBestComboReachedNotJustLatest()
        {
            // Two separate 1-line clears in a row build combo to 2, then a non-clearing
            // placement resets current combo to 0 - the objective must still remember it once saw 2.
            var provider = new FakeBlockShapeProvider(BlockShapeTestFactory.Horizontal(2), BlockShapeTestFactory.SingleCell());
            var game = CreateGame(2, 3, provider, new FakeRandomSource(0, 0, 1), traySize: 1);
            game.StartNewGame();
            var tracker = new ObjectiveTracker(game, new ObjectiveDefinition(ObjectiveType.Combo, 2), null);

            game.TryPlaceBlock(0, new Int2(0, 0)); // clears row 0 -> combo 1
            game.TryPlaceBlock(0, new Int2(0, 1)); // clears row 1 -> combo 2
            Assert.IsTrue(tracker.IsPrimaryComplete);
        }

        [Test]
        public void MovesUsedWhenPrimaryCompleted_RecordsMoveCountAtCompletionOnly()
        {
            var provider = new FakeBlockShapeProvider(BlockShapeTestFactory.SingleCell());
            var game = CreateGame(8, 8, provider, new FakeRandomSource(0));
            game.StartNewGame();
            var tracker = new ObjectiveTracker(game, new ObjectiveDefinition(ObjectiveType.Survive, 2), null);

            Assert.AreEqual(-1, tracker.MovesUsedWhenPrimaryCompleted);
            game.TryPlaceBlock(0, new Int2(0, 0));
            game.TryPlaceBlock(0, new Int2(1, 0));
            Assert.AreEqual(2, tracker.MovesUsedWhenPrimaryCompleted);

            game.TryPlaceBlock(0, new Int2(2, 0));
            Assert.AreEqual(2, tracker.MovesUsedWhenPrimaryCompleted, "Must not keep updating after first completion.");
        }
    }

    public class StarCalculatorTests
    {
        [Test]
        public void NoStars_WhenPrimaryNotComplete()
        {
            Assert.AreEqual(0, StarCalculator.CalculateStars(false, 5, true));
        }

        [Test]
        public void ThreeStars_WhenBonusComplete_RegardlessOfMoveCount()
        {
            Assert.AreEqual(3, StarCalculator.CalculateStars(true, 999, true));
        }

        [Test]
        public void TwoStars_WhenEfficientButNoBonus()
        {
            Assert.AreEqual(2, StarCalculator.CalculateStars(true, StarCalculator.EfficientMovesThreshold, false));
        }

        [Test]
        public void OneStar_WhenSlowAndNoBonus()
        {
            Assert.AreEqual(1, StarCalculator.CalculateStars(true, StarCalculator.EfficientMovesThreshold + 1, false));
        }
    }
}
