using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using NeoMonitor.Abstractions;

namespace NeoMonitor.Profiles
{
    public class NodeAddressTypeJsonConverter : JsonConverter<NodeAddressType>
    {
        public override NodeAddressType Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            string text = reader.GetString();
            return Enum.Parse<NodeAddressType>(text);
        }

        public override void Write(Utf8JsonWriter writer, NodeAddressType value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString());
        }
    }
}