namespace BotLife.Application.Bot;

public interface IBotParametersProvider
{
    /// <summary>
    /// Initial energy of the bot.
    /// </summary>
    int GetEnergy();
    
    /// <summary>
    /// Factor used to calculate the age of the bot.
    /// The age of the bot is calculated based on (number of cycles) / (age factor).
    /// </summary>
    int GetAgeFactor();
}