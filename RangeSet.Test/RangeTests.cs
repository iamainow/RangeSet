namespace RangeSet.Tests;

using RangeSet;

public class RangeTests
{
    [Fact]
    public void Constructor_ValidRange_Success()
    {
        var range = new Range<int>(1, 10);
        
        Assert.Equal(1, range.First);
        Assert.Equal(10, range.Last);
    }

    [Fact]
    public void Constructor_InvalidRange_ThrowsException()
    {
        Assert.Throws<ArgumentException>(() => new Range<int>(10, 1));
    }

    [Fact]
    public void Constructor_SingleValueRange_Success()
    {
        var range = new Range<int>(5, 5);
        
        Assert.Equal(5, range.First);
        Assert.Equal(5, range.Last);
    }

    [Fact]
    public void ToString_ReturnsCorrectFormat()
    {
        var range = new Range<int>(1, 100);
        
        var result = range.ToString();
        
        Assert.Equal("1 - 100", result);
    }

    [Fact]
    public void Equals_SameRange_ReturnsTrue()
    {
        var range1 = new Range<int>(1, 10);
        var range2 = new Range<int>(1, 10);
        
        Assert.True(range1.Equals(range2));
        Assert.True(range1 == range2);
        Assert.True(range1.GetHashCode() == range2.GetHashCode());
    }

    [Fact]
    public void Equals_DifferentRange_ReturnsFalse()
    {
        var range1 = new Range<int>(1, 10);
        var range2 = new Range<int>(2, 10);
        var range3 = new Range<int>(1, 11);
        
        Assert.False(range1.Equals(range2));
        Assert.False(range1.Equals(range3));
        Assert.True(range1 != range2);
    }

    [Fact]
    public void Equals_DifferentTypes_ReturnsFalse()
    {
        var range = new Range<int>(1, 10);
        var obj = "not a range";
        
        Assert.False(range.Equals(obj));
    }

    [Fact]
    public void Equals_NullObject_ReturnsFalse()
    {
        var range = new Range<int>(1, 10);
        
        Assert.False(range.Equals(null));
    }

    [Fact]
    public void Constructor_UInt_Success()
    {
        var uintRange = new Range<uint>(0, 1000);

        Assert.Equal(0u, uintRange.First);
        Assert.Equal(1000u, uintRange.Last);
    }

    [Fact]
    public void Constructor_Int_BoundaryValues_Success()
    {
        var intRange = new Range<int>(int.MinValue, int.MaxValue);

        Assert.Equal(int.MinValue, intRange.First);
        Assert.Equal(int.MaxValue, intRange.Last);
    }
}