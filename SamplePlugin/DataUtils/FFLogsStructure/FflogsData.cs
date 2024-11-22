using System.Text.Json.Serialization;

namespace xiv.raid.DataUtils.FFLogsStructure;

public class FflogsData
{
    [JsonPropertyName("characterData")]
    public CharacterData CharacterData { get; set; }
}
