# RangeSet

A high-performance, generic range set library for .NET with support for union, intersection, and difference operations. Works with any numeric type including integers and custom types.

The library provides two main implementations:
- **`ArrayRangeSet<T>`**: A heap-allocated class for general-purpose use
- **`SpanRangeSet<T>`**: A `ref struct` for high-performance, low-allocation scenarios

## Features

- **Dual Implementation**: Provides both heap-allocated (`ArrayRangeSet<T>`) and low-allocation ref struct (`SpanRangeSet<T>`) implementations
- **Generic Design**: Works with any unmanaged type implementing `IComparable<T>`, `IEquatable<T>`, `IMinMaxValue<T>`, and increment/decrement operators
- **High Performance**: Uses `Span<T>` and `ref struct` for low-allocation, efficient operations
- **AOT Compatible**: Fully compatible with .NET Native AOT compilation
- **Type Safe**: Compile-time type checking with no boxing for value types
- **Set Operations**: Union, Except (subtraction), and Intersect operations on range sets

## Supported Types

- Integer types: `byte`, `sbyte`, `short`, `ushort`, `int`, `uint`, `long`, `ulong`, `Int128`, `UInt128`
- Custom `unmanaged` types implementing the required interfaces

## Requirements

- .NET 8, 9, or 10

## Installation

```bash
dotnet add package RangeSet
```

## Quick Start

```csharp
using RangeSet;

// Example 1: Using ArrayRangeSet (heap-allocated)
var range1 = new ArrayRangeSet<uint>(new Range<uint>[] { new(1, 5), new(10, 20) });
var range2 = new ArrayRangeSet<uint>(new Range<uint>[] { new(3, 12) });

// Union two range sets → [1-20]
var union = range1.Union(range2);

// Subtract one range set from another → [1-2, 13-20]
var difference = range1.Except(range2);

// Find intersection → [3-5, 10-12]
var intersection = range1.Intersect(range2);

// Example 2: Using SpanRangeSet (low-allocation, stack-friendly)
Span<Range<uint>> buffer1 = stackalloc Range<uint>[2];
buffer1[0] = new Range<uint>(1, 5);
buffer1[1] = new Range<uint>(10, 20);
var spanRange1 = new SpanRangeSet<uint>(buffer1);

Span<Range<uint>> buffer2 = stackalloc Range<uint>[1];
buffer2[0] = new Range<uint>(3, 12);
var spanRange2 = new SpanRangeSet<uint>(buffer2);

// Union requires a caller-provided result buffer (enables zero-allocation)
int unionSize = SpanRangeSet.CalculateUnionSize(spanRange1.RangesCount, spanRange2.RangesCount);
Span<Range<uint>> unionBuffer = stackalloc Range<uint>[unionSize];
var spanUnion = spanRange1.Union(spanRange2, unionBuffer);

// Except and Intersect allocate the result buffer internally
var spanDiff = spanRange1.Except(spanRange2);
var spanIntersect = spanRange1.Intersect(spanRange2);
```

## Core Types

### Choosing Between ArrayRangeSet and SpanRangeSet

| Feature | `ArrayRangeSet<T>` | `SpanRangeSet<T>` |
|---------|-------------------|-------------------|
| **Allocation** | Heap-allocated | `Union` is zero-allocation with caller-provided buffer; `Except`/`Intersect` allocate result internally |
| **Lifetime** | Managed by GC | Must not escape defining scope |
| **Performance** | Good general performance | Lower GC pressure; best for temporary operations |
| **Use Case** | General purpose, longer lifetime | High-performance scenarios, hot paths |
| **Creation** | `new ArrayRangeSet<T>(ranges)` | `new SpanRangeSet<T>(spanOfRanges)` |

### `ArrayRangeSet<T>`

A heap-allocated range set class that stores a normalized collection of non-overlapping, non-adjacent ranges.

```csharp
public class ArrayRangeSet<T>
    where T : unmanaged, IEquatable<T>, IComparable<T>, 
              IMinMaxValue<T>, IIncrementOperators<T>, IDecrementOperators<T>
```

**Operations:**
- `Union()` - Combine two range sets
- `Except()` - Subtract one range set from another
- `Intersect()` - Find common ranges between two sets
- `ToArray()` - Convert to array of `Range<T>`
- `ToReadOnlySpan()` - Access underlying span
- `RangesCount` - Get the number of ranges in the set

### `SpanRangeSet<T>`

A low-allocation range set ref struct for high-performance scenarios. Cannot escape its defining scope.

```csharp
public readonly ref struct SpanRangeSet<T>
    where T : unmanaged, IEquatable<T>, IComparable<T>, 
              IMinMaxValue<T>, IIncrementOperators<T>, IDecrementOperators<T>
```

Create using the constructor (the provided span is normalized in-place):
```csharp
Span<Range<uint>> buffer = stackalloc Range<uint>[10];
// ... populate buffer
var rangeSet = new SpanRangeSet<uint>(buffer);
```

**Operations:**
- `Union(other, resultBuffer)` - Combine two range sets into a caller-provided buffer (zero-allocation)
- `Except(other)` - Subtract one range set from another (allocates result internally)
- `Intersect(other)` - Find common ranges between two sets (allocates result internally)
- `ToArray()` - Convert to array of `Range<T>`
- `ToReadOnlySpan()` - Access underlying span
- `RangesCount` - Get the number of ranges in the set

### `SpanRangeSet` (static helper)

Provides buffer size calculation helpers for use with `SpanRangeSet<T>`:

```csharp
int unionSize     = SpanRangeSet.CalculateUnionSize(set1.RangesCount, set2.RangesCount);
int exceptSize    = SpanRangeSet.CalculateExceptSize(set1.RangesCount, set2.RangesCount);
int intersectSize = SpanRangeSet.CalculateIntersectSize(set1.RangesCount, set2.RangesCount);
```

Use these to allocate (or stackalloc) the correct buffer size before calling operations.

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

Low-level static helper class for range operations on spans. Provides methods for:
- `UnionNormalizedNormalized()` - Union of two normalized range sets
- `ExceptNormalizedSorted()` - Difference between a normalized and a sorted range set
- `IntersectNormalizedNormalized()` - Intersection of two normalized range sets
- `NormalizeUnsorted()` - Sort and normalize an unsorted span of ranges in-place; returns the new length
- `NormalizeSorted()` - Normalize a sorted (but possibly overlapping/adjacent) span in-place; returns the new length
- `Sort()` - Sort ranges by start value
- `CalcIntersectBufferLength()` - Calculate the required result buffer length for an intersection

## Type Requirements

Both `ArrayRangeSet<T>` and `SpanRangeSet<T>` require the same type constraints. To use a custom type, it must be an unmanaged type implementing:

```csharp
public readonly unmanaged struct MyType : 
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

- **Low-allocation**: Uses `Span<T>` for efficient processing; `SpanRangeSet<T>.Union` supports fully zero-allocation operation via caller-provided buffers
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
