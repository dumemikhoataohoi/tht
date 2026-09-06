using GemGrid.Configuration;
using GemGrid.Core;
using GemGrid.Gameplay;
using NUnit.Framework;

namespace GemGrid.Tests.EditMode
{
    public class GameOverCheckerTests
    {
        private static BlockData Block(BlockShapeDefinition shape) => new BlockData("id", shape);

        /// <summary>
        /// Occupies the two diagonal cells of a 2x2 grid ((0,0) and (1,1)) via two
        /// separate single-cell placements. Neither placement completes a row or column
        /// (each row/column only ever has one of its two cells filled), so nothing gets
        /// auto-cleared — unlike filling 3+ cells of a 2x2 grid, which always completes
        /// at least one line by pigeonhole and would be wiped immediately by GridModel.
        /// The two remaining free cells, (1,0) and (0,1), are diagonal to each other
        /// (not horizontally adjacent), so a 2-cell horizontal shape cannot occupy both.
        /// </summary>
        private static GridModel CreateGridWithOnlyDiagonalGapsFree()
        {
            var grid = new GridModel(2, 2);
            grid.Place(BlockShapeTestFactory.SingleCell(), new Int2(0, 0));
            grid.Place(BlockShapeTestFactory.SingleCell(), new Int2(1, 1));
            return grid;
        }

        [Test]
        public void HasAnyValidMove_ReturnsTrue_WhenGridIsEmpty()
        {
            var grid = new GridModel(8, 8);
            var tray = new[] { Block(BlockShapeTestFactory.SingleCell()) };

            Assert.IsTrue(GameOverChecker.HasAnyValidMove(grid, tray));
        }

        [Test]
        public void HasAnyValidMove_ReturnsFalse_WhenRemainingGapsAreTooSmallOrMisshapenForEveryTrayShape()
        {
            var grid = CreateGridWithOnlyDiagonalGapsFree();
            var tray = new[] { Block(BlockShapeTestFactory.Horizontal(2)) };

            Assert.IsFalse(GameOverChecker.HasAnyValidMove(grid, tray));
        }

        [Test]
        public void HasAnyValidMove_ReturnsTrue_WhenOnlyIsolatedSingleCellGapsRemain()
        {
            var grid = CreateGridWithOnlyDiagonalGapsFree();
            var tray = new[] { Block(BlockShapeTestFactory.SingleCell()) };

            Assert.IsTrue(GameOverChecker.HasAnyValidMove(grid, tray));
        }

        [Test]
        public void HasAnyValidMove_IgnoresEmptyTraySlots()
        {
            var grid = new GridModel(8, 8);
            var tray = new[] { BlockData.Empty };

            Assert.IsFalse(GameOverChecker.HasAnyValidMove(grid, tray));
        }

        [Test]
        public void IsGameOver_ReturnsTrue_WhenNoShapeInTrayFitsAnywhere()
        {
            var grid = CreateGridWithOnlyDiagonalGapsFree();
            var tray = new[] { Block(BlockShapeTestFactory.Horizontal(2)) };

            Assert.IsTrue(GameOverChecker.IsGameOver(grid, tray));
        }

        [Test]
        public void IsGameOver_ReturnsFalse_WhenAtLeastOneShapeFits()
        {
            var grid = new GridModel(8, 8);
            var tray = new[] { Block(BlockShapeTestFactory.SingleCell()) };

            Assert.IsFalse(GameOverChecker.IsGameOver(grid, tray));
        }
    }
}
