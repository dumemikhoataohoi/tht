using GemGrid.Configuration;
using GemGrid.Journey;
using GemGrid.Tests.EditMode;
using NUnit.Framework;

namespace GemGrid.Tests.EditMode.V2
{
    public class BiasedBlockShapeProviderTests
    {
        [Test]
        public void ZeroBias_AllShapesKeepTheirOriginalWeight()
        {
            var baseLibrary = new BlockShapeLibrary(new[] { BlockShapeTestFactory.SingleCell(weight: 3), BlockShapeTestFactory.Horizontal(4, weight: 3) });

            var biased = new BiasedBlockShapeProvider(baseLibrary, complexityBias: 0f).GetLibrary();

            foreach (var shape in biased.AllShapes)
                Assert.AreEqual(3, shape.SpawnWeight);
        }

        [Test]
        public void HighBias_LargerShapesGetHeavierWeightThanSmallOnes()
        {
            var baseLibrary = new BlockShapeLibrary(new[] { BlockShapeTestFactory.SingleCell(weight: 1), BlockShapeTestFactory.Horizontal(5, weight: 1) });

            var biased = new BiasedBlockShapeProvider(baseLibrary, complexityBias: 1f).GetLibrary();

            var single = biased.AllShapes[0];
            var big = biased.AllShapes[1];
            Assert.Greater(big.SpawnWeight, single.SpawnWeight);
        }

        [Test]
        public void EveryShape_AlwaysKeepsAtLeastWeightOne_NeverFullyExcluded()
        {
            var baseLibrary = new BlockShapeLibrary(new[] { BlockShapeTestFactory.SingleCell(weight: 1), BlockShapeTestFactory.Horizontal(5, weight: 1) });

            var biased = new BiasedBlockShapeProvider(baseLibrary, complexityBias: 1f).GetLibrary();

            foreach (var shape in biased.AllShapes)
                Assert.GreaterOrEqual(shape.SpawnWeight, 1, "GAME_DESIGN_V2.md §7 bans fully removing the easy escape hatch.");
        }

        [Test]
        public void OutOfRangeBias_IsClampedRatherThanThrowing()
        {
            var baseLibrary = new BlockShapeLibrary(new[] { BlockShapeTestFactory.SingleCell() });

            Assert.DoesNotThrow(() => new BiasedBlockShapeProvider(baseLibrary, complexityBias: 5f));
            Assert.DoesNotThrow(() => new BiasedBlockShapeProvider(baseLibrary, complexityBias: -5f));
        }
    }
}
