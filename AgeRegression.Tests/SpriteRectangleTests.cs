using AgeRegression.Data;
using FluentAssertions;

namespace AgeRegression.Tests;

public sealed class SpriteRectangleTests
{
    [Theory]
    [InlineData(0, 0, 0)]
    [InlineData(1, 64, 0)]
    [InlineData(2, 128, 0)]
    [InlineData(13, 0, 64)]
    [InlineData(14, 64, 64)]
    [InlineData(7, 448, 0)]
    public void SpriteIndex_ToRectangle_Using64x64(int spriteIndex, int expectedX, int expectedY)
    {
        // For 64x64 sprites in a 13-column grid
        var column = spriteIndex % 13;
        var row = spriteIndex / 13;
        var x = column * 64;
        var y = row * 64;

        x.Should().Be(expectedX);
        y.Should().Be(expectedY);
    }

    [Theory]
    [InlineData(0, 0, 0)]
    [InlineData(1, 16, 0)]
    [InlineData(2, 32, 0)]
    [InlineData(16, 0, 16)]
    [InlineData(32, 0, 32)]
    [InlineData(7, 112, 0)]
    public void SpriteIndex_ToRectangle_Using16x16(int spriteIndex, int expectedX, int expectedY)
    {
        // For 16x16 sprites in a standard Stardew object grid
        var column = spriteIndex % 16;
        var row = spriteIndex / 16;
        var x = column * 16;
        var y = row * 16;

        x.Should().Be(expectedX);
        y.Should().Be(expectedY);
    }
}