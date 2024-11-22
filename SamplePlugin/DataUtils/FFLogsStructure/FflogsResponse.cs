using System.Text.Json.Serialization;

namespace xiv.raid.DataUtils.FFLogsStructure;

public class FflogsResponse
{
    [JsonPropertyName("data")]
    public FflogsData Data { get; set; }
}
