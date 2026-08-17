using System.Collections.Generic;

using System;
using System.Linq;
using UnityEngine;

public static class LinqExtension
{
    #region MinBy
    //https://github.com/morelinq/MoreLINQ/blob/ec4bbd3c7ca61e3a98695aaa2afb23da001ee420/MoreLinq/MinBy.cs
    /// <summary>
    /// Returns the minimal element of the given sequence, based on
    /// the given projection.
    /// </summary>
    /// <remarks>
    /// If more than one element has the minimal projected value, the first
    /// one encountered will be returned. This overload uses the default comparer
    /// for the projected type. This operator uses immediate execution, but
    /// only buffers a single result (the current minimal element).
    /// </remarks>
    /// <typeparam name="TSource">Type of the source sequence</typeparam>
    /// <typeparam name="TKey">Type of the projected element</typeparam>
    /// <param name="source">Source sequence</param>
    /// <param name="selector">Selector to use to pick the results to compare</param>
    /// <returns>The minimal element, according to the projection.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="selector"/> is null</exception>
    /// <exception cref="InvalidOperationException"><paramref name="source"/> is empty</exception>

    public static TSource MinBy<TSource, TKey>(this IEnumerable<TSource> source,
        Func<TSource, TKey> selector)
    {
        return source.MinBy(selector, null);
    }

    /// <summary>
    /// Returns the minimal element of the given sequence, based on
    /// the given projection and the specified comparer for projected values.
    /// </summary>
    /// <remarks>
    /// If more than one element has the minimal projected value, the first
    /// one encountered will be returned. This operator uses immediate execution, but
    /// only buffers a single result (the current minimal element).
    /// </remarks>
    /// <typeparam name="TSource">Type of the source sequence</typeparam>
    /// <typeparam name="TKey">Type of the projected element</typeparam>
    /// <param name="source">Source sequence</param>
    /// <param name="selector">Selector to use to pick the results to compare</param>
    /// <param name="comparer">Comparer to use to compare projected values</param>
    /// <returns>The minimal element, according to the projection.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/>, <paramref name="selector"/> 
    /// or <paramref name="comparer"/> is null</exception>
    /// <exception cref="InvalidOperationException"><paramref name="source"/> is empty</exception>

    public static TSource MinBy<TSource, TKey>(this IEnumerable<TSource> source,
        Func<TSource, TKey> selector, IComparer<TKey> comparer)
    {
        if (source == null) throw new ArgumentNullException("source");
        if (selector == null) throw new ArgumentNullException("selector");
        comparer = comparer ?? Comparer<TKey>.Default;

        using (var sourceIterator = source.GetEnumerator())
        {
            if (!sourceIterator.MoveNext())
            {
                throw new InvalidOperationException("Sequence contains no elements");
            }
            var min = sourceIterator.Current;
            var minKey = selector(min);
            while (sourceIterator.MoveNext())
            {
                var candidate = sourceIterator.Current;
                var candidateProjected = selector(candidate);
                if (comparer.Compare(candidateProjected, minKey) < 0)
                {
                    min = candidate;
                    minKey = candidateProjected;
                }
            }
            return min;
        }
    }
    #endregion


    #region MaxBy
    // https://github.com/morelinq/MoreLINQ/blob/ec4bbd3c7ca61e3a98695aaa2afb23da001ee420/MoreLinq/MaxBy.cs#L25
    /// <summary>
    /// Returns the maximal element of the given sequence, based on
    /// the given projection.
    /// </summary>
    /// <remarks>
    /// If more than one element has the maximal projected value, the first
    /// one encountered will be returned. This overload uses the default comparer
    /// for the projected type. This operator uses immediate execution, but
    /// only buffers a single result (the current maximal element).
    /// </remarks>
    /// <typeparam name="TSource">Type of the source sequence</typeparam>
    /// <typeparam name="TKey">Type of the projected element</typeparam>
    /// <param name="source">Source sequence</param>
    /// <param name="selector">Selector to use to pick the results to compare</param>
    /// <returns>The maximal element, according to the projection.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="selector"/> is null</exception>
    /// <exception cref="InvalidOperationException"><paramref name="source"/> is empty</exception>

    public static TSource MaxBy<TSource, TKey>(this IEnumerable<TSource> source,
        Func<TSource, TKey> selector)
    {
        return source.MaxBy(selector, null);
    }

