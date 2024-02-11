namespace BotLife.Application.Bot.MuBot;

public class PsiBotParametersProvider : IBotParametersProvider
{
    public int GetEnergy() => 100;

    public int GetAgeFactor() => 1000;
}