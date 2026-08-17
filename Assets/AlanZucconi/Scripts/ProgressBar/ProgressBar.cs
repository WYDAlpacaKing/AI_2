using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

[System.Serializable]
public class ProgressBar
{
    public float Value;
    public string Label;

    public ProgressBar (string label)
    {
        Value = 0f;
        Label = label;
    }

    // Looops over a range,
    //  and updates the bar at the same time
    /*
    public IEnumerable<int> Loop(string text, int start, int count, int increment = +1)
    {
        for (int i = start; i < count; i += increment)
        {
            Value = (i+0) / (float) count;
            Label = $"{text}: {i+1} of {count} ({(int)(Value * 100)}%)";

            yield return i;
        }

        Value = 1f;
        Label = $"{text}: Done!";
    }
    */

    // "count" represents the maximum number of the foor loop, not how many elements to do
    public IEnumerable<int> Loop(string text, int start, int count)
        => Loop(text, Enumerable.Range(start, count-start));

    public IEnumerable<int> Loop(string text, int count)
        => Loop(text, 0, count);
    
    
    // Loop over a list
    public IEnumerable<T> Loop<T>(string text, IEnumerable<T> source)
    {
        int count = source.Count();
        int i = 0;

        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();

        foreach (T t in source)
        {
            //Value = (i+0) / (float) count;
            //Label = $"{text} ({t}): {i + 1} of {count} ({(int)(Value * 100)}%)";
            Update($"{text} ({t})", i, count);

            if (i != 0)
            {
                float elapsedTime = (float) stopwatch.Elapsed.TotalSeconds;
                // elapsedSeconds : i = remainingTime : (count-i)
                float remainingTime = elapsedTime * (count-i) / i;
                Label += $" {Utils.FormatTime(remainingTime)} left";
                //TimeSpan span = TimeSpan.FromSeconds(remainingTime);
                //Label += $" {span.ToString(@"hh\:mm\:ss")}s left";
            }

            yield return t;

            i++;
        }

        stopwatch.Stop();

        Value = 1f;
        Label = $"{text}: Done! ";

        float totalTime = (float)stopwatch.Elapsed.TotalSeconds;
        Label += $"{Utils.FormatTime(totalTime)}";
        //TimeSpan newSpan = TimeSpan.FromSeconds(totalTime);
        //Label += $" {newSpan.ToString(@"hh\:mm\:ss")}s";
    }

    // Updates the bar
    public void Update(string text, int i, int count)
    {
        Value = (i + 0) / (float)count;
        Label = $"{text}: {i + 1} of {count} ({(int)(Value * 100)}%)";
    }
}
