using System.Text.Json.Serialization;
using TrelloDotNet.Model;

namespace TrelloPlugin.Models;

public class TrelloMember
{
    [JsonPropertyName("member_id")]
    public required string Id { get; init; }

    [JsonPropertyName("member_name")]
    public required string Name { get; init; }

    public static TrelloMember FromMember(Member member)
    {
        return new TrelloMember
        {
            Id = member.Id,
            Name = member.FullName
        };
    }

    public override string ToString()
    {
        return $"{Name}";
    }
}