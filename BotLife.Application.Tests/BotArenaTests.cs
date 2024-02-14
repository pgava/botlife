using System.Collections;
using BotLife.Application.Bot;
using BotLife.Application.Arena;
using FluentAssertions;

namespace BotLife.Application.Tests;

public class BotArenaTests : BotLifeTestBase
{
    private readonly IArena _sut;

    public BotArenaTests()
    {
        _sut = Arena;
    }

    [Theory]
    [ClassData(typeof(MoveToDataGenerator))]
    public void When_Given_A_Direction_Should_Be_Able_To_Find_Next_Move(
        Position from, Direction where, Position to)
    {
        _sut.BuildArena(TestConstants.MaxWidth, TestConstants.MaxHeight);
        var mu = CreateMuBot();
        _sut.MoveTo(mu, from, where).Should().Be(to);
    }

    [Fact]
    public void Should_Be_Able_To_Scan_And_Find_The_Bot_In_Proximity()
    {
        _sut.BuildArena(TestConstants.MaxWidth, TestConstants.MaxHeight);
        var mu = CreateMuBot();
        var psi = CreatePsiBot();

        var muPosition = new Position(5, 5);
        var psiPosition = new Position(4, 5);
        _sut.AddBotAtPosition(mu, muPosition);
        _sut.AddBotAtPosition(psi, psiPosition);

        var events = _sut.Scan(mu, muPosition, 3);

        var enumerable = events as Event[] ?? events.ToArray();
        enumerable.Should().HaveCount(1);
        enumerable.Should().BeEquivalentTo(new[] { new Event(EventType.FoundPsi, mu, psi) });
    }

    [Fact]
    public void Should_Be_Able_To_Scan_And_Find_The_Bot_In_Same_Position()
    {
        _sut.BuildArena(TestConstants.MaxWidth, TestConstants.MaxHeight);
        var mu = CreateMuBot();
        var psi = CreatePsiBot();

        var muPosition = new Position(5, 5);
        var psiPosition = new Position(5, 5);
        _sut.AddBotAtPosition(mu, muPosition);
        _sut.AddBotAtPosition(psi, psiPosition);

        var events = _sut.Scan(mu, muPosition, 3);

        var enumerable = events as Event[] ?? events.ToArray();
        enumerable.Should().HaveCount(1);
        enumerable.Should().BeEquivalentTo(new[] { new Event(EventType.FoundPsi, mu, psi) });
    }

    [Fact]
    public void Should_Be_Able_To_Move_Bot()
    {
        _sut.BuildArena(TestConstants.MaxWidth, TestConstants.MaxHeight);
        var mu = CreateMuBot();
        var from = new Position(5, 5);
        _sut.AddBotAtPosition(mu, from);
        _sut.MoveTo(mu, from, Direction.Down);

        var bots = _sut.GetBotsAt(new Position(5, 6));
        bots.Should().Contain(mu);
    }

    [Fact]
    public void Should_Be_Able_To_Remove_Bot_From_Map()
    {
        _sut.BuildArena(TestConstants.MaxWidth, TestConstants.MaxHeight);
        var mu = CreateMuBot();
        var psi = CreatePsiBot();
        var position = new Position(5, 5);
        _sut.AddBotAtPosition(mu, position);
        _sut.AddBotAtPosition(psi, position);
        _sut.RemoveBot(psi);

        var bots = _sut.GetBotsAt(position);
        var enumerable = bots as IBot[] ?? bots.ToArray();
        enumerable.Count().Should().Be(1);
        enumerable.Should().Contain(mu);
    }

}

public class MoveToDataGenerator : IEnumerable<object[]>
{
    private readonly List<object[]> _data =
    [
        [new Position(0, 0), Direction.Up, new Position(0, 0)],
        [new Position(0, 0), Direction.Down, new Position(0, 1)],
        [new Position(0, 0), Direction.Left, new Position(0, 0)],
        [new Position(0, 0), Direction.Right, new Position(1, 0)],
        [new Position(1, 1), Direction.Up, new Position(1, 0)],
        [new Position(1, 1), Direction.Left, new Position(0, 1)],
    ];

    public IEnumerator<object[]> GetEnumerator() => _data.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

public static class TestConstants
{
    public const int MaxHeight = 30;
    public const int MaxWidth = 50;
}
