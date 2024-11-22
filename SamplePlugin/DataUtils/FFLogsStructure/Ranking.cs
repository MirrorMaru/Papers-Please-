using System.Text.Json.Serialization;

namespace xiv.raid.DataUtils.FFLogsStructure;

public class Ranking
{
    [JsonPropertyName("encounter")]
    public Encounter Encounter { get; set; }
    
    [JsonPropertyName("rankPercent")]
    public double RankPercent { get; set; }
}
