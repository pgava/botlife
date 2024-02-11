namespace BotLife.Application.Bot;

public interface IBotActParametersProvider : IBotParametersProvider
{
    /// <summary>
    /// Scan area size.
    /// </summary>
    int GetScanArea();

    /// <summary>
    /// Step frequency. The bot will move every "n" cycles.
    /// </summary>
    public int GetStepFrequency();
}