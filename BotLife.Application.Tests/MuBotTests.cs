using BotLife.Application.Arena;
using BotLife.Application.Bot;
using BotLife.Application.Bot.Mu;
using FluentAssertions;
using Moq;
using Xunit.Abstractions;

namespace BotLife.Application.Tests;

public class MuBotTests : BotLifeTestBase
{
    private readonly ITestOutputHelper _testOutputHelper;
    private readonly IArena _arena = new Mock<IArena>().Object;

    public MuBotTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    public void Bot_Should_Move()
    {
        var sut = CreateMuBot();

        sut.Next();sut.Next();

        sut.IsTimeToMove().Should().BeTrue();
    }

    [Fact]
    public void Bot_When_Is_Not_Time_To_Move_Should_Not_Scan()
    {
        var sut = CreateMuBot();

        sut.Next();

        sut.Scan().Should().BeEquivalentTo(new List<Event>());
    }

    [Fact]
    public void Bot_When_Is_Not_Time_To_Move_Should_Not_React()
    {
        var sut = CreateMuBot();

        sut.Next();

        sut.ChooseAction(new List<Event>()).Should().BeEquivalentTo(new Act(Event.Empty, ActType.None));
    }

    [Fact]
    public void Bot_When_Is_Not_Time_To_Move_Should_Not_Run()
    {
        var sut = CreateMuBot();

        sut.Next();

        sut.IsTimeToMove().Should().BeFalse();
    }

    [Fact]
    public void Bot_Should_Lose_Energy_While_Walking()
    {
        var sut = CreateMuBot();

        sut.Next();

        sut.WalkEnergy().Should().Be(0.1505);
    }

    [Fact]
    public void Bot_Should_Lose_Energy_Every_Cycle()
    {
        var sut = CreateMuBot();

        sut.Next();

        sut.CycleEnergy().Should().Be(0.3466);
    }

    [Fact(Skip = "Long running test")]
    public void Track_Bot_Energy()
    {
        var sut = CreateMuBot();
        sut.SetPosition(new Position(1, 1));

        int cycles = 0;
        while (sut.IsAlive() && cycles < 300)
        {
            sut.Next();
            cycles++;
            _testOutputHelper.WriteLine($"Bot Energy, Age, CE, WE: {sut.Energy}, {sut.Age}, {sut.CycleEnergy()}, {sut.WalkEnergy()}");
        }
    }

    protected override MuBot CreateMuBot()
    {
        var mu = new MuBot(Logger, Mediator, Randomizer(), _arena, ActParameters());
        mu.SetPosition(new Position(1, 1));
        return mu;
    }

}