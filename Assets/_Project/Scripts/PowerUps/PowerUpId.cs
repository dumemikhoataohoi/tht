namespace GemGrid.PowerUps
{
    /// <summary>
    /// Full power-up roster per GAME_DESIGN_V2.md §9. Hint/Hammer/Shuffle/Undo have
    /// working effects this milestone (see HintFinder/HammerAction/ShuffleAction/UndoAction);
    /// Bomb/LineClear/DoubleScore are carried over from GAME_DESIGN.md v1 as catalog/inventory
    /// entries only (purchasable, ownable, saved) — their gameplay effects are a follow-up,
    /// per the implementation brief's explicit "Implement" vs "Kế thừa/giữ" distinction.
    /// </summary>
    public enum PowerUpId
    {
        Hint,
        Hammer,
        Shuffle,
        Undo,
        Bomb,
        LineClear,
        DoubleScore
    }
}
