using GemGrid.Core;
using GemGrid.Gameplay;
using GemGrid.Tests.EditMode;
using NUnit.Framework;

namespace GemGrid.Tests.EditMode.V2
{
    public class GridModelUndoAndHammerTests
    {
        [Test]
        public void ClearCell_OccupiedCell_ClearsItAndReturnsTrue()
        {
            var grid = new GridModel(4, 4);
            grid.Place(BlockShapeTestFactory.SingleCell(), new Int2(1, 1));

            bool cleared = grid.ClearCell(new Int2(1, 1));

            Assert.IsTrue(cleared);
            Assert.IsFalse(grid.IsCellOccupied(new Int2(1, 1)));
        }

        [Test]
        public void ClearCell_EmptyCell_ReturnsFalse()
        {
            var grid = new GridModel(4, 4);

            Assert.IsFalse(grid.ClearCell(new Int2(0, 0)));
        }

        [Test]
        public void ClearCell_OutOfBounds_ReturnsFalse()
        {
            var grid = new GridModel(4, 4);

            Assert.IsFalse(grid.ClearCell(new Int2(-1, 0)));
            Assert.IsFalse(grid.ClearCell(new Int2(4, 0)));
        }

        [Test]
        public void SnapshotAndRestoreOccupancy_RoundTrips()
        {
            var grid = new GridModel(4, 4);
            grid.Place(BlockShapeTestFactory.SingleCell(), new Int2(0, 0));
            grid.Place(BlockShapeTestFactory.SingleCell(), new Int2(2, 2));
            var snapshot = grid.SnapshotOccupancy();

            grid.Place(BlockShapeTestFactory.SingleCell(), new Int2(3, 3));
            grid.RestoreOccupancy(snapshot);

            Assert.IsTrue(grid.IsCellOccupied(new Int2(0, 0)));
            Assert.IsTrue(grid.IsCellOccupied(new Int2(2, 2)));
            Assert.IsFalse(grid.IsCellOccupied(new Int2(3, 3)));
        }

        [Test]
        public void RestoreOccupancy_WrongDimensions_Throws()
        {
            var grid = new GridModel(4, 4);
            var wrongSize = new bool[3, 3];

            Assert.Throws<System.ArgumentException>(() => grid.RestoreOccupancy(wrongSize));
        }
    }
}
