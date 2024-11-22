using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;

namespace xiv.raid;

public class DalamudApi
{
    public static void Initialize(IDalamudPluginInterface pluginInterface) => pluginInterface.Create<DalamudApi>();
    
    [PluginService] public static IPluginLog PluginLog { get; private set; } = null;

    [PluginService] public static IDataManager DataManager { get; private set; } = null;
}
