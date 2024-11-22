using System.Text.Json.Serialization;

namespace xiv.raid.DataUtils.FFLogsStructure;

public class CharacterData
{
    [JsonPropertyName("character")]
    public Character Character { get; set; }
}
