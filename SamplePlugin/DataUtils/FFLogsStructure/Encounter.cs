using System.Text.Json.Serialization;

namespace xiv.raid.DataUtils.FFLogsStructure;

public class Encounter
{
    [JsonPropertyName("name")]
    public string Name { get; set; }
}
