using GemGrid.Configuration;
using GemGrid.Core;
using GemGrid.Gameplay;
using GemGrid.PowerUps;
using GemGrid.Tests.EditMode;
using NUnit.Framework;

namespace GemGrid.Tests.EditMode.V2
{
    public class PowerUpInventoryTests
    {
        [Test]
        public void TryConsume_WithNoneOwned_FailsAndChangesNothing()
        {
            var inventory = new PowerUpInventory();

            Assert.IsFalse(inventory.TryConsume(PowerUpId.Hammer));
            Assert.AreEqual(0, inventory.GetOwnedCount(PowerUpId.Hammer));
        }

        [Test]
        public void Add_ThenConsume_DecrementsCorrectly()
        {
            var inventory = new PowerUpInventory();
            inventory.Add(PowerUpId.Hint, 2);

            Assert.IsTrue(inventory.TryConsume(PowerUpId.Hint));
            Assert.AreEqual(1, inventory.GetOwnedCount(PowerUpId.Hint));
            Assert.IsTrue(inventory.TryConsume(PowerUpId.Hint));
            Assert.AreEqual(0, inventory.GetOwnedCount(PowerUpId.Hint));
            Assert.IsFalse(inventory.TryConsume(PowerUpId.Hint));
        }

        [Test]
        public void SnapshotAndLoadSnapshot_RoundTrips()
        {
            var inventory = new PowerUpInventory();
            inventory.Add(PowerUpId.Hammer, 3);
            inventory.Add(PowerUpId.Shuffle, 1);

            var restored = new PowerUpInventory();
            restored.LoadSnapshot(inventory.Snapshot());

            Assert.AreEqual(3, restored.GetOwnedCount(PowerUpId.Hammer));
            Assert.AreEqual(1, restored.GetOwnedCount(PowerUpId.Shuffle));
        }
    }

    public class PowerUpLevelUsageTests
    {
        [Test]
        public void CanUse_FreeUseAvailable_TrueEvenWithEmptyInventory()
        {
            var usage = new PowerUpLevelUsage(PowerUpCatalog.CreateDefault()); // Hint grants 1 free use/level
            var inventory = new PowerUpInventory();
            usage.ResetForNewLevel();

            Assert.IsTrue(usage.CanUse(PowerUpId.Hint, inventory));
        }

        [Test]
        public void Consume_UsesFreeUseFirst_BeforeTouchingInventory()
        {
            var usage = new PowerUpLevelUsage(PowerUpCatalog.CreateDefault());
            var inventory = new PowerUpInventory();
            inventory.Add(PowerUpId.Hint, 1);
            usage.ResetForNewLevel();

            Assert.IsTrue(usage.Consume(PowerUpId.Hint, inventory));

            Assert.AreEqual(1, inventory.GetOwnedCount(PowerUpId.Hint), "The free use should have been spent, not the owned unit.");
        }

        [Test]
        public void Consume_AfterFreeUseExhausted_FallsBackToInventory()
        {
            var usage = new PowerUpLevelUsage(PowerUpCatalog.CreateDefault());
            var inventory = new PowerUpInventory();
            inventory.Add(PowerUpId.Hint, 1);
            usage.ResetForNewLevel();
            usage.Consume(PowerUpId.Hint, inventory); // spends the free use

            bool secondUse = usage.Consume(PowerUpId.Hint, inventory);

            Assert.IsTrue(secondUse);
            Assert.AreEqual(0, inventory.GetOwnedCount(PowerUpId.Hint));
        }

        [Test]
        public void CanUse_NoFreeUseAndEmptyInventory_IsIllegal()
        {
            var usage = new PowerUpLevelUsage(PowerUpCatalog.CreateDefault()); // Hammer grants 0 free uses/level
            var inventory = new PowerUpInventory();
            usage.ResetForNewLevel();

            Assert.IsFalse(usage.CanUse(PowerUpId.Hammer, inventory));
            Assert.IsFalse(usage.Consume(PowerUpId.Hammer, inventory));
        }

