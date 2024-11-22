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

            DalamudApi.PluginLog.Error("ranking looks empty ! Received json : " + json);
            return new List<Ranking>();
        }
        catch (Exception e)
        {
            DalamudApi.PluginLog.Error(e.Message);
            return new List<Ranking>();
        }
    }
}
