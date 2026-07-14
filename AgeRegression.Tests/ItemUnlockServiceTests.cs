using System.Collections.Generic;
using System.Linq;
using AgeRegression.Data;
using AgeRegression.Items;
using AgeRegression.Tests;
using static AgeRegression.Tests.UnlockTestHelpers;
using AgeRegression.Utilities;
using FluentAssertions;

namespace AgeRegression.Tests;

// ---------------------------------------------------------------------------
// ItemUnlockService tests
// ---------------------------------------------------------------------------

public sealed class ItemUnlockServiceTests
{
    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private static UnlockRequirement Requirement(params IUnlockCondition[] conditions) =>
        new() { Conditions = conditions.ToList() };

    private static ResolvedWardrobeItem MakeDiaper(UnlockRequirement? unlock = null)
    {
        var def = new DiaperTypeData
        {
            Id     = "test_diaper",
            Unlock = unlock ?? new UnlockRequirement()
        };
        return ResolvedWardrobeItem.FromDiaper(def);
    }

    private static ResolvedWardrobeItem MakeAccessory(UnlockRequirement? unlock = null)
    {
        var def = new WardrobeItemData
        {
            Id     = "test_accessory",
            Slot   = "test",
            Unlock = unlock ?? new UnlockRequirement()
        };
        return ResolvedWardrobeItem.FromAccessory(def);
    }

    // -------------------------------------------------------------------------
    // No unlock requirement (always unlocked)
    // -------------------------------------------------------------------------

    [Fact]
    public void IsUnlocked_NoConditions_ReturnsTrue()
    {
        var service = new ItemUnlockService(new StubGameState());
        service.IsUnlocked(MakeDiaper(new UnlockRequirement())).Should().BeTrue();
    }

    [Fact]
    public void IsUnlocked_NoUnlockMetadata_DefaultsToTrue()
    {
        var service = new ItemUnlockService(new StubGameState());
        service.IsUnlocked(MakeDiaper()).Should().BeTrue();
    }

    [Fact]
    public void IsUnlocked_AccessoryNoConditions_ReturnsTrue()
    {
        var service = new ItemUnlockService(new StubGameState());
        service.IsUnlocked(MakeAccessory(new UnlockRequirement())).Should().BeTrue();
    }

    [Fact]
    public void IsUnlocked_EmptyConditionsList_ReturnsTrue()
    {
        var service = new ItemUnlockService(new StubGameState());
        service.IsUnlocked(MakeDiaper(Requirement())).Should().BeTrue();
    }

    // -------------------------------------------------------------------------
    // Year condition
    // -------------------------------------------------------------------------

    [Fact]
    public void IsUnlocked_YearCondition_UnlockedWhenCurrentYearMeetsRequirement()
    {
        var service = new ItemUnlockService(new StubGameState(year: 2));
        service.IsUnlocked(MakeDiaper(Requirement(new YearCondition(2)))).Should().BeTrue();
    }

    [Fact]
    public void IsUnlocked_YearCondition_UnlockedWhenCurrentYearExceedsRequirement()
    {
        var service = new ItemUnlockService(new StubGameState(year: 3));
        service.IsUnlocked(MakeDiaper(Requirement(new YearCondition(2)))).Should().BeTrue();
    }

    [Fact]
    public void IsUnlocked_YearCondition_LockedWhenCurrentYearBelowRequirement()
    {
        var service = new ItemUnlockService(new StubGameState(year: 1));
        service.IsUnlocked(MakeDiaper(Requirement(new YearCondition(2)))).Should().BeFalse();
    }

    // -------------------------------------------------------------------------
    // Season condition
    // -------------------------------------------------------------------------

    [Fact]
    public void IsUnlocked_SeasonCondition_UnlockedWhenSeasonMatches()
    {
        var service = new ItemUnlockService(new StubGameState(season: "summer"));
        service.IsUnlocked(MakeDiaper(Requirement(new SeasonCondition("summer")))).Should().BeTrue();
    }