    /// <summary>
    /// Returns the maximal element of the given sequence, based on
    /// the given projection and the specified comparer for projected values. 
    /// </summary>
    /// <remarks>
    /// If more than one element has the maximal projected value, the first
    /// one encountered will be returned. This operator uses immediate execution, but
    /// only buffers a single result (the current maximal element).
    /// </remarks>
    /// <typeparam name="TSource">Type of the source sequence</typeparam>
    /// <typeparam name="TKey">Type of the projected element</typeparam>
    /// <param name="source">Source sequence</param>
    /// <param name="selector">Selector to use to pick the results to compare</param>
    /// <param name="comparer">Comparer to use to compare projected values</param>
    /// <returns>The maximal element, according to the projection.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/>, <paramref name="selector"/> 
    /// or <paramref name="comparer"/> is null</exception>
    /// <exception cref="InvalidOperationException"><paramref name="source"/> is empty</exception>

    public static TSource MaxBy<TSource, TKey>(this IEnumerable<TSource> source,
        Func<TSource, TKey> selector, IComparer<TKey> comparer)
    {
        if (source == null) throw new ArgumentNullException("source");
        if (selector == null) throw new ArgumentNullException("selector");
        comparer = comparer ?? Comparer<TKey>.Default;

        using (var sourceIterator = source.GetEnumerator())
        {
            if (!sourceIterator.MoveNext())
            {
                throw new InvalidOperationException("Sequence contains no elements");
            }
            var max = sourceIterator.Current;
            var maxKey = selector(max);
            while (sourceIterator.MoveNext())
            {
                var candidate = sourceIterator.Current;
                var candidateProjected = selector(candidate);
                if (comparer.Compare(candidateProjected, maxKey) > 0)
                {
                    max = candidate;
                    maxKey = candidateProjected;
                }
            }
            return max;
        }
    }
#endregion

    /*
    // https://stackoverflow.com/questions/3188693/how-can-i-get-linq-to-return-the-object-which-has-the-max-value-for-a-given-prop/3188751
    public static T MinBy<T>(this IEnumerable<T> list, Func<T, float> value)
    {
        return list.Aggregate
        (
            (a, b) =>
                value(a) < value(b)
                ? a : b
        );
    }

    public static T MaxBy<T>(this IEnumerable<T> list, Func<T, float> value)
    {
        return list.Aggregate
        (
            (a, b) =>
                value(a) > value(b)
                ? a : b
        );
    }
    */

    /*

    #region NoAlloc
    public static T MinBy<T>(this IEnumerable<T> source, Func<T, float> selector)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        if (selector == null) throw new ArgumentNullException(nameof(selector));

        using var enumerator = source.GetEnumerator();
        if (!enumerator.MoveNext())
            throw new InvalidOperationException("Sequence contains no elements");

        var minElem = enumerator.Current;
        var minVal = selector(minElem);

        while (enumerator.MoveNext())
        {
            var current = enumerator.Current;
            var currentVal = selector(current);
            if (currentVal < minVal)
            {
                minElem = current;
                minVal = currentVal;
            }
        }

        return minElem;
    }

    public static T MaxBy<T>(this IEnumerable<T> source, Func<T, float> selector)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        if (selector == null) throw new ArgumentNullException(nameof(selector));

        using var enumerator = source.GetEnumerator();
        if (!enumerator.MoveNext())
            throw new InvalidOperationException("Sequence contains no elements");

        var maxElem = enumerator.Current;
        var maxVal = selector(maxElem);

        while (enumerator.MoveNext())
        {
            var current = enumerator.Current;
            var currentVal = selector(current);
            if (currentVal > maxVal)
            {
                maxElem = current;
                maxVal = currentVal;
            }
        }

        return maxElem;
    }
    #endregion
    */



    public static int IndexOfMin<T>(this IEnumerable<T> list, Func<T, float> value)
    {
        float min = float.PositiveInfinity;
        int minI = 0;

        int i = 0;
        foreach (T item in list)
        {
            float current = value(item);

            if (current < min)
            {
                min = current;
                minI = i;
            }
            i++;
        }

        return minI;
    }
    public static int IndexOfMax<T>(this IEnumerable<T> list, Func<T, float> value)
    {
        float max = float.NegativeInfinity;
        int maxI = 0;

        int i = 0;
        foreach (T item in list)
        {
            float current = value(item);

            if (current > max)
            {
                max = current;
                maxI = i;
            }
            i++;
        }

        return maxI;
    }


    // Simpler version of Zip
    public static IEnumerable<(T1, T2)> Zip<T1, T2>(this IEnumerable<T1> source1, IEnumerable<T2> source2)
        => source1.Zip(source2, (a, b) => (a, b));



    // https://www.elevenwinds.com/blog/linq-distinctby-with-lambda-expression-parameter/
    public static IEnumerable<T> DistinctBy<T>(this IEnumerable<T> list, Func<T, object> propertySelector)
    {
        return list.GroupBy(propertySelector).Select(x => x.First());
    }


    // https://stackoverflow.com/questions/275073/why-do-c-sharp-multidimensional-arrays-not-implement-ienumerablet
    public static IEnumerable<T> Flatten<T>(this Array target)
    {
        foreach (var item in target)
            yield return (T)item;
    }

