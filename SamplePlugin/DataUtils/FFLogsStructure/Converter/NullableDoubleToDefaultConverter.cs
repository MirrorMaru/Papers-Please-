using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace xiv.raid.DataUtils.FFLogsStructure.Converter;

public class NullableDoubleToDefaultConverter : JsonConverter<double>
{
    public override double Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            return -1;
        }
        
        return reader.GetDouble();
    }

    public override void Write(Utf8JsonWriter writer, double value, JsonSerializerOptions options)
    {
        writer.WriteNumberValue(value);
    }
}
