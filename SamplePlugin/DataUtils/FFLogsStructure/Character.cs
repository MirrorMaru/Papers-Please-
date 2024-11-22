using System.Text.Json.Serialization;

namespace xiv.raid.DataUtils.FFLogsStructure;

public class Character
{
    [JsonPropertyName("zoneRankings")]
    public ZoneRankings ZoneRankings { get; set; }
}
