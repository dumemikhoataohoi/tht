using GemGrid.Configuration;
using GemGrid.Core;
using GemGrid.Gameplay;
using GemGrid.Journey;
using GemGrid.Tests.EditMode;
using NUnit.Framework;

namespace GemGrid.Tests.EditMode.V2
{
    public class JourneyLevelSessionTests
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
        public void AttemptStaysInProgress_AfterPrimaryObjectiveMet_UntilNaturalGameOver()
        {
            // See GAME_DESIGN_V2.md §2: Journey resolves win/lose the same way Classic
            // resolves Game Over, so a player can keep playing past the primary objective
            // to also chase the bonus objective for a 3rd star.
            var provider = new FakeBlockShapeProvider(BlockShapeTestFactory.SingleCell());
            var game = CreateGame(8, 8, provider, new FakeRandomSource(0));
            var level = new LevelDefinition(0, 5, new ObjectiveDefinition(ObjectiveType.Score, 2), null, 0, 0f, 0);
            var session = new JourneyLevelSession(level, game);
            session.Start();

            game.TryPlaceBlock(0, new Int2(0, 0));
            game.TryPlaceBlock(0, new Int2(1, 0));

            Assert.IsTrue(session.Objectives.IsPrimaryComplete);
            Assert.AreEqual(LevelAttemptResult.InProgress, session.Result);
            session.Dispose();
        }

        [Test]
        public void Win_IsRecorded_WhenObjectiveMetByTheTimeGameOverNaturallyHappens()
        {
            var single = BlockShapeTestFactory.SingleCell();
            var domino = BlockShapeTestFactory.Horizontal(2);
            var provider = new FakeBlockShapeProvider(single, domino);
            var game = CreateGame(2, 2, provider, new FakeRandomSource(0, 0, 1));
            var level = new LevelDefinition(0, 5, new ObjectiveDefinition(ObjectiveType.Survive, 2), null, 0, 0f, 0);
            var session = new JourneyLevelSession(level, game);
            session.Start();

            game.TryPlaceBlock(0, new Int2(0, 0));
            game.TryPlaceBlock(0, new Int2(1, 1)); // 2nd successful placement satisfies Survive(2); also triggers natural Game Over (see GameManagerFlowTests for this exact fixture)

            Assert.AreEqual(GameStateType.GameOver, game.State);
            Assert.AreEqual(LevelAttemptResult.Won, session.Result);
            session.Dispose();
        }

        [Test]
        public void Lose_IsRecorded_WhenGameOverHappensBeforeObjectiveMet()
        {
            var single = BlockShapeTestFactory.SingleCell();
            var domino = BlockShapeTestFactory.Horizontal(2);
            var provider = new FakeBlockShapeProvider(single, domino);
            var game = CreateGame(2, 2, provider, new FakeRandomSource(0, 0, 1));
            var level = new LevelDefinition(0, 5, new ObjectiveDefinition(ObjectiveType.Score, 999), null, 0, 0f, 0);
            var session = new JourneyLevelSession(level, game);
            session.Start();

            game.TryPlaceBlock(0, new Int2(0, 0));
            game.TryPlaceBlock(0, new Int2(1, 1)); // triggers natural Game Over, objective (999 score) nowhere near met

            Assert.AreEqual(GameStateType.GameOver, game.State);
            Assert.AreEqual(LevelAttemptResult.Lost, session.Result);
            Assert.AreEqual(0, session.CalculateStars());
            session.Dispose();
        }

        [Test]
        public void Start_WithPreFilledCells_SeedsGridBeforePlayBegins()
        {
            var provider = new FakeBlockShapeProvider(BlockShapeTestFactory.SingleCell());
            var game = CreateGame(8, 8, provider, new FakeRandomSource(0));
            var level = new LevelDefinition(2, 2, new ObjectiveDefinition(ObjectiveType.Score, 1), null, 0, 0f, preFilledCellCount: 5);
            var session = new JourneyLevelSession(level, game);

            session.Start();

            int occupiedCount = 0;
            for (int x = 0; x < 8; x++)
                for (int y = 0; y < 8; y++)
                    if (game.Grid.IsCellOccupied(new Int2(x, y))) occupiedCount++;

            Assert.AreEqual(5, occupiedCount);
            session.Dispose();
        }

        [Test]
        public void CalculateStars_ThreeStars_WhenBonusObjectiveAlsoMet()
        {
            var single = BlockShapeTestFactory.SingleCell();
            var domino = BlockShapeTestFactory.Horizontal(2);
            var provider = new FakeBlockShapeProvider(single, domino);
            var game = CreateGame(2, 2, provider, new FakeRandomSource(0, 0, 1));
            var level = new LevelDefinition(0, 5, new ObjectiveDefinition(ObjectiveType.Survive, 1), new ObjectiveDefinition(ObjectiveType.Survive, 2), 0, 0f, 0);
            var session = new JourneyLevelSession(level, game);
            session.Start();

            game.TryPlaceBlock(0, new Int2(0, 0)); // primary (Survive 1) already met here
            game.TryPlaceBlock(0, new Int2(1, 1)); // bonus (Survive 2) met here too, and triggers natural Game Over

            Assert.AreEqual(LevelAttemptResult.Won, session.Result);
            Assert.AreEqual(3, session.CalculateStars());
            session.Dispose();
        }
    }
}