    [Fact]
    public void IsUnlocked_SeasonCondition_CaseInsensitiveMatch()
    {
        var service = new ItemUnlockService(new StubGameState(season: "Summer"));
        service.IsUnlocked(MakeDiaper(Requirement(new SeasonCondition("summer")))).Should().BeTrue();
    }

    [Fact]
    public void IsUnlocked_SeasonCondition_LockedWhenSeasonDoesNotMatch()
    {
        var service = new ItemUnlockService(new StubGameState(season: "spring"));
        service.IsUnlocked(MakeDiaper(Requirement(new SeasonCondition("winter")))).Should().BeFalse();
    }

    // -------------------------------------------------------------------------
    // Year + Season combined (both must be met)
    // -------------------------------------------------------------------------

    [Fact]
    public void IsUnlocked_YearAndSeason_UnlockedWhenBothMet()
    {
        var service = new ItemUnlockService(new StubGameState(year: 2, season: "summer"));
        service.IsUnlocked(
            MakeDiaper(Requirement(new YearCondition(2), new SeasonCondition("summer"))))
            .Should().BeTrue();
    }

    [Fact]
    public void IsUnlocked_YearAndSeason_LockedWhenYearNotMet()
    {
        var service = new ItemUnlockService(new StubGameState(year: 1, season: "summer"));
        service.IsUnlocked(
            MakeDiaper(Requirement(new YearCondition(2), new SeasonCondition("summer"))))
            .Should().BeFalse();
    }

    [Fact]
    public void IsUnlocked_YearAndSeason_LockedWhenSeasonNotMet()
    {
        var service = new ItemUnlockService(new StubGameState(year: 2, season: "spring"));
        service.IsUnlocked(
            MakeDiaper(Requirement(new YearCondition(2), new SeasonCondition("summer"))))
            .Should().BeFalse();
    }

    [Fact]
    public void IsUnlocked_YearAndSeason_LockedWhenNeitherMet()
    {
        var service = new ItemUnlockService(new StubGameState(year: 1, season: "spring"));
        service.IsUnlocked(
            MakeDiaper(Requirement(new YearCondition(2), new SeasonCondition("summer"))))
            .Should().BeFalse();
    }

    // -------------------------------------------------------------------------
    // Accessory unlock
    // -------------------------------------------------------------------------

    [Fact]
    public void IsUnlocked_AccessoryWithYearCondition_EvaluatesCorrectly()
    {
        var service = new ItemUnlockService(new StubGameState(year: 2));
        service.IsUnlocked(MakeAccessory(Requirement(new YearCondition(2)))).Should().BeTrue();
    }

    // -------------------------------------------------------------------------
    // Friendship condition
    // -------------------------------------------------------------------------

    [Fact]
    public void IsUnlocked_NoFriendshipCondition_ReturnsTrue()
    {
        var service = new ItemUnlockService(new StubGameState());
        service.IsUnlocked(MakeDiaper(new UnlockRequirement())).Should().BeTrue();
    }

    [Fact]
    public void IsUnlocked_FriendshipAndSeasonBothRequired_UnlockedWhenBothMet()
    {
        var state   = new StubGameState(season: "summer", friendship: new() { ["Penny"] = 1000 });
        var service = new ItemUnlockService(state);
        var item    = MakeDiaper(Requirement(
            new SeasonCondition("summer"), new FriendshipCondition("Penny", 1000)));

        service.IsUnlocked(item).Should().BeTrue();
    }

    [Fact]
    public void IsUnlocked_FriendshipAndSeasonBothRequired_LockedWhenFriendshipTooLow()
    {
        var state   = new StubGameState(season: "summer", friendship: new() { ["Penny"] = 500 });
        var service = new ItemUnlockService(state);
        var item    = MakeDiaper(Requirement(
            new SeasonCondition("summer"), new FriendshipCondition("Penny", 1000)));

        service.IsUnlocked(item).Should().BeFalse();
    }

