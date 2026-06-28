using AgeRegression.Data;
using AgeRegression.Dialogue;
using FluentAssertions;

namespace AgeRegression.Tests;

public sealed class DialogueConditionEvaluatorTests
{
    private readonly DialogueConditionEvaluator _evaluator = new();

    [Fact]
    public void NullConditions_AlwaysReturnsTrue()
    {
        _evaluator.Evaluate(null, MakeContext()).Should().BeTrue();
    }

    [Fact]
    public void EmptyConditions_AlwaysReturnsTrue()
    {
        _evaluator.Evaluate(new DialogueConditions(), MakeContext())
            .Should().BeTrue();
    }

    [Fact]
    public void RegressionStage_MatchingStage_ReturnsTrue()
    {
        var ctx  = MakeContext(stageId: "little");
        var cond = new DialogueConditions
        {
            RegressionStages = new List<string> { "little", "middle" }
        };
        _evaluator.Evaluate(cond, ctx).Should().BeTrue();
    }

    [Fact]
    public void RegressionStage_NonMatchingStage_ReturnsFalse()
    {
        var ctx  = MakeContext(stageId: "none");
        var cond = new DialogueConditions
        {
            RegressionStages = new List<string> { "little", "baby" }
        };
        _evaluator.Evaluate(cond, ctx).Should().BeFalse();
    }

    [Fact]
    public void MinFriendship_NotMet_ReturnsFalse()
    {
        var ctx  = MakeContext(friendshipHearts: 2);
        var cond = new DialogueConditions { MinFriendshipHearts = 4 };
        _evaluator.Evaluate(cond, ctx).Should().BeFalse();
    }

    [Fact]
    public void IsMarried_RequiredButNotMet_ReturnsFalse()
    {
        var ctx  = MakeContext(isMarried: false);
        var cond = new DialogueConditions { IsMarried = true };
        _evaluator.Evaluate(cond, ctx).Should().BeFalse();
    }

    [Fact]
    public void Season_NotMatching_ReturnsFalse()
    {
        var ctx  = MakeContext(season: "winter");
        var cond = new DialogueConditions
        {
            Seasons = new List<string> { "spring" }
        };
        _evaluator.Evaluate(cond, ctx).Should().BeFalse();
    }

    [Fact]
    public void TimeRange_BeforeRange_ReturnsFalse()
    {
        var ctx  = MakeContext(timeOfDay: 600);
        var cond = new DialogueConditions { TimeFrom = 800 };
        _evaluator.Evaluate(cond, ctx).Should().BeFalse();
    }

    [Fact]
    public void IsWearingDiaper_RequiredButNotWearing_ReturnsFalse()
    {
        var ctx  = MakeContext(isWearingDiaper: false);
        var cond = new DialogueConditions { IsWearingDiaper = true };
        _evaluator.Evaluate(cond, ctx).Should().BeFalse();
    }

    [Fact]
    public void ContinenceThreshold_Matching_ReturnsTrue()
    {
        var ctx  = MakeContext(continenceThresholdId: "warning");
        var cond = new DialogueConditions
        {
            ContinenceThresholds = new List<string> { "warning", "struggling" }
        };
        _evaluator.Evaluate(cond, ctx).Should().BeTrue();
    }

    [Fact]
    public void HungerRange_AboveMax_ReturnsFalse()
    {
        var ctx  = MakeContext(hungerNormalized: 0.8f);
        var cond = new DialogueConditions { MaxHungerNormalized = 0.5f };
        _evaluator.Evaluate(cond, ctx).Should().BeFalse();
    }

    [Fact]
    public void MultipleConditions_OneFails_ReturnsFalse()
    {
        var ctx = MakeContext(
            stageId: "little",
            friendshipHearts: 2,
            season: "spring",
            isWearingDiaper: true);

        var cond = new DialogueConditions
        {
            RegressionStages    = new List<string> { "little" },
            MinFriendshipHearts = 4,
            Seasons             = new List<string> { "spring" },
            IsWearingDiaper     = true
        };

        _evaluator.Evaluate(cond, ctx).Should().BeFalse();
    }

    private static DialogueContext MakeContext(
        string stageId = "none",
        int friendshipHearts = 0,
        bool isMarried = false,
        string season = "spring",
        int timeOfDay = 1000,
        string weather = "sunny",
        string locationName = "Farm",
        bool isWearingDiaper = false,
        string diaperConditionId = "none",
        float continenceNormalized = 1f,
        string continenceThresholdId = "comfortable",
        float hungerNormalized = 1f,
        float thirstNormalized = 1f,
        float comfortNormalized = 1f,
        string[]? personalityTags = null)
    {
        return new DialogueContext
        {
            RegressionStageId     = stageId,
            FriendshipHearts      = friendshipHearts,
            IsMarried             = isMarried,
            Season                = season,
            TimeOfDay             = timeOfDay,
            Weather               = weather,
            LocationName          = locationName,
            IsWearingDiaper       = isWearingDiaper,
            DiaperConditionId     = diaperConditionId,
            ContinenceNormalized  = continenceNormalized,
            ContinenceThresholdId = continenceThresholdId,
            HungerNormalized      = hungerNormalized,
            ThirstNormalized      = thirstNormalized,
            ComfortNormalized     = comfortNormalized,
            NpcPersonalityTags    = personalityTags ?? Array.Empty<string>(),
            EquippedAccessories   = new HashSet<string>(),
            GameFlags             = new HashSet<string>()
        };
    }
}