    // https://stackoverflow.com/questions/4823467/using-linq-to-find-the-cumulative-sum-of-an-array-of-numbers-in-c-sharp
    public static IEnumerable<float> CumulativeSum(this IEnumerable<float> sequence)
    {
        float sum = 0;
        foreach (var item in sequence)
        {
            sum += item;
            yield return sum;
        }
    }

    // probability is a function that gets the absolute probability of an element T
    public static T RandomProbability<T> (this IEnumerable<T> list, Func<T, float> probability)
    {
        var cumulativeProbabilities = list
            // Replace each element with a tuple <item, probability>
            //.Select(item => new Tuple<T, float>(item, probability(item)))
            .Select(item => (item: item, probability: probability(item)))
            // Replaces probability with cumulative probability
            .SelectAggregate
            (
                //new Tuple<T, float>(default(T), 0f), // Seed
                //(aggregate, next) => new Tuple<T, float>(next.first, next.second+aggregate.second)
                (item: default(T), probability: 0f), // Seed
                (aggregate, next) => (next.item, next.probability + aggregate.probability)
            );

        // https://stackoverflow.com/questions/46735106/pick-random-element-from-list-with-probability
        float random = UnityEngine.Random.Range(0f, 1f);
        //var selected = cumulativeProbabilities.SkipWhile(i => i.second < random).First();
        //return selected.first;
        var selected = cumulativeProbabilities.SkipWhile(i => i.probability < random).First();
        return selected.item;
    }

    // https://stackoverflow.com/questions/4823467/using-linq-to-find-the-cumulative-sum-of-an-array-of-numbers-in-c-sharp
    public static IEnumerable<TAccumulate> SelectAggregate<TSource, TAccumulate>(
    this IEnumerable<TSource> source,
    TAccumulate seed,
    Func<TAccumulate, TSource, TAccumulate> func)
    {
        return source.SelectAggregateIterator(seed, func);
    }

    private static IEnumerable<TAccumulate> SelectAggregateIterator<TSource, TAccumulate>(
        this IEnumerable<TSource> source,
        TAccumulate seed,
        Func<TAccumulate, TSource, TAccumulate> func)
    {
        TAccumulate previous = seed;
        foreach (var item in source)
        {
            TAccumulate result = func(previous, item);
            previous = result;
            yield return result;
        }
    }



    // https://www.c-sharpcorner.com/forums/ranking-items-in-a-list-with-linq
    // Gets the top k
    public static IEnumerable<T> Rank<T>(this IEnumerable<T> sequence, Func<T, float> sorter, int k)
    {
        return sequence
            .OrderByDescending(item => sorter(item))
            .Take(k);
    }

    // https://stackoverflow.com/questions/28611083/linq-to-return-null-if-an-array-is-empty
    // ToArray, or Null if the sequence is empty
    public static T[] ToArrayOrNull<T>(this IEnumerable<T> seq)
    {
        var result = seq.ToArray();

        if (result.Length == 0)
            return null;

        return result;
    }





    // Standard deviations
    public static float StandardDeviation(this IEnumerable<float> seq)
    {
        float mean = seq.Average();
        return
            Mathf.Sqrt
            (
                seq
                .Select(x => Mathf.Pow(x - mean, 2f))
                .Sum() / seq.Count()
            );
    }

    // https://stackoverflow.com/questions/2094729/recommended-way-to-check-if-a-sequence-is-empty
    public static bool IsEmpty<T>(this IEnumerable<T> source)
    {
        return !source.Any();
    }


    public static T Random<T>(this IEnumerable<T> source)
    {
        if (source == null)
        {
            throw new ArgumentNullException(nameof(source));
        }

        int count = source.Count();

        if (count == 0)
        {
            throw new InvalidOperationException("The sequence is empty.");
        }

        int randomIndex = UnityEngine.Random.Range(0, count);
        return source.ElementAt(randomIndex);
    }

    
    // https://stackoverflow.com/questions/967047/how-to-perform-a-binary-search-on-ilistt
    // Binary search on IList
    public static Int32 BinarySearchIndexOf<T>(this IList<T> list, T value, IComparer<T> comparer = null)
    {
        if (list == null)
            throw new ArgumentNullException(nameof(list));

        comparer = comparer ?? Comparer<T>.Default;

        Int32 lower = 0;
        Int32 upper = list.Count - 1;

        while (lower <= upper)
        {
            Int32 middle = lower + (upper - lower) / 2;
            Int32 comparisonResult = comparer.Compare(value, list[middle]);
            if (comparisonResult == 0)
                return middle;
            else if (comparisonResult < 0)
                upper = middle - 1;
            else
                lower = middle + 1;
        }

        return ~lower;
    }
    
}
