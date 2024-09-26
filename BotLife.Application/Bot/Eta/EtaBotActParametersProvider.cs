namespace BotLife.Application.Bot.Eta;

public class EtaBotActParametersProvider : IBotActParametersProvider
{
    public int GetEnergy() => 200;

    public int GetScanArea() => 5;

    public int GetStepFrequency() => 10;

    /// <summary>
    /// 300 cycles to age 1 year => 1 cycle = 100ms => 1 year = 30s => 20 years => 10 minutes
    /// Let's assume that the bot lives 10 minutes.
    /// </summary>
    public int GetYearCycles() => 300;

    public int GetMaxAge() => 25;
    
    public int GetCloneMinEnergy() => 5;

    public int GetCloneMinAge() => 1;

    public int GetCloneMaxAge() => 4;
}