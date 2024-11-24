using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace xiv.raid.DataUtils.TomestoneStructure;

public class Encounters
{
    [JsonPropertyName("ultimate")]
    public List<UltimateEncounter> Ultimates { get; set; }
}
