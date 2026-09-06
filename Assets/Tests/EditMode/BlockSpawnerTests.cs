using GemGrid.Gameplay;
using NUnit.Framework;

namespace GemGrid.Tests.EditMode
{
    public class BlockSpawnerTests
    {
        [Test]
        public void RefillTray_FillsAllSlotsUpToConfiguredTraySize()
        {
            var provider = new FakeBlockShapeProvider(BlockShapeTestFactory.SingleCell());
            var spawner = new BlockSpawner(provider, new FakeRandomSource(0), traySize: 3);

            spawner.RefillTray();

            Assert.AreEqual(3, spawner.Tray.Count);
            foreach (var block in spawner.Tray)
                Assert.IsFalse(block.IsEmpty);
        }

        [Test]
        public void IsTrayEmpty_ReturnsTrue_BeforeFirstRefill()
        {
            var provider = new FakeBlockShapeProvider(BlockShapeTestFactory.SingleCell());
            var spawner = new BlockSpawner(provider, new FakeRandomSource(0));

            Assert.IsTrue(spawner.IsTrayEmpty());
        }

        [Test]
        public void ConsumeSlot_ClearsOnlyThatSlot()
        {
            var provider = new FakeBlockShapeProvider(BlockShapeTestFactory.SingleCell());
            var spawner = new BlockSpawner(provider, new FakeRandomSource(0));
            spawner.RefillTray();

            spawner.ConsumeSlot(1);

            Assert.IsFalse(spawner.GetSlot(0).IsEmpty);
            Assert.IsTrue(spawner.GetSlot(1).IsEmpty);
            Assert.IsFalse(spawner.GetSlot(2).IsEmpty);
        }

        [Test]
        public void IsTrayEmpty_ReturnsTrue_OnlyAfterAllSlotsConsumed()
        {
            var provider = new FakeBlockShapeProvider(BlockShapeTestFactory.SingleCell());
            var spawner = new BlockSpawner(provider, new FakeRandomSource(0), traySize: 2);
            spawner.RefillTray();

            spawner.ConsumeSlot(0);
            Assert.IsFalse(spawner.IsTrayEmpty());

            spawner.ConsumeSlot(1);
            Assert.IsTrue(spawner.IsTrayEmpty());
        }

        [Test]
        public void RefillTray_RespectsSpawnWeight_FavoringHeavierShape()
        {
            var heavy = BlockShapeTestFactory.SingleCell(weight: 9);
            var light = BlockShapeTestFactory.Horizontal(2, weight: 1);
            var provider = new FakeBlockShapeProvider(heavy, light);
            // total weight 10; rolls 0..8 => heavy ("single"), roll 9 => light ("h2")
            var spawner = new BlockSpawner(provider, new FakeRandomSource(0, 5, 9), traySize: 3);

            spawner.RefillTray();

            Assert.AreEqual("single", spawner.GetSlot(0).Shape.Id);
            Assert.AreEqual("single", spawner.GetSlot(1).Shape.Id);
            Assert.AreEqual("h2", spawner.GetSlot(2).Shape.Id);
        }
    }
}
