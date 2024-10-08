namespace BotLife.Application.Bot;

public interface IBotParametersProvider
{
    /// <summary>
    /// Initial energy of the bot.
    /// </summary>
    int GetEnergy();
    
    /// <summary>
    /// Factor used to calculate the age of the bot.
    /// The age of the bot is calculated based on (number of cycles) / (year cycles).
    /// </summary>
    int GetYearCycles();

    /// <summary>
    /// Maximum age of the bot.
    /// </summary>
    int GetMaxAge();
    
    /// <summary>
    /// Minimum energy required to clone.
    /// </summary>
    int GetCloneMinEnergy();
    
    /// <summary>
    /// Minimum age required to clone.
    /// </summary>
    int GetCloneMinAge();
    
    /// <summary>
    /// Maximum age required to clone.
    /// </summary>
    int GetCloneMaxAge();
}