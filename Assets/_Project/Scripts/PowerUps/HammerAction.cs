using GemGrid.Core;
using GemGrid.Gameplay;

namespace GemGrid.PowerUps
{
    /// <summary>Hammer power-up: destroys one chosen cell. Only legal while Playing — see GAME_DESIGN_V2.md §9.</summary>
    public static class HammerAction
    {
        public static bool Apply(GameManager game, Int2 cell)
        {
            if (game == null || game.State != GameStateType.Playing) return false;
            return game.Grid.ClearCell(cell);
        }
    }
}
