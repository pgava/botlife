namespace BotLife.Application.Bot.MuBot;

public class MuBotActParametersProvider : IBotActParametersProvider
{
    public int GetEnergy() => 100;

    public int GetScanArea() => 3;

    public int GetStepFrequency() => 5;

    public int GetAgeFactor() => 1000;
}