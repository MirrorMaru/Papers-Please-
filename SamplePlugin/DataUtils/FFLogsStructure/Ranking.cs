using System.Text.Json.Serialization;
using xiv.raid.DataUtils.FFLogsStructure.Converter;

namespace xiv.raid.DataUtils.FFLogsStructure;

public class Ranking
{
    [JsonPropertyName("encounter")]
    public Encounter Encounter { get; set; }

    [JsonPropertyName("rankPercent")]
    [JsonConverter(typeof(NullableDoubleToDefaultConverter))]
    public double RankPercent { get; set; } 
    
    [JsonPropertyName("spec")]
    public string Spec { get; set; }
}
