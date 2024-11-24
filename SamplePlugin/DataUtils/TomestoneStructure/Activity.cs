using System.Text.Json.Serialization;

namespace xiv.raid.DataUtils.TomestoneStructure;

public class Activity
{
    [JsonPropertyName("patch")]
    public string Patch { get; set; }
}