    [Fact]
    public void IsUnlocked_FriendshipAndSeasonBothRequired_LockedWhenSeasonWrong()
    {
        var state   = new StubGameState(season: "spring", friendship: new() { ["Penny"] = 1000 });
        var service = new ItemUnlockService(state);
        var item    = MakeDiaper(Requirement(
            new SeasonCondition("summer"), new FriendshipCondition("Penny", 1000)));

        service.IsUnlocked(item).Should().BeFalse();
    }

    [Fact]
    public void IsUnlocked_YearSeasonAndFriendshipAllRequired_UnlockedWhenAllMet()
    {
        var state   = new StubGameState(year: 2, season: "summer", friendship: new() { ["Penny"] = 1000 });
        var service = new ItemUnlockService(state);
        var item    = MakeDiaper(Requirement(
            new YearCondition(2), new SeasonCondition("summer"), new FriendshipCondition("Penny", 1000)));

        service.IsUnlocked(item).Should().BeTrue();
    }

    [Fact]
    public void IsUnlocked_YearSeasonAndFriendshipAllRequired_LockedWhenFriendshipTooLow()
    {
        var state   = new StubGameState(year: 2, season: "summer", friendship: new() { ["Penny"] = 500 });
        var service = new ItemUnlockService(state);
        var item    = MakeDiaper(Requirement(
            new YearCondition(2), new SeasonCondition("summer"), new FriendshipCondition("Penny", 1000)));

        service.IsUnlocked(item).Should().BeFalse();
    }

    [Fact]
    public void IsUnlocked_YearSeasonAndFriendshipAllRequired_LockedWhenSeasonWrong()
    {
        var state   = new StubGameState(year: 2, season: "spring", friendship: new() { ["Penny"] = 1000 });
        var service = new ItemUnlockService(state);
        var item    = MakeDiaper(Requirement(
            new YearCondition(2), new SeasonCondition("summer"), new FriendshipCondition("Penny", 1000)));

        service.IsUnlocked(item).Should().BeFalse();
    }

    [Fact]
    public void IsUnlocked_FriendshipMet_ReturnsTrue()
    {
        var state   = new StubGameState(friendship: new() { ["Penny"] = 1000 });
        var service = new ItemUnlockService(state);
        var item    = MakeDiaper(Requirement(new FriendshipCondition("Penny", 1000)));

        service.IsUnlocked(item).Should().BeTrue();
    }

    [Fact]
    public void IsUnlocked_FriendshipExceeded_ReturnsTrue()
    {
        var state   = new StubGameState(friendship: new() { ["Penny"] = 2000 });
        var service = new ItemUnlockService(state);
        var item    = MakeDiaper(Requirement(new FriendshipCondition("Penny", 1000)));

        service.IsUnlocked(item).Should().BeTrue();
    }

    [Fact]
    public void IsUnlocked_FriendshipBelowRequired_ReturnsFalse()
    {
        var state   = new StubGameState(friendship: new() { ["Penny"] = 500 });
        var service = new ItemUnlockService(state);
        var item    = MakeDiaper(Requirement(new FriendshipCondition("Penny", 1000)));

        service.IsUnlocked(item).Should().BeFalse();
    }

    [Fact]
    public void IsUnlocked_FriendshipNpcUnknown_ReturnsFalse()
    {
        // NPC not in friendship data → GetFriendshipPoints returns null → locked
        var state   = new StubGameState(friendship: new());
        var service = new ItemUnlockService(state);
        var item    = MakeDiaper(Requirement(new FriendshipCondition("Penny", 1000)));

        service.IsUnlocked(item).Should().BeFalse();
    }

