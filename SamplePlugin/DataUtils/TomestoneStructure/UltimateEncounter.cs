using System.Text.Json.Serialization;

namespace xiv.raid.DataUtils.TomestoneStructure;

public class UltimateEncounter
{
    [JsonPropertyName("compactName")]
    public string CompactName { get; set; }
    
    [JsonPropertyName("activity")]
    public Activity? Activity { get; set; }
}
