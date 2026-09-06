namespace GemGrid.Configuration
{
    public interface IGameplayConfigProvider
    {
        ScoreRules ScoreRules { get; }
        ComboRules ComboRules { get; }
    }
}
