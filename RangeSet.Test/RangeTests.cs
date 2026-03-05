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
    public void Constructor_UnsignedTypes_Success()
    {
        var byteRange = new Range<byte>(0, 255);
        var uintRange = new Range<uint>(0, 1000);
        var ulongRange = new Range<ulong>(0, ulong.MaxValue);
        
        Assert.Equal(0, byteRange.First);
        Assert.Equal(255, byteRange.Last);
        Assert.Equal(0u, uintRange.First);
        Assert.Equal(1000u, uintRange.Last);
    }

    [Fact]
    public void Constructor_SignedTypes_Success()
    {
        var sbyteRange = new Range<sbyte>(-128, 127);
        var shortRange = new Range<short>(-1000, 1000);
        var intRange = new Range<int>(int.MinValue, int.MaxValue);
        var longRange = new Range<long>(long.MinValue, long.MaxValue);
        
        Assert.Equal(-128, sbyteRange.First);
        Assert.Equal(127, sbyteRange.Last);
        Assert.Equal(int.MinValue, intRange.First);
        Assert.Equal(int.MaxValue, intRange.Last);
    }

    [Fact]
    public void Constructor_Int128_Success()
    {
        var range = new Range<Int128>(Int128.MinValue, Int128.MaxValue);
        
        Assert.Equal(Int128.MinValue, range.First);
        Assert.Equal(Int128.MaxValue, range.Last);
    }

    [Fact]
    public void Constructor_UInt128_Success()
    {
        var range = new Range<UInt128>(0, UInt128.MaxValue);

        Assert.Equal((UInt128)0, range.First);
        Assert.Equal(UInt128.MaxValue, range.Last);
    }

    [Fact]
    public void Equals_Int128Ranges_ReturnsTrue()
    {
        var range1 = new Range<Int128>(Int128.MinValue, 0);
        var range2 = new Range<Int128>(Int128.MinValue, 0);
        
        Assert.True(range1.Equals(range2));
        Assert.True(range1 == range2);
    }

    [Fact]
    public void Equals_UInt128Ranges_ReturnsTrue()
    {
        var range1 = new Range<UInt128>(0, UInt128.MaxValue / 2);
        var range2 = new Range<UInt128>(0, UInt128.MaxValue / 2);
        
        Assert.True(range1.Equals(range2));
        Assert.True(range1 == range2);
    }
}