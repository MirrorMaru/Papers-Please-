using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace xiv.raid.DataUtils.FFLogsStructure;

public class Character
{
    [JsonPropertyName("zoneRankings")]
    public ZoneRankings ZoneRankings { get; set; }
    
    [JsonPropertyName("lodestoneID")]
    public int LodestoneId { get; set; }
}
