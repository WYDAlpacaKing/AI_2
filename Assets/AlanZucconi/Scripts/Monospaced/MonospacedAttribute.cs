using UnityEngine;

public class MonospacedAttribute : PropertyAttribute
{
    public int minLines;
    public int maxLines;

    public MonospacedAttribute(int minLines = 1, int maxLines = 1)
    {
        this.minLines = minLines;
        this.maxLines = maxLines;
    }
}