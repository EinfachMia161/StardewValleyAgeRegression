using AgeRegression.UI;
using FluentAssertions;

namespace AgeRegression.Tests;

public sealed class SpriteReferenceTests
{
    [Fact]
    public void EncapsulatesSheetAndIndex()
    {
        var sprite = new SpriteReference("assets/sprites/accessories.png", 3);

        sprite.SpriteSheet.Should().Be("assets/sprites/accessories.png");
        sprite.SpriteIndex.Should().Be(3);
    }

    [Fact]
    public void DifferentCategories_KeepDistinctSheetPaths()
    {
        // A diaper and an accessory must preserve their own sheet so the
        // resolver can load each from a separate texture.
        var diaper = new SpriteReference("assets/sprites/diapers", 2);
        var accessory = new SpriteReference("assets/sprites/accessories", 4);

        diaper.SpriteSheet.Should().Be("assets/sprites/diapers");
        accessory.SpriteSheet.Should().Be("assets/sprites/accessories");
        diaper.SpriteSheet.Should().NotBe(accessory.SpriteSheet);
        diaper.SpriteIndex.Should().NotBe(accessory.SpriteIndex);
    }

    [Fact]
    public void IsImmutable()
    {
        var sprite = new SpriteReference("sheet", 1);

        var copy = sprite with { SpriteIndex = 9 };

        sprite.SpriteIndex.Should().Be(1);
        copy.SpriteIndex.Should().Be(9);
        copy.SpriteSheet.Should().Be("sheet");
    }
}
