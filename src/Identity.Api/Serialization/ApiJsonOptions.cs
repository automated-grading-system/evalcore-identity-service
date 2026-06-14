using System.Text.Json;
using System.Text.Json.Serialization;

namespace Identity.Api.Serialization;

public static class ApiJsonOptions
{
    public static JsonSerializerOptions SerializerOptions { get; } = Create();

    public static void Configure(JsonSerializerOptions options)
    {
        options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        options.Converters.Add(new UtcDateTimeOffsetJsonConverter());
    }

    private static JsonSerializerOptions Create()
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        Configure(options);
        return options;
    }
}
