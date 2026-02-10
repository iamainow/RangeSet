# RangeSet

A high-performance, generic range set library for .NET with support for union, intersection, and difference operations. Works with any comparable type including integers, dates, and custom types.

## Features

- **Generic Design**: Works with any type implementing `IComparable<T>`, `IEquatable<T>`, `IMinMaxValue<T>`, and arithmetic operators
- **High Performance**: Uses `Span<T>` and `ref struct` for zero-allocation operations
- **AOT Compatible**: Fully compatible with .NET Native AOT compilation
- **Type Safe**: Compile-time type checking with no boxing for value types
- **Set Operations**: Union, Except (subtraction), and Intersect operations on range sets

## Supported Types

- Numeric types: `uint`, `int`, `ulong`, `long`, etc.
- Date/Time: `DateTime`, `DateTimeOffset`
- Custom types implementing required interfaces

## Installation

```bash
dotnet add package RangeSet
```

## Quick Start

```csharp
using RangeSet;

// Create ranges of unsigned integers
var range1 = new RangeArrayGeneric<uint>();
var range2 = new RangeArrayGeneric<uint>();

// Union two range sets
var union = range1.Union(range2, 1u);

// Subtract one range set from another
var difference = range1.Except(range2, 1u);

// Find intersection
var intersection = range1.Intersect(range2);
```

## Core Types

### `RangeArrayGeneric<T>`

The main range set struct that stores a normalized collection of non-overlapping, non-adjacent ranges.

```csharp
public readonly ref struct RangeArrayGeneric<T>
    where T : unmanaged, IEquatable<T>, IComparable<T>, 
              IMinMaxValue<T>, IAdditionOperators<T, T, T>, 
              ISubtractionOperators<T, T, T>
```

**Operations:**
- `Union()` - Combine two range sets
- `Except()` - Subtract one range set from another
- `Intersect()` - Find common ranges between two sets
- `ToArray()` - Convert to array of `CustomRange<T>`
- `ToReadOnlySpan()` - Access underlying span

### `CustomRange<T>`

Represents a single inclusive range from `First` to `Last`.

```csharp
public readonly struct CustomRange<T>
    where T : struct, IEquatable<T>, IComparable<T>
```

**Example:**
```csharp
var range = new CustomRange<uint>(1, 100);
Console.WriteLine(range); // "1 - 100"
```

### `SpanHelperGeneric`

Low-level helper class for range operations on spans. Provides methods for:
- `UnionNormalizedNormalized()` - Union of two normalized range sets
- `ExceptNormalizedSorted()` - Difference between normalized and sorted ranges
- `IntersectNormalizedNormalized()` - Intersection of two normalized range sets
- `MakeNormalizedFromUnsorted()` - Normalize unsorted ranges
- `Sort()` - Sort ranges by start value

## Type Requirements

To use a custom type with `RangeArrayGeneric<T>`, it must implement:

```csharp
public readonly struct MyType : 
    IEquatable<MyType>, 
    IComparable<MyType>,
    IMinMaxValue<MyType>,
    IAdditionOperators<MyType, MyType, MyType>,
    ISubtractionOperators<MyType, MyType, MyType>
{
    public static MyType MaxValue => ...;
    public static MyType MinValue => ...;
    // ... other interface implementations
}
```

## Performance

The library is optimized for high-performance scenarios:

- **Zero-allocation**: Uses `Span<T>` and stack allocation where possible
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
