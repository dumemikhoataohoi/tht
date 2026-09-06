namespace GemGrid.Journey
{
    /// <summary>Level objective kinds — see GAME_DESIGN_V2.md §7.</summary>
    public enum ObjectiveType
    {
        /// <summary>Reach a total score within the attempt.</summary>
        Score,

        /// <summary>Clear a total number of rows/columns within the attempt.</summary>
        LinesCleared,

        /// <summary>Reach a combo streak of at least N at some point.</summary>
        Combo,

        /// <summary>Clear 2+ lines in a single placement, N separate times.</summary>
        MultiClear,

        /// <summary>Fully empty the 3-slot tray (triggering a refill) N times.</summary>
        FillSlot,

        /// <summary>Successfully place at least N blocks before running out of moves.</summary>
        Survive
    }
}
