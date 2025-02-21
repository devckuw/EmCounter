using Dalamud.Configuration;
using Dalamud.Plugin;
using System;
using System.Collections.Generic;

namespace EmCounter;

[Serializable]
public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 0;
    public bool showFlyText { get; set; } = true;
    public bool showFlyTextNames { get; set; } = true;

    public Dictionary<ulong, Dictionary<ulong, Dictionary<ushort, int>>> dataCount = new Dictionary<ulong, Dictionary<ulong, Dictionary<ushort, int>>>();
    public Dictionary<ulong, string> dataNames = new Dictionary<ulong, string>();

    // the below exist just to make saving less cumbersome
    public void Save()
    {
        Plugin.PluginInterface.SavePluginConfig(this);
    }
}
