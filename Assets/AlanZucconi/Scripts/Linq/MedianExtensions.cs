using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

// https://gist.github.com/axelheer/b1cb9d7c267d6762b244
public static class MedianExtensions
{
    // --- by Alan ---
    public static float Percentile(this IEnumerable<float> source, float percentile = 0.5f)
    {
        if (source == null)
            throw new ArgumentNullException(nameof(source));
        if (!source.Any())
            throw new InvalidOperationException("Cannot compute percentile of an empty sequence.");
        if (percentile < 0f || percentile > 1f)
            throw new ArgumentOutOfRangeException(nameof(percentile), "Percentile must be between 0 and 1.");

        var sorted = source.OrderBy(x => x).ToArray();
        return PercentileOnSortedArray(sorted, percentile);
        /*
        float position = (sorted.Length - 1) * percentile;
        int lowerIndex = (int)Math.Floor(position);
        int upperIndex = (int)Math.Ceiling(position);

        if (lowerIndex == upperIndex)
            return sorted[lowerIndex];

        float lowerValue = sorted[lowerIndex];
        float upperValue = sorted[upperIndex];
        float weight = position - lowerIndex;

        return lowerValue + (upperValue - lowerValue) * weight;*/
    }

    // Calculates the percentile on a sorted array
    // The array HAS to be sorted.
    private static float PercentileOnSortedArray(this float[] sorted, float percentile = 0.5f)
    {
        float position = (sorted.Length - 1) * percentile;
        int lowerIndex = (int)Math.Floor(position);
        int upperIndex = (int)Math.Ceiling(position);

        if (lowerIndex == upperIndex)
            return sorted[lowerIndex];

        float lowerValue = sorted[lowerIndex];
        float upperValue = sorted[upperIndex];
        float weight = position - lowerIndex;

        return lowerValue + (upperValue - lowerValue) * weight;
    }


    public static float Percentile<TSource>(this IEnumerable<TSource> source, Func<TSource, float> selector, float percentile = 0.5f)
    {
        return source.Select(selector).Percentile(percentile);
    }


    public static float Quartile1<TSource>(this IEnumerable<TSource> source, Func<TSource, float> selector)
        => source.Select(selector).Percentile(1f / 4f);
    public static float Quartile3<TSource>(this IEnumerable<TSource> source, Func<TSource, float> selector)
        => source.Select(selector).Percentile(3f / 4f);

    public static float Quartile1(this IEnumerable<float> source) => source.Quartile1(x => x);
    public static float Quartile3(this IEnumerable<float> source) => source.Quartile3(x => x);


    // Calculates the interquartile range
    // q1 is Quartile 1
    // q2 is the median
    // q3 is Quartile 2
    // 50% of the data lies between q1 and qr (IQR)
    // This function is faster than calculating the statistics separately,
    // since it needs to sort the source
    public static (float q1, float q2, float q3) IQR <TSource> (this IEnumerable<TSource> source, Func<TSource, float> selector)
    {
        var sorted = source
            .Select(selector)
            .OrderBy(x => x)
            .ToArray();
        return
            (
                sorted.PercentileOnSortedArray(1f / 4f),
                sorted.PercentileOnSortedArray(2f / 4f),
                sorted.PercentileOnSortedArray(3f / 4f)
            );
    }

    /*
    public static T Percentile<TSource, T>(this IEnumerable<TSource> source, Func<TSource, T> selector, float percentile = 0.5f)
    {
        return source.Select(selector).Percentile(percentile);
    }
    */
    // -------------
    public static double Median(this IEnumerable<int> source)
    {
        if (source == null)
            throw new ArgumentNullException("source");
        var data = source.OrderBy(n => n).ToArray();
        if (data.Length == 0)
            throw new InvalidOperationException();
        if (data.Length % 2 == 0)
            return (data[data.Length / 2 - 1] + data[data.Length / 2]) / 2.0;
        return data[data.Length / 2];
    }
    
    public static double? Median(this IEnumerable<int?> source)
    {
        if (source == null)
            throw new ArgumentNullException("source");
        var data = source.Where(n => n.HasValue).Select(n => n.Value).OrderBy(n => n).ToArray();
        if (data.Length == 0)
            return null;
        if (data.Length % 2 == 0)
            return (data[data.Length / 2 - 1] + data[data.Length / 2]) / 2.0;
        return data[data.Length / 2];
    }
    
    public static double Median(this IEnumerable<long> source)
    {
        if (source == null)
            throw new ArgumentNullException("source");
        var data = source.OrderBy(n => n).ToArray();
        if (data.Length == 0)
            throw new InvalidOperationException();
        if (data.Length % 2 == 0)
            return (data[data.Length / 2 - 1] + data[data.Length / 2]) / 2.0;
        return data[data.Length / 2];
    }
    
