using System.Text.Json.Serialization;

namespace xiv.raid.DataUtils.TomestoneStructure;

public class UltimateEncounter
{
    [JsonPropertyName("compactName")]
    public string CompactName { get; set; }
    
    [JsonPropertyName("achievement")]
    public Achievement? Achievement { get; set; }
}
