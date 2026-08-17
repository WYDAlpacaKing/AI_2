using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

// A sorted list which can be used to store data into intervals

//[Serializable]
public class IntervalList<T>
{
    public readonly SortedList<float, T> Schedule = new();
    private float CurrentTime = 0f;

    public bool LoopTime = false;

    public T this[float time]
    {
        get => GetAt(time);
        //set => Add(time, value);
    }

    // Append this interval to the list
    public void Append(float duration, T value)
    {
        Schedule[CurrentTime] = value;
        CurrentTime += duration;
    }


    public void Reset()
    {
        Schedule.Clear();
        CurrentTime = 0f;
    }


    public T GetAt(float time)
    {
        if (LoopTime)
            time %= CurrentTime;

        // Time before the first entry
        if (time < Schedule.Keys[0])
            return Schedule.Values[0];

        // Time after the last entry
        if (time >= Schedule.Keys[^1])
            return Schedule.Values[^1];

        int index = Schedule.Keys
            .TakeWhile(k => k <= time)
            .Count() - 1;

        // Exact time found!
        return Schedule.Values[index];
    }
}
