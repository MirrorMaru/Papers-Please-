using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace xiv.raid.DataUtils.FFLogsStructure;

public class ZoneRankings
{
    [JsonPropertyName("rankings")]
    public List<Ranking> Rankings { get; set; }
}
