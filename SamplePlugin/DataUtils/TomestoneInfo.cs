using System;
using System.Collections.Generic;

namespace xiv.raid.Utils;

public class TomestoneInfo
{
    public TomestoneInfo(Dictionary<string, float> progress)
    {
        Progress = new Dictionary<string, float>(progress);
    }

    public Dictionary<string, float> Progress { get; }

    public void addProgress(string name, float value)
    {
        Progress.Add(name, value);
    }
}