    [Fact]
    public void IsUnlocked_FriendshipNpcMismatch_ReturnsFalse()
    {
        var state   = new StubGameState(friendship: new() { ["Abigail"] = 1000 });
        var service = new ItemUnlockService(state);
        var item    = MakeDiaper(Requirement(new FriendshipCondition("Penny", 1000)));

        service.IsUnlocked(item).Should().BeFalse();
    }

    [Fact]
    public void IsUnlocked_FriendshipExplicitRequirementWithMissingData_ReturnsFalse()
    {
        var state   = new StubGameState(friendship: null);
        var service = new ItemUnlockService(state);
        var item    = MakeDiaper(Requirement(new FriendshipCondition("Penny", 1000)));

        service.IsUnlocked(item).Should().BeFalse();
    }

    [Fact]
    public void IsUnlocked_FriendshipAndYearBothRequired_LockedWhenOnlyFriendshipMet()
    {
        var state   = new StubGameState(year: 1, friendship: new() { ["Penny"] = 1000 });
        var service = new ItemUnlockService(state);
        var item    = MakeDiaper(Requirement(
            new YearCondition(2), new FriendshipCondition("Penny", 1000)));

        service.IsUnlocked(item).Should().BeFalse();
    }

    [Fact]
    public void IsUnlocked_FriendshipAndYearBothRequired_UnlockedWhenBothMet()
    {
        var state   = new StubGameState(year: 2, friendship: new() { ["Penny"] = 1000 });
        var service = new ItemUnlockService(state);
        var item    = MakeDiaper(Requirement(
            new YearCondition(2), new FriendshipCondition("Penny", 1000)));

        service.IsUnlocked(item).Should().BeTrue();
    }

    // -------------------------------------------------------------------------
    // Mail flag condition
    // -------------------------------------------------------------------------

    [Fact]
    public void IsUnlocked_MailFlagConditionMet_ReturnsTrue()
    {
        var state   = new StubGameState(mailFlags: new HashSet<string> { "mail_test" });
        var service = new ItemUnlockService(state);
        var item    = MakeDiaper(Requirement(new MailFlagCondition("mail_test")));

        service.IsUnlocked(item).Should().BeTrue();
    }

    [Fact]
    public void IsUnlocked_MailFlagConditionMissing_ReturnsFalse()
    {
        var state   = new StubGameState(mailFlags: new HashSet<string>());
        var service = new ItemUnlockService(state);
        var item    = MakeDiaper(Requirement(new MailFlagCondition("mail_test")));

        service.IsUnlocked(item).Should().BeFalse();
    }

    [Fact]
    public void IsUnlocked_MailFlagAndYearBothRequired_UnlockedWhenBothMet()
    {
        var state   = new StubGameState(year: 2, mailFlags: new HashSet<string> { "mail_test" });
        var service = new ItemUnlockService(state);
        var item    = MakeDiaper(Requirement(
            new YearCondition(2), new MailFlagCondition("mail_test")));

        service.IsUnlocked(item).Should().BeTrue();
    }

    [Fact]
    public void IsUnlocked_NoMailCondition_KeepsExistingBehavior()
    {
        var service = new ItemUnlockService(new StubGameState());
        var item    = MakeDiaper(new UnlockRequirement());

        service.IsUnlocked(item).Should().BeTrue();
    }

    // -------------------------------------------------------------------------
    // ShopStockProvider integration
    // -------------------------------------------------------------------------

    [Fact]
    public void ShopStockProvider_ExcludesLockedItems()
    {
        const string diapers = """
            [
              { "Id": "year1_diaper", "Price": 50, "ShopAvailable": true },
              { "Id": "year2_diaper", "Price": 80, "ShopAvailable": true,
                "Unlock": { "Conditions": [ { "Type": "Year", "Value": 2 } ] } }
            ]
            """;

        var loader   = BuildLoader(diaperJson: diapers);
        var service  = new ItemUnlockService(new StubGameState(year: 1));
        var provider = new ShopStockProvider(loader, service);

        var stock = provider.GetAvailableStock().ToList();

        stock.Should().HaveCount(1);
        stock[0].Item.ItemId.Should().Be("year1_diaper");
    }

