using System.Text.Json.Serialization;

namespace xiv.raid.DataUtils.TomestoneStructure;

public class Achievement
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
}
