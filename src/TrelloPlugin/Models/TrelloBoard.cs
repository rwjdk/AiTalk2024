using System.Text.Json.Serialization;
using TrelloDotNet.Model;

namespace TrelloPlugin.Models;

public class TrelloBoard
{
    [JsonPropertyName("board_id")]
    public required string Id { get; init; }

    [JsonPropertyName("board_name")]
    public required string Name { get; init; }

    [JsonPropertyName("board_url")]
    public required string Url { get; init; }

    public static TrelloBoard FromBoard(Board board)
    {
        return new TrelloBoard
        {
            Id = board.Id,
            Name = board.Name,
            Url = board.ShortUrl
        };
    }

    public override string ToString()
    {
        return $"{Name}";
    }
}