        [Test]
        public void ResetForNewLevel_RestoresFreeUses()
        {
            var usage = new PowerUpLevelUsage(PowerUpCatalog.CreateDefault());
            var inventory = new PowerUpInventory();
            usage.ResetForNewLevel();
            usage.Consume(PowerUpId.Hint, inventory); // uses up the 1 free Hint

            usage.ResetForNewLevel();

            Assert.IsTrue(usage.CanUse(PowerUpId.Hint, inventory), "A new level attempt must grant a fresh free use.");
        }
    }

    public class HintFinderTests
    {
        [Test]
        public void FindValidPlacement_ReturnsALegalCell()
        {
            var grid = new GridModel(4, 4);
            var shape = BlockShapeTestFactory.SingleCell();

            var result = HintFinder.FindValidPlacement(grid, shape);

            Assert.IsTrue(result.HasValue);
            Assert.IsTrue(grid.CanPlace(shape, result.Value));
        }

        [Test]
        public void FindValidPlacement_NoRoomLeft_ReturnsNull()
        {
            var grid = new GridModel(1, 1);
            var shape = BlockShapeTestFactory.SingleCell();
            grid.Place(shape, new Int2(0, 0));
            var biggerShape = BlockShapeTestFactory.Horizontal(2);

            var result = HintFinder.FindValidPlacement(grid, biggerShape);

            Assert.IsFalse(result.HasValue);
        }
    }

    public class HammerShuffleUndoActionTests
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
        public void HammerAction_ClearsTargetedCell()
        {
            var provider = new FakeBlockShapeProvider(BlockShapeTestFactory.SingleCell());
            var game = CreateGame(4, 4, provider, new FakeRandomSource(0));
            game.StartNewGame();
            game.TryPlaceBlock(0, new Int2(1, 1));

            bool applied = HammerAction.Apply(game, new Int2(1, 1));

            Assert.IsTrue(applied);
            Assert.IsFalse(game.Grid.IsCellOccupied(new Int2(1, 1)));
        }

        [Test]
        public void HammerAction_OnEmptyCell_ReturnsFalse()
        {
            var provider = new FakeBlockShapeProvider(BlockShapeTestFactory.SingleCell());
            var game = CreateGame(4, 4, provider, new FakeRandomSource(0));
            game.StartNewGame();

            Assert.IsFalse(HammerAction.Apply(game, new Int2(0, 0)));
        }

        [Test]
        public void ShuffleAction_DealsANewTray()
        {
            var provider = new FakeBlockShapeProvider(BlockShapeTestFactory.SingleCell());
            var game = CreateGame(8, 8, provider, new FakeRandomSource(0));
            game.StartNewGame();
            var before = game.Spawner.GetSlot(0).InstanceId;

            bool applied = ShuffleAction.Apply(game);

            Assert.IsTrue(applied);
            Assert.AreNotEqual(before, game.Spawner.GetSlot(0).InstanceId);
        }

        [Test]
        public void UndoAction_DelegatesToGameManager()
        {
            var provider = new FakeBlockShapeProvider(BlockShapeTestFactory.SingleCell());
            var game = CreateGame(8, 8, provider, new FakeRandomSource(0));
            game.StartNewGame();
            game.TryPlaceBlock(0, new Int2(0, 0));

            bool applied = UndoAction.Apply(game);

            Assert.IsTrue(applied);
            Assert.IsFalse(game.Grid.IsCellOccupied(new Int2(0, 0)));
        }

        [Test]
        public void HammerAction_WhileNotPlaying_IsIllegal()
        {
            var provider = new FakeBlockShapeProvider(BlockShapeTestFactory.SingleCell());
            var game = CreateGame(4, 4, provider, new FakeRandomSource(0));
            // Never started -> State is Idle, not Playing.

            Assert.IsFalse(HammerAction.Apply(game, new Int2(0, 0)));
        }
    }
}
