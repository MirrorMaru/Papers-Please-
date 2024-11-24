﻿using System.Text.Json.Serialization;

namespace xiv.raid.DataUtils.TomestoneStructure;

public class TomestoneResponse
{
    [JsonPropertyName("encounters")]
    public Encounters Encounters { get; set; }
    
    [JsonPropertyName("ultimateProgressionTarget")]
    public string UltimateProgressionTarget { get; set; }
}
