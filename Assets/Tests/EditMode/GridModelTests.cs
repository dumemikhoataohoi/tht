using GemGrid.Configuration;
using GemGrid.Core;
using GemGrid.Gameplay;
using NUnit.Framework;

namespace GemGrid.Tests.EditMode
{
    public class GridModelTests
    {
        [Test]
        public void Constructor_CreatesGridWithConfiguredWidthAndHeight()
        {
            var grid = new GridModel(8, 8);

            Assert.AreEqual(8, grid.Width);
            Assert.AreEqual(8, grid.Height);
        }

        [Test]
        public void NewGrid_AllCellsStartEmpty()
        {
            var grid = new GridModel(8, 8);

            for (int y = 0; y < grid.Height; y++)
                for (int x = 0; x < grid.Width; x++)
                    Assert.IsFalse(grid.IsCellOccupied(new Int2(x, y)), $"Cell ({x},{y}) should start empty.");
        }

        [Test]
        public void CanPlace_ReturnsTrue_WhenTargetCellsAreEmptyAndInsideGrid()
        {
            var grid = new GridModel(8, 8);

            Assert.IsTrue(grid.CanPlace(BlockShapeTestFactory.SingleCell(), new Int2(3, 3)));
        }

        [Test]
        public void CanPlace_ReturnsFalse_WhenTargetCellIsAlreadyOccupied()
        {
            var grid = new GridModel(8, 8);
            grid.Place(BlockShapeTestFactory.SingleCell(), new Int2(3, 3));

            Assert.IsFalse(grid.CanPlace(BlockShapeTestFactory.SingleCell(), new Int2(3, 3)));
        }

        [Test]
        public void CanPlace_ReturnsFalse_WhenShapeExtendsOutsideGridBounds()
        {
            var grid = new GridModel(8, 8);

            Assert.IsFalse(grid.CanPlace(BlockShapeTestFactory.Horizontal(3), new Int2(6, 0)));
        }

        [Test]
        public void Place_WithOccupiedTargetCell_DoesNotModifyGridAndReturnsFailure()
        {
            var grid = new GridModel(8, 8);
            grid.Place(BlockShapeTestFactory.SingleCell(), new Int2(0, 0));

            var result = grid.Place(BlockShapeTestFactory.SingleCell(), new Int2(0, 0));

            Assert.IsFalse(result.Success);
            Assert.IsTrue(grid.IsCellOccupied(new Int2(0, 0)));
        }

        [Test]
        public void IsCellOccupied_ReturnsTrue_AfterPlacingBlockOnThatCell()
        {
            var grid = new GridModel(8, 8);

            grid.Place(BlockShapeTestFactory.SingleCell(), new Int2(2, 2));

            Assert.IsTrue(grid.IsCellOccupied(new Int2(2, 2)));
        }

        [Test]
        public void Place_FillingEntireRow_ClearsThatRow()
        {
            var grid = new GridModel(8, 8);
            for (int x = 0; x < 7; x++)
                grid.Place(BlockShapeTestFactory.SingleCell(), new Int2(x, 0));

            var result = grid.Place(BlockShapeTestFactory.SingleCell(), new Int2(7, 0));

            Assert.IsTrue(result.Success);
            CollectionAssert.AreEqual(new[] { 0 }, result.Clears.ClearedRows);
            for (int x = 0; x < 8; x++)
                Assert.IsFalse(grid.IsCellOccupied(new Int2(x, 0)), $"Row cell ({x},0) should be cleared.");
        }

        [Test]
        public void Place_FillingTwoRowsSimultaneously_ClearsBothRows()
        {
            var grid = new GridModel(8, 8);
            for (int y = 0; y < 2; y++)
                for (int x = 0; x < 7; x++)
                    grid.Place(BlockShapeTestFactory.SingleCell(), new Int2(x, y));

            var verticalPair = new BlockShapeDefinition("vertical-pair", new[] { new Int2(0, 0), new Int2(0, 1) });
            var result = grid.Place(verticalPair, new Int2(7, 0));

            Assert.IsTrue(result.Success);
            CollectionAssert.AreEquivalent(new[] { 0, 1 }, result.Clears.ClearedRows);
            Assert.AreEqual(2, result.Clears.TotalLinesCleared);
        }

        [Test]
        public void Place_FillingEntireColumn_ClearsThatColumn()
        {
            var grid = new GridModel(8, 8);
            for (int y = 0; y < 7; y++)
                grid.Place(BlockShapeTestFactory.SingleCell(), new Int2(0, y));

            var result = grid.Place(BlockShapeTestFactory.SingleCell(), new Int2(0, 7));

            Assert.IsTrue(result.Success);
            CollectionAssert.AreEqual(new[] { 0 }, result.Clears.ClearedColumns);
            for (int y = 0; y < 8; y++)
                Assert.IsFalse(grid.IsCellOccupied(new Int2(0, y)));
        }

        [Test]
        public void Place_FillingTwoColumnsSimultaneously_ClearsBothColumns()
        {
            var grid = new GridModel(8, 8);
            for (int x = 0; x < 2; x++)
                for (int y = 0; y < 7; y++)
                    grid.Place(BlockShapeTestFactory.SingleCell(), new Int2(x, y));

            var horizontalPair = new BlockShapeDefinition("horizontal-pair", new[] { new Int2(0, 0), new Int2(1, 0) });
            var result = grid.Place(horizontalPair, new Int2(0, 7));

            Assert.IsTrue(result.Success);
            CollectionAssert.AreEquivalent(new[] { 0, 1 }, result.Clears.ClearedColumns);
            Assert.AreEqual(2, result.Clears.TotalLinesCleared);
        }

        [Test]
        public void Place_FillingRowAndColumnAtOnce_ClearsBothSimultaneously()
        {
            var grid = new GridModel(8, 8);
            for (int x = 0; x < 7; x++)
                grid.Place(BlockShapeTestFactory.SingleCell(), new Int2(x, 0));
            for (int y = 1; y < 8; y++)
                grid.Place(BlockShapeTestFactory.SingleCell(), new Int2(7, y));

            var result = grid.Place(BlockShapeTestFactory.SingleCell(), new Int2(7, 0));

            Assert.IsTrue(result.Success);
            CollectionAssert.AreEqual(new[] { 0 }, result.Clears.ClearedRows);
            CollectionAssert.AreEqual(new[] { 7 }, result.Clears.ClearedColumns);
            Assert.AreEqual(2, result.Clears.TotalLinesCleared);
        }

        [Test]
        public void Reset_ClearsAllOccupiedCells()
        {
            var grid = new GridModel(8, 8);
            grid.Place(BlockShapeTestFactory.SingleCell(), new Int2(1, 1));
            grid.Place(BlockShapeTestFactory.SingleCell(), new Int2(2, 2));

            grid.Reset();

            for (int y = 0; y < grid.Height; y++)
                for (int x = 0; x < grid.Width; x++)
                    Assert.IsFalse(grid.IsCellOccupied(new Int2(x, y)));
        }
    }
}
