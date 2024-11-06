using System.Text.Json.Serialization;
using TrelloDotNet.Model;

namespace TrelloPlugin.Models;

public class TrelloLabel
{
    [JsonPropertyName("label_id")]
    public required string Id { get; init; }

    [JsonPropertyName("label_name")]
    public required string Name { get; init; }

    [JsonPropertyName("label_color")]
    public required string Color { get; init; }

    public static TrelloLabel FromLabel(Label label)
    {
        return new TrelloLabel
        {
            Id = label.Id,
            Name = label.Name,
            Color = label.Color
        };
    }

    public override string ToString()
    {
        return $"{Name} ({Color})";
    }
}