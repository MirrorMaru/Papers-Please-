using System;
using System.Threading.Tasks;
using xiv.raid.OAuth;

namespace xiv.raid.DataUtils;

public static class TomestoneRequester
{
    public static async Task<String> GetTomestoneInfo(string world, string player)
    {
        DalamudApi.PluginLog.Debug("Sending tomestone request to get player info");
        try
        {
            return await ApiLink.Instance.GetTomestoneData(world, player);
        }
        catch (Exception e)
        {
            DalamudApi.PluginLog.Error(e.ToString());
            return e.ToString();
        }
    }
}
