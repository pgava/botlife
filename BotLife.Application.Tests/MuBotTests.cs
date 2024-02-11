using System.Collections;
using BotLife.Application.Bot;
using BotLife.Application.Arena;
using BotLife.Application.Bot.MuBot;
using BotLife.Application.Engine;
using FluentAssertions;

namespace BotLife.Application.Tests;

public class MuBotTests
{
    private readonly IArena _arena;


    public MuBotTests()
    {
        _arena = new BotLifeTestHelper().Arena;
    }

    [Fact]
    public void Bot_Should_Move()
    {
        var sut = new MuBot(_arena, new FakeActParametersProvider());
        sut.SetPosition(new Position(1, 1));
        sut.Next();
        sut.Next();
        sut.IsTimeToMove().Should().BeTrue();
    }

    [Fact]
    public void Bot_When_Is_Not_Time_To_Move_Should_Not_Scan()
    {
        var sut = new MuBot(_arena, new FakeActParametersProvider());
        sut.SetPosition(new Position(1, 1));
        sut.Next();
        sut.Scan().Should().BeEquivalentTo(new List<Event>());
    }

    [Fact]
    public void Bot_When_Is_Not_Time_To_Move_Should_Not_React()
    {
        var sut = new MuBot(_arena, new FakeActParametersProvider());
        sut.SetPosition(new Position(1, 1));
        sut.Next();
        sut.React(new List<Event>()).Should().BeEquivalentTo(new Act(Event.Empty, ActType.None));
    }

    [Fact]
    public void Bot_When_Is_Not_Time_To_Move_Should_Not_Run()
    {
        var sut = new MuBot(_arena, new FakeActParametersProvider());
        sut.SetPosition(new Position(1, 1));
        sut.Next();
    }

    [Fact]
    public void Bot_Should_Lose_Energy_While_Walking()
    {
        var sut = new MuBot(_arena, new FakeActParametersProvider());
        sut.SetPosition(new Position(1, 1));
        sut.Next();
        sut.WalkEnergy().Should().Be(0.0301);
    }

    [Fact]
    public void Bot_Should_Lose_Energy_Every_Cycle()
    {
        var sut = new MuBot(_arena, new FakeActParametersProvider());
        sut.SetPosition(new Position(1, 1));
        sut.Next();
        sut.CycleEnergy().Should().Be(0.0151);
    }
}