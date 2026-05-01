using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Withat.Models;

/// <summary>
/// Обёртка над <see cref="ImmutableArray{T}"/> со структурным равенством для использования
/// в моделях incremental source generator (кеш pipeline сравнивает значения по шагам).
/// Создание: неявное приведение из <see cref="ImmutableArray{T}"/>; для <see cref="IEnumerable{T}"/> —
/// <see cref="CreateRange"/> (неявный оператор из интерфейса в C# запрещён).
/// </summary>
public readonly struct EquatableArray<T> : IEquatable<EquatableArray<T>>, IEnumerable<T>
{
    private readonly ImmutableArray<T> _array;

    private EquatableArray(ImmutableArray<T> array) =>
        _array = array.IsDefault ? ImmutableArray<T>.Empty : array;

    /// <summary>Пустая последовательность (то же, что <see cref="ImmutableArray{T}.Empty"/>).</summary>
    public static EquatableArray<T> Empty => ImmutableArray<T>.Empty;

    public static implicit operator EquatableArray<T>(ImmutableArray<T> array) => new(array);

    /// <summary>
    /// Материализует последовательность так же, как <see cref="ImmutableArray.CreateRange(System.Collections.Generic.IEnumerable{T})"/>.
    /// </summary>
    public static EquatableArray<T> CreateRange(IEnumerable<T> items) =>
        items is null ? throw new ArgumentNullException(nameof(items)) : ImmutableArray.CreateRange(items);

    public ImmutableArray<T> AsImmutable() => _array;

    public int Length => _array.Length;

    public bool IsDefaultOrEmpty => _array.IsDefaultOrEmpty;

    public bool Equals(EquatableArray<T> other) => SequenceEqual(_array, other._array);

    public override bool Equals(object? obj) => obj is EquatableArray<T> o && Equals(o);

    public override int GetHashCode()
    {
        if (_array.IsEmpty)
            return 0;

        unchecked
        {
            var hash = 17;
            var comparer = EqualityComparer<T>.Default;
            for (var i = 0; i < _array.Length; i++)
                hash = hash * 31 + comparer.GetHashCode(_array[i]!);
            return hash;
        }
    }

    public static bool operator ==(EquatableArray<T> left, EquatableArray<T> right) => left.Equals(right);

    public static bool operator !=(EquatableArray<T> left, EquatableArray<T> right) => !left.Equals(right);

    public IEnumerator<T> GetEnumerator()
    {
        var arr = _array;
        for (var i = 0; i < arr.Length; i++)
            yield return arr[i];
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    private static bool SequenceEqual(ImmutableArray<T> left, ImmutableArray<T> right)
    {
        if (left.IsDefault != right.IsDefault)
            return false;
        if (left.IsDefault)
            return true;
        if (left.Length != right.Length)
            return false;

        var comparer = EqualityComparer<T>.Default;
        for (var i = 0; i < left.Length; i++)
        {
            if (!comparer.Equals(left[i], right[i]))
                return false;
        }

        return true;
    }
}
