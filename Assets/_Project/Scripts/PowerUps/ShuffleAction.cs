using GemGrid.Core;
using GemGrid.Gameplay;

namespace GemGrid.PowerUps
{
    /// <summary>Shuffle power-up: deals a completely fresh 3-block tray without costing a turn. Only legal while Playing — see GAME_DESIGN_V2.md §9.</summary>
    public static class ShuffleAction
    {
        public static bool Apply(GameManager game)
        {
            if (game == null || game.State != GameStateType.Playing) return false;

            game.Spawner.RefillTray();
            game.CheckGameOverNow(); // defensive: an extremely unlucky fresh tray could theoretically still be stuck.
            return true;
        }
    }
}
