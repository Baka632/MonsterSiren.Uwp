using System.Collections.ObjectModel;

namespace MonsterSiren.Api.Helpers.Converters;

internal sealed class InternedStringArrayConverter : JsonConverter<IEnumerable<string>>
{
    private const string msr = "塞壬唱片-MSR";
    private static readonly string[] empty = [];
    private static readonly ReadOnlyCollection<string> singleMsr = new([msr]);

    public override IEnumerable<string> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        while (reader.TokenType == JsonTokenType.Comment)
        {
            reader.Read();
        }

        if (reader.TokenType != JsonTokenType.StartArray)
        {
            throw new JsonException("期望一个数组。");
        }

        if (!reader.Read())
        {
            throw new JsonException("JSON 意外终止。");
        }
        while (reader.TokenType == JsonTokenType.Comment)
        {
            reader.Read();
        }

        if (reader.TokenType == JsonTokenType.EndArray)
        {
            return empty;
        }

        if (reader.TokenType != JsonTokenType.String)
        {
            throw new JsonException("必须是纯字符串数组。");
        }

        string? firstValue = reader.GetString();
        string firstInterned = string.IsNullOrEmpty(firstValue) ? string.Empty : string.Intern(firstValue);

        if (!reader.Read())
        {
            throw new JsonException("JSON 意外终止。");
        }
        while (reader.TokenType == JsonTokenType.Comment)
        {
            reader.Read();
        }

        if (reader.TokenType == JsonTokenType.EndArray)
        {
            if (ReferenceEquals(msr, firstInterned))
            {
                return singleMsr;
            }

            return new ReadOnlyCollection<string>([firstInterned]);
        }

        List<string> list = new(2) { firstInterned };

        while (reader.TokenType != JsonTokenType.EndArray)
        {
            if (reader.TokenType == JsonTokenType.String)
            {
                string? value = reader.GetString();
                list.Add(string.IsNullOrEmpty(value) ? string.Empty : string.Intern(value));
            }
            else
            {
                throw new JsonException("必须是纯字符串数组。");
            }

            if (!reader.Read())
            {
                throw new JsonException("JSON 意外终止。");
            }
            while (reader.TokenType == JsonTokenType.Comment)
            {
                reader.Read();
            }
        }

        return list;
    }

    public override void Write(Utf8JsonWriter writer, IEnumerable<string> value, JsonSerializerOptions options)
    {
        writer.WriteStartArray();
        foreach (string item in value)
        {
            writer.WriteStringValue(item);
        }
        writer.WriteEndArray();
    }
}
