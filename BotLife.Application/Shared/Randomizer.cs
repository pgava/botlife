namespace BotLife.Application.Shared;

public interface IRandomizer
{
    int Rnd(int min, int max);
    void Shuffle<T>(T[] values);
}

public class Randomizer : IRandomizer
{
    public int Rnd(int min, int max)
    {
        Random random = new Random(Environment.TickCount);
        return random.Next(min, max);
    }

    public void Shuffle<T>(T[] values)
    {
        Random.Shared.Shuffle(values);
    }
}