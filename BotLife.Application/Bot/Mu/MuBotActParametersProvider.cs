namespace BotLife.Application.Bot.Mu;

public class MuBotActParametersProvider : IBotActParametersProvider
{
    public int GetEnergy() => 100;

    public int GetScanArea() => 3;

    public int GetStepFrequency() => 10;

    /// <summary>
    /// 300 cycles to age 1 year => 1 cycle = 100ms => 1 year = 30s => 20 years => 10 minutes
    /// Let's assume that the bot lives 10 minutes.
    /// </summary>
    public int GetAgeFactor() => 300;

    public int GetMaxAge() => 4;

}