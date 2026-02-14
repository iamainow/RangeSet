# RangeSet

A high-performance, generic range set library for .NET with support for union, intersection, and difference operations. Works with any numeric type including integers, floating-point numbers, and custom types.

## Features

- **Generic Design**: Works with any type implementing `IComparable<T>`, `IEquatable<T>`, `IMinMaxValue<T>`, and increment/decrement operators
- **High Performance**: Uses `Span<T>` and `ref struct` for low-allocation, efficient operations
- **AOT Compatible**: Fully compatible with .NET Native AOT compilation
- **Type Safe**: Compile-time type checking with no boxing for value types
- **Set Operations**: Union, Except (subtraction), and Intersect operations on range sets

## Supported Types

- Integer types: `byte`, `sbyte`, `short`, `ushort`, `int`, `uint`, `long`, `ulong`, `Int128`, `UInt128`
- Floating-point types: `Half`, `float`, `double`, `decimal`
- Custom `unmanaged` types implementing the required interfaces

## Installation

```bash
dotnet add package RangeSet
```

## Quick Start

```csharp
using RangeSet;

// Create ranges
var range1 = RangeSortedSet.Create<uint>(new Range<uint>[] { (1, 5), (10, 20) });
var range2 = RangeSortedSet.Create<uint>(new Range<uint>[] { (3, 12) });

// Union two range sets → [1-20]
var union = range1.Union(range2);

// Subtract one range set from another → [1-2]
var difference = range1.Except(range2);

// Find intersection → [3-5, 10-12]
var intersection = range1.Intersect(range2);
```

## Core Types

### `RangeSortedSet<T>`

The main range set struct that stores a normalized collection of non-overlapping, non-adjacent ranges.

```csharp
public readonly ref struct RangeSortedSet<T>
    where T : unmanaged, IEquatable<T>, IComparable<T>, 
              IMinMaxValue<T>, IIncrementOperators<T>, IDecrementOperators<T>
```

**Operations:**
- `Union()` - Combine two range sets
- `Except()` - Subtract one range set from another
- `Intersect()` - Find common ranges between two sets
- `ToArray()` - Convert to array of `Range<T>`
- `ToReadOnlySpan()` - Access underlying span

### `Range<T>`

Represents a single inclusive range from `First` to `Last`.

```csharp
public readonly struct Range<T>
    where T : struct, IEquatable<T>, IComparable<T>
```

**Example:**
```csharp
var range = new Range<uint>(1, 100);
Console.WriteLine(range); // "1 - 100"
```

### `RangeOperations`

Low-level helper class for range operations on spans. Provides methods for:
- `UnionNormalizedNormalized()` - Union of two normalized range sets
- `ExceptNormalizedSorted()` - Difference between normalized and sorted ranges
- `IntersectNormalizedNormalized()` - Intersection of two normalized range sets
- `MakeNormalizedFromUnsorted()` - Normalize unsorted ranges
- `Sort()` - Sort ranges by start value

## Type Requirements

To use a custom type with `RangeSortedSet<T>`, it must implement:

```csharp
public readonly struct MyType : 
    IEquatable<MyType>, 
    IComparable<MyType>,
    IMinMaxValue<MyType>,
    IIncrementOperators<MyType>,
    IDecrementOperators<MyType>
{
    public static MyType MaxValue => ...;
    public static MyType MinValue => ...;
    // ... other interface implementations
}
```

## Performance

The library is optimized for high-performance scenarios:

- **Low-allocation**: Uses `Span<T>` for efficient processing with minimal heap allocations
- **Normalized storage**: Ranges are always stored sorted, non-overlapping, and non-adjacent
- **Efficient algorithms**: O(n) union, intersection, and difference operations
- **AOT ready**: No reflection or dynamic code generation

Benchmarks show excellent performance for range operations on large datasets.

## Building

```bash
dotnet build --configuration Release
```

## Testing

```bash
dotnet test
```

## Benchmarks

Run benchmarks with:

```bash
dotnet run --project RangeSet.Benchmarks -c Release
```

## License

MIT License - see [LICENSE](LICENSE) file for details.

## Contributing

Contributions are welcome! Please ensure your code follows the existing patterns and passes all tests.
