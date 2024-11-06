using System.Text.Json.Serialization;
using TrelloDotNet.Model;

namespace TrelloPlugin.Models;

public class TrelloList
{
    [JsonPropertyName("list_id")]
    public required string Id { get; init; }

    [JsonPropertyName("list_name")]
    public required string Name { get; init; }

    public static TrelloList FromList(List list)
    {
        return new TrelloList
        {
            Id = list.Id,
            Name = list.Name
        };
    }

    public override string ToString()
    {
        return $"{Name}";
    }
}