    public static double? Median(this IEnumerable<long?> source)
    {
        if (source == null)
            throw new ArgumentNullException("source");
        var data = source.Where(n => n.HasValue).Select(n => n.Value).OrderBy(n => n).ToArray();
        if (data.Length == 0)
            return null;
        if (data.Length % 2 == 0)
            return (data[data.Length / 2 - 1] + data[data.Length / 2]) / 2.0;
        return data[data.Length / 2];
    }
    
    public static float Median(this IEnumerable<float> source)
    {
        if (source == null)
            throw new ArgumentNullException("source");
        var data = source.OrderBy(n => n).ToArray();
        if (data.Length == 0)
            throw new InvalidOperationException();
        if (data.Length % 2 == 0)
            return (data[data.Length / 2 - 1] + data[data.Length / 2]) / 2.0f;
        return data[data.Length / 2];
    }
    
    public static float? Median(this IEnumerable<float?> source)
    {
        if (source == null)
            throw new ArgumentNullException("source");
        var data = source.Where(n => n.HasValue).Select(n => n.Value).OrderBy(n => n).ToArray();
        if (data.Length == 0)
            return null;
        if (data.Length % 2 == 0)
            return (data[data.Length / 2 - 1] + data[data.Length / 2]) / 2.0f;
        return data[data.Length / 2];
    }
    
    public static double Median(this IEnumerable<double> source)
    {
        if (source == null)
            throw new ArgumentNullException("source");
        var data = source.OrderBy(n => n).ToArray();
        if (data.Length == 0)
            throw new InvalidOperationException();
        if (data.Length % 2 == 0)
            return (data[data.Length / 2 - 1] + data[data.Length / 2]) / 2.0;
        return data[data.Length / 2];
    }
    
    public static double? Median(this IEnumerable<double?> source)
    {
        if (source == null)
            throw new ArgumentNullException("source");
        var data = source.Where(n => n.HasValue).Select(n => n.Value).OrderBy(n => n).ToArray();
        if (data.Length == 0)
            return null;
        if (data.Length % 2 == 0)
            return (data[data.Length / 2 - 1] + data[data.Length / 2]) / 2.0;
        return data[data.Length / 2];
    }
    
    public static decimal Median(this IEnumerable<decimal> source)
    {
        if (source == null)
            throw new ArgumentNullException("source");
        var data = source.OrderBy(n => n).ToArray();
        if (data.Length == 0)
            throw new InvalidOperationException();
        if (data.Length % 2 == 0)
            return (data[data.Length / 2 - 1] + data[data.Length / 2]) / 2.0m;
        return data[data.Length / 2];
    }
    
    public static decimal? Median(this IEnumerable<decimal?> source)
    {
        if (source == null)
            throw new ArgumentNullException("source");
        var data = source.Where(n => n.HasValue).Select(n => n.Value).OrderBy(n => n).ToArray();
        if (data.Length == 0)
            return null;
        if (data.Length % 2 == 0)
            return (data[data.Length / 2 - 1] + data[data.Length / 2]) / 2.0m;
        return data[data.Length / 2];
    }
    
    public static double Median<TSource>(this IEnumerable<TSource> source, Func<TSource, int> selector)
    {
        return source.Select(selector).Median();
    }
    
    public static double? Median<TSource>(this IEnumerable<TSource> source, Func<TSource, int?> selector)
    {
        return source.Select(selector).Median();
    }
    
    public static double Median<TSource>(this IEnumerable<TSource> source, Func<TSource, long> selector)
    {
        return source.Select(selector).Median();
    }
    
    public static double? Median<TSource>(this IEnumerable<TSource> source, Func<TSource, long?> selector)
    {
        return source.Select(selector).Median();
    }
    
    public static float Median<TSource>(this IEnumerable<TSource> source, Func<TSource, float> selector)
    {
        return source.Select(selector).Median();
    }
    
    public static float? Median<TSource>(this IEnumerable<TSource> source, Func<TSource, float?> selector)
    {
        return source.Select(selector).Median();
    }
    
    public static double Median<TSource>(this IEnumerable<TSource> source, Func<TSource, double> selector)
    {
        return source.Select(selector).Median();
    }
    
    public static double? Median<TSource>(this IEnumerable<TSource> source, Func<TSource, double?> selector)
    {
        return source.Select(selector).Median();
    }
    
    public static decimal Median<TSource>(this IEnumerable<TSource> source, Func<TSource, decimal> selector)
    {
        return source.Select(selector).Median();
    }
    
    public static decimal? Median<TSource>(this IEnumerable<TSource> source, Func<TSource, decimal?> selector)
    {
        return source.Select(selector).Median();
    }
}