    [Fact]
    public void ShopStockProvider_IncludesItemsOnceUnlocked()
    {
        const string diapers = """
            [
              { "Id": "year2_diaper", "Price": 80, "ShopAvailable": true,
                "Unlock": { "Conditions": [ { "Type": "Year", "Value": 2 } ] } }
            ]
            """;

        var loader   = BuildLoader(diaperJson: diapers);
        var service  = new ItemUnlockService(new StubGameState(year: 2));
        var provider = new ShopStockProvider(loader, service);

        provider.GetAvailableStock().Should().HaveCount(1);
    }

    [Fact]
    public void ShopStockProvider_ShopAvailableFalseStillExcludesUnlockedItems()
    {
        const string diapers = """
            [
              { "Id": "hidden", "Price": 50, "ShopAvailable": false }
            ]
            """;

        var loader   = BuildLoader(diaperJson: diapers);
        var service  = new ItemUnlockService(new StubGameState());
        var provider = new ShopStockProvider(loader, service);

        provider.GetAvailableStock().Should().BeEmpty();
    }

    [Fact]
    public void ShopStockProvider_UnlockAndShopAvailableBothRequiredForStock()
    {
        const string diapers = """
            [
              { "Id": "available_unlocked",  "Price": 50, "ShopAvailable": true  },
              { "Id": "available_locked",     "Price": 50, "ShopAvailable": true,
                "Unlock": { "Conditions": [ { "Type": "Year", "Value": 3 } ] } },
              { "Id": "unavailable_unlocked", "Price": 50, "ShopAvailable": false }
            ]
            """;

        var loader   = BuildLoader(diaperJson: diapers);
        var service  = new ItemUnlockService(new StubGameState(year: 1));
        var provider = new ShopStockProvider(loader, service);

        var stock = provider.GetAvailableStock().ToList();

        stock.Should().HaveCount(1);
        stock[0].Item.ItemId.Should().Be("available_unlocked");
    }

    [Fact]
    public void ShopStockProvider_SeasonUnlock_ExcludesOutOfSeasonItems()
    {
        const string diapers = """
            [
              { "Id": "summer_diaper", "Price": 80, "ShopAvailable": true,
                "Unlock": { "Conditions": [ { "Type": "Season", "Value": "summer" } ] } }
            ]
            """;

        var loader   = BuildLoader(diaperJson: diapers);
        var service  = new ItemUnlockService(new StubGameState(season: "spring"));
        var provider = new ShopStockProvider(loader, service);

        provider.GetAvailableStock().Should().BeEmpty();
    }

    [Fact]
    public void ShopStockProvider_SeasonUnlock_IncludesInSeasonItems()
    {
        const string diapers = """
            [
              { "Id": "summer_diaper", "Price": 80, "ShopAvailable": true,
                "Unlock": { "Conditions": [ { "Type": "Season", "Value": "summer" } ] } }
            ]
            """;

        var loader   = BuildLoader(diaperJson: diapers);
        var service  = new ItemUnlockService(new StubGameState(season: "summer"));
        var provider = new ShopStockProvider(loader, service);

        provider.GetAvailableStock().Should().HaveCount(1);
    }

    // -------------------------------------------------------------------------
    // Conditions list (new path)
    // -------------------------------------------------------------------------

    [Fact]
    public void IsUnlocked_EmptyRequirement_AlwaysUnlocked()
    {
        var service = new ItemUnlockService(new StubGameState());
        service.IsUnlocked(MakeDiaper(new UnlockRequirement())).Should().BeTrue();
    }

    [Fact]
    public void IsUnlocked_ConditionsList_YearConditionMet_ReturnsTrue()
    {
        var unlock = new UnlockRequirement
        {
            Conditions = [new YearCondition(2)]
        };
        var service = new ItemUnlockService(new StubGameState(year: 2));
        service.IsUnlocked(MakeDiaper(unlock)).Should().BeTrue();
    }

