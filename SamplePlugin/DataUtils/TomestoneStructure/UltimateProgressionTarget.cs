using System.Text.Json.Serialization;

namespace xiv.raid.DataUtils.TomestoneStructure;

public class UltimateProgressionTarget
{
    [JsonPropertyName("percent")]
    public string Percent { get; set; }
    
    [JsonPropertyName("name")]
    public string Name { get; set; }
}
