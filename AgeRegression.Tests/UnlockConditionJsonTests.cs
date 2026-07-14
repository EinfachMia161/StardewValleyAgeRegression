using AgeRegression.Data;
using AgeRegression.Items;
using AgeRegression.Tests;
using static AgeRegression.Tests.UnlockTestHelpers;
using AgeRegression.Utilities;
using FluentAssertions;
using Newtonsoft.Json;

namespace AgeRegression.Tests;

// ---------------------------------------------------------------------------
// Phase 4: JSON migration support for unlock conditions
// ---------------------------------------------------------------------------

public sealed class UnlockConditionJsonTests
{
    // ---------------------------------------------------------------------
    // New JSON condition format loads correctly
    // ---------------------------------------------------------------------

    [Fact]
    public void Deserialize_NewConditionFormat_YearAndMailFlag()
    {
        const string json = """
            {
              "Conditions": [
                { "Type": "Year", "Value": 2 },
                { "Type": "MailFlag", "Flag": "example" }
              ]
            }
            """;

        var unlock = JsonConvert.DeserializeObject<UnlockRequirement>(
            json, new UnlockConditionJsonConverter());

        var conditions = unlock!.GetConditions().ToList();
        conditions.Should().HaveCount(2);
        conditions[0].Should().BeOfType<YearCondition>()
            .Which.RequiredYear.Should().Be(2);
        conditions[1].Should().BeOfType<MailFlagCondition>()
            .Which.MailFlag.Should().Be("example");
    }

    [Fact]
    public void Deserialize_NewConditionFormat_TypeDiscriminatorCaseInsensitive()
    {
        const string json = """
            { "Conditions": [ { "type": "year", "Value": 3 } ] }
            """;

        var unlock = JsonConvert.DeserializeObject<UnlockRequirement>(
            json, new UnlockConditionJsonConverter());

        unlock!.GetConditions().Should().ContainSingle()
            .Which.Should().BeOfType<YearCondition>()
            .Which.RequiredYear.Should().Be(3);
    }

    // ---------------------------------------------------------------------
    // Multiple condition entries load correctly
    // ---------------------------------------------------------------------

    [Fact]
    public void Deserialize_MultipleConditions_AllMapped()
    {
        const string json = """
            {
              "Conditions": [
                { "Type": "Year", "Value": 2 },
                { "Type": "Season", "Value": "summer" },
                { "Type": "Friendship", "Npc": "Penny", "Points": 1000 },
                { "Type": "MailFlag", "Flag": "flag_x" }
              ]
            }
            """;

        var unlock = JsonConvert.DeserializeObject<UnlockRequirement>(
            json, new UnlockConditionJsonConverter());

        var conditions = unlock!.GetConditions().ToList();
        conditions.Should().HaveCount(4);
        conditions.OfType<YearCondition>().Should().ContainSingle(c => c.RequiredYear == 2);
        conditions.OfType<SeasonCondition>().Should().ContainSingle(c => c.RequiredSeason == "summer");
        conditions.OfType<FriendshipCondition>().Should().ContainSingle(
            c => c.NpcName == "Penny" && c.RequiredPoints == 1000);
        conditions.OfType<MailFlagCondition>().Should().ContainSingle(c => c.MailFlag == "flag_x");
    }

    [Fact]
    public void Deserialize_SeasonCondition_AcceptsSeasonAlias()
    {
        const string json = """
            { "Conditions": [ { "Type": "Season", "Season": "fall" } ] }
            """;

        var unlock = JsonConvert.DeserializeObject<UnlockRequirement>(
            json, new UnlockConditionJsonConverter());

        unlock!.GetConditions().Should().ContainSingle()
            .Which.Should().BeOfType<SeasonCondition>()
            .Which.RequiredSeason.Should().Be("fall");
    }

    // ---------------------------------------------------------------------
    // Missing Conditions leaves the list null
    // ---------------------------------------------------------------------

    [Fact]
    public void Deserialize_MissingConditions_LeavesListNull()
    {
        const string json = """{}""";

        var unlock = JsonConvert.DeserializeObject<UnlockRequirement>(
            json, new UnlockConditionJsonConverter());

        unlock!.Conditions.Should().BeNull();
        unlock.GetConditions().Should().BeEmpty();
    }

    // ---------------------------------------------------------------------
    // End-to-end loading through the DataLoader pipeline
    // ---------------------------------------------------------------------

    [Fact]
    public void DataLoader_NewConditionFormat_EvaluatesThroughPipeline()
    {
        const string diapers = """
            [
              {
                "Id": "cond_item", "Price": 80, "ShopAvailable": true,
                "Unlock": {
                  "Conditions": [
                    { "Type": "Year", "Value": 2 },
                    { "Type": "Season", "Value": "summer" }
                  ]
                }
              }
            ]
            """;

        var item = LoadDiaper(diapers, "cond_item");

        new ItemUnlockService(new StubGameState(year: 2, season: "summer"))
            .IsUnlocked(item).Should().BeTrue();
        new ItemUnlockService(new StubGameState(year: 2, season: "spring"))
            .IsUnlocked(item).Should().BeFalse();
        new ItemUnlockService(new StubGameState(year: 1, season: "summer"))
            .IsUnlocked(item).Should().BeFalse();
    }
}
