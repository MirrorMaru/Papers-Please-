using System;
using System.Threading.Tasks;
using xiv.raid.OAuth;

namespace xiv.raid.DataUtils;

public static class LogsRequester
{
    public static async Task<String> GetPlayerInfo(string playerName, string region, string playerWorld, string jobName)
    {
        DalamudApi.PluginLog.Debug("Sending FFlgos request to get player info");
        try
        {
            string query = @"
            query GetCharacter($name : String!, $serverSlug: String!, $serverRegion: String!, $job: String!) {
	characterData {
		character(name: $name, serverSlug: $serverSlug, serverRegion: $serverRegion) {
			canonicalID
			lodestoneID
			id
			zoneRankings(specName: $job, difficulty: 101)
		}
	}
}
";
            var variable = new
            {
                name = playerName,
                serverSlug = playerWorld,
                serverRegion = region,
                job = jobName
            };
            return await ApiLink.Instance.GetFFLogsData(query, variable);
        }
        catch (Exception ex)
        {
            DalamudApi.PluginLog.Error(ex.ToString());
            return ex.ToString();
        }
    }
}
