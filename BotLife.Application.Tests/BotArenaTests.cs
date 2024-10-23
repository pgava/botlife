using System.Collections;
using BotLife.Contracts;
using FluentAssertions;

namespace BotLife.Application.Tests;

public class BotArenaTests : BotLifeTestBase
{

    [Theory]
    [ClassData(typeof(MoveToDataGenerator))]
    public void When_Given_A_Direction_Should_Be_Able_To_Find_Next_Move(Position from, Direction where, Position to)
    {
        var mu = AddMuBotAt(from);

        Arena.MoveTo(mu, from, where).Should().Be(to);
    }

    [Fact]
    public void Should_Be_Able_To_Scan_And_Find_The_Bot_In_Proximity()
    {
        var mu = AddMuBotAt(Position.At(5, 5));
        var psi = AddPsiBotAt(Position.At(4, 5));

        var events = Arena.Scan(mu, Position.At(5, 5), 3);

        var enumerable = events as Event[] ?? events.ToArray();
        enumerable.Should().HaveCount(1);
        enumerable.Should().BeEquivalentTo(new[] { new Event(EventType.FoundPsi, mu, psi) });
    }

    [Fact]
    public void Should_Be_Able_To_Scan_And_Find_The_Bot_In_Same_Position()
    {
        var mu = AddMuBotAt(Position.At(5, 5));
        var psi = AddPsiBotAt(Position.At(5, 5));

        var events = Arena.Scan(mu, Position.At(5, 5), 3);

        var enumerable = events as Event[] ?? events.ToArray();
        enumerable.Should().HaveCount(1);
        enumerable.Should().BeEquivalentTo(new[] { new Event(EventType.FoundPsi, mu, psi) });
    }

    [Fact]
    public void Should_Be_Able_To_Move_Bot()
    {
        var mu = AddMuBotAt(Position.At(5, 5));

        Arena.MoveTo(mu, Position.At(5, 5), Direction.Down);

        var bots = Arena.GetBotsAt(Position.At(5, 6));
        bots.Should().Contain(mu);
    }

    [Fact]
    public void Should_Be_Able_To_Remove_Bot_From_Map()
    {
        var mu = AddMuBotAt(Position.At(5, 5));
        var psi = AddPsiBotAt(Position.At(5, 5));

        Arena.RemoveBot(psi);

        var bots = Arena.GetBotsAt(Position.At(5, 5));
        var enumerable = bots as IBot[] ?? bots.ToArray();
        enumerable.Count().Should().Be(1);
        enumerable.Should().Contain(mu);
    }

}

public class MoveToDataGenerator : IEnumerable<object[]>
{
    private readonly List<object[]> _data =
    [
        [Position.At(0, 0), Direction.Up, Position.At(0, 0)],
        [Position.At(0, 0), Direction.Down, Position.At(0, 1)],
        [Position.At(0, 0), Direction.Left, Position.At(0, 0)],
        [Position.At(0, 0), Direction.Right, Position.At(1, 0)],
        [Position.At(1, 1), Direction.Up, Position.At(1, 0)],
        [Position.At(1, 1), Direction.Left, Position.At(0, 1)],
    ];

    public IEnumerator<object[]> GetEnumerator() => _data.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
