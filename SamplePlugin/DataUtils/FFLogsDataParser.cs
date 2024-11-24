using System;
using System.Collections.Generic;
using System.Text.Json;

namespace xiv.raid.DataUtils.FFLogsStructure;

public static class FFLogsDataParser
{
    public static List<Ranking> GetFflogsResponseFromJson(string json)
    {
        try
        {
            var fflogsResponse = JsonSerializer.Deserialize<FflogsResponse>(json) ?? new FflogsResponse();
            var rankings = fflogsResponse?.Data?.CharacterData?.Character?.ZoneRankings?.Rankings;

            if (rankings != null)
            {
                return rankings;
            }
            
            return new List<Ranking>();
        }
        catch (Exception e)
        {
            DalamudApi.PluginLog.Warning(e.Message);
            return new List<Ranking>();
        }
    }
}
