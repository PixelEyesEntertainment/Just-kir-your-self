using System;

[Serializable]
public class NoteEntry
{
    public float time;
    public int lane;
    public int soundId;
    public float hold = 0f;
}

[Serializable]
public class ChartData
{
    public float speed;
    public NoteEntry[] notes;
}