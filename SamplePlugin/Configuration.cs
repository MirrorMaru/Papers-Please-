using Dalamud.Configuration;
using Dalamud.Plugin;
using System;

namespace xiv.raid;

[Serializable]
public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 0;

    public string ClientId { get; set; } = "";
    public string SecretId { get; set; } = "";
    public string TomestoneToken { get; set; } = "";

    // the below exist just to make saving less cumbersome
    public void Save()
    {
        Plugin.PluginInterface.SavePluginConfig(this);
    }
}
