using System;
using System.Text.Json;
using xiv.raid.DataUtils.TomestoneStructure;

namespace xiv.raid.DataUtils;

public static class TomestoneDataParser
{
    public static TomestoneData TomestoneDataFromJson(string json)
    {
        var tomestoneResponse = JsonSerializer.Deserialize<TomestoneResponse>(json);

        return new TomestoneData(tomestoneResponse.Encounters, tomestoneResponse.UltimateProgressionTarget);
    }
}