    [Fact]
    public void IsUnlocked_ConditionsList_YearConditionNotMet_ReturnsFalse()
    {
        var unlock = new UnlockRequirement
        {
            Conditions = [new YearCondition(3)]
        };
        var service = new ItemUnlockService(new StubGameState(year: 1));
        service.IsUnlocked(MakeDiaper(unlock)).Should().BeFalse();
    }

    [Fact]
    public void IsUnlocked_ConditionsList_MailFlagConditionMet_ReturnsTrue()
    {
        var unlock = new UnlockRequirement
        {
            Conditions = [new MailFlagCondition("flag_test")]
        };
        var state   = new StubGameState(mailFlags: new HashSet<string> { "flag_test" });
        var service = new ItemUnlockService(state);
        service.IsUnlocked(MakeDiaper(unlock)).Should().BeTrue();
    }

    [Fact]
    public void IsUnlocked_ConditionsList_FriendshipConditionMet_ReturnsTrue()
    {
        var unlock = new UnlockRequirement
        {
            Conditions = [new FriendshipCondition("Penny", 1000)]
        };
        var state   = new StubGameState(friendship: new() { ["Penny"] = 1000 });
        var service = new ItemUnlockService(state);
        service.IsUnlocked(MakeDiaper(unlock)).Should().BeTrue();
    }

    // -------------------------------------------------------------------------
    // Phase 3: ItemUnlockService consumes the condition pipeline
    // -------------------------------------------------------------------------

    [Fact]
    public void IsUnlocked_ConditionListUsed_EvaluatesEachCondition()
    {
        // The service must drive evaluation through UnlockConditionEvaluator
        // using the Conditions list.
        var unlock = new UnlockRequirement
        {
            Conditions = [new YearCondition(2), new MailFlagCondition("flag_p3")]
        };
        var state   = new StubGameState(year: 2, mailFlags: new HashSet<string> { "flag_p3" });
        var service = new ItemUnlockService(state);

        service.IsUnlocked(MakeDiaper(unlock)).Should().BeTrue();
    }

    [Fact]
    public void IsUnlocked_MultipleConditions_AreAndEvaluated()
    {
        var unlock = new UnlockRequirement
        {
            Conditions =
            [
                new YearCondition(2),
                new SeasonCondition("summer"),
                new MailFlagCondition("flag_p3")
            ]
        };
        var state = new StubGameState(
            year: 2, season: "summer", mailFlags: new HashSet<string> { "flag_p3" });
        var service = new ItemUnlockService(state);

        service.IsUnlocked(MakeDiaper(unlock)).Should().BeTrue();
    }

    [Fact]
    public void IsUnlocked_FailedConditionBlocksUnlock()
    {
        // Year is met but the mail-flag condition fails → overall locked.
        var unlock = new UnlockRequirement
        {
            Conditions =
            [
                new YearCondition(1),
                new MailFlagCondition("missing_flag")
            ]
        };
        var state   = new StubGameState(year: 3, mailFlags: new HashSet<string>());
        var service = new ItemUnlockService(state);

        service.IsUnlocked(MakeDiaper(unlock)).Should().BeFalse();
    }

    [Fact]
    public void IsUnlocked_FirstFailingConditionShortCircuits()
    {
        // Even though the second condition would pass, the first failing
        // condition must lock the item (AND semantics).
        var unlock = new UnlockRequirement
        {
            Conditions =
            [
                new MailFlagCondition("missing_flag"),
                new SeasonCondition("summer")
            ]
        };
        var state   = new StubGameState(season: "summer", mailFlags: new HashSet<string>());
        var service = new ItemUnlockService(state);

        service.IsUnlocked(MakeDiaper(unlock)).Should().BeFalse();
    }
}
