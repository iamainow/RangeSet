# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Commands

```bash
# Build
dotnet build --configuration Release

# Run all tests
dotnet test

# Run a single test class
dotnet test --filter "ClassName=RangeSet.Test.ArrayRangeSetTests"

# Run a single test method
dotnet test --filter "FullyQualifiedName~ArrayRangeSetTests.Union_ReturnsCorrectResult"

# Run benchmarks (Release mode required)
dotnet run --project RangeSet.Benchmarks -c Release

# Pack NuGet package
dotnet pack RangeSet/RangeSet.csproj --configuration Release
```

## Architecture

This is a .NET library (multi-targeted: `net8.0;net9.0;net10.0`) providing high-performance generic range set operations. Three projects:

- **`RangeSet/`** - The library (NuGet package)
- **`RangeSet.Test/`** - xUnit tests
- **`RangeSet.Benchmarks/`** - BenchmarkDotNet benchmarks

### Core types (all in `RangeSet/` namespace)

- **`Range<T>`** - Immutable inclusive range `[First, Last]`. Type constraint: `struct, IEquatable<T>, IComparable<T>`.
- **`RangeOperations`** - Static low-level algorithms operating on `Span<Range<T>>`. All operations require non-overlapping inputs. Key distinction in naming: `Unsorted` (arbitrary), `Sorted` (sorted but may overlap/be adjacent), `Normalized` (sorted, non-overlapping, non-adjacent).
- **`ArrayRangeSet<T>`** - Heap-allocated class wrapping a normalized `Range<T>[]`. Operations return new instances.
- **`SpanRangeSet<T>`** - `ref struct` wrapping a caller-provided `Span<Range<T>>`. Constructor normalizes the span in-place. `Union` requires a caller-provided result buffer; `Except`/`Intersect` allocate internally.
- **`SpanList<T>`** - Public `ref struct` list backed by a `Span<T>`. Used by `RangeOperations` to build results into pre-allocated buffers without heap allocation.
- **`RangeComparer<T>`** - Singleton `IComparer<Range<T>>` used for sorting.

### Type constraint pattern

`ArrayRangeSet<T>` and `SpanRangeSet<T>` require: `unmanaged, IEquatable<T>, IComparable<T>, IMinMaxValue<T>, IIncrementOperators<T>, IDecrementOperators<T>`. The `IMinMaxValue<T>` constraint is needed for boundary handling in union (to avoid overflow when checking adjacency at `MaxValue`).

### Dependencies

- **`CommunityToolkit.HighPerformance`** - Used in `SpanRangeSet<T>` for `MemoryOwner<T>` (pooled heap buffers) in `Except` and `Intersect` results.

### Build notes

`TreatWarningsAsErrors` is enabled globally (via `Directory.Build.props`). All analyzer warnings are errors — this includes CA/IDE rules at `AnalysisLevel=latest-all`. Suppress with `#pragma warning disable` or `[SuppressMessage]` only when necessary.

### Key design invariants

- Ranges stored in all set types are always normalized (sorted, non-overlapping, non-adjacent).
- `RangeOperations` result buffers must not overlap with either input span.
- `SpanRangeSet<T>` cannot escape its defining scope (ref struct restriction).
- `SpanRangeSet.CalculateUnionSize/ExceptSize/IntersectSize` are the safe way to size result buffers.
