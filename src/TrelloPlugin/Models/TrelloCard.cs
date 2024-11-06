using System.Text.Json.Serialization;
using TrelloDotNet.Model;

namespace TrelloPlugin.Models;

public class TrelloCard
{
    private bool _dueComplete;

    [JsonPropertyName("card_id")]
    public required string Id { get; init; }

    [JsonPropertyName("card_name")]
    public required string Name { get; init; }

    [JsonPropertyName("card_is_overdue")]
    public bool IsOverDue => !_dueComplete && DueDate.HasValue && DueDate.Value < DateTimeOffset.UtcNow;

    [JsonPropertyName("card_is_due_today")]
    public bool IsDueToday => !_dueComplete && DueDate.HasValue && DueDate.Value.Date == DateTimeOffset.UtcNow.Date;

    [JsonPropertyName("card_is_started")]
    public bool IsStarted => StartDate.HasValue && StartDate.Value < DateTime.UtcNow;

    [JsonPropertyName("card_due_date")]
    public required DateTimeOffset? DueDate { get; init; }

    [JsonPropertyName("card_start_date")]
    public required DateTimeOffset? StartDate { get; init; }

    [JsonPropertyName("card_labels")]
    public required TrelloLabel[] Labels { get; init; }

    [JsonPropertyName("card_members")]
    public required TrelloMember[] Members { get; init; }

    [JsonPropertyName("card_list_id")]
    public required string ListId { get; init; }

    [JsonPropertyName("card_list_name")]
    public required string ListName { get; init; }

    [JsonPropertyName("card_url")]
    public required string Url { get; init; }

    [JsonPropertyName("is_assigned_to_user")]
    public required bool IsAssignedToUser { get; init; }


    public static TrelloCard FromCard(Card card, Member currentUser)
    {
        try
        {
            TrelloList list = TrelloList.FromList(card.List);
            return new TrelloCard
            {
                Id = card.Id,
                Name = card.Name,
                ListId = list.Id,
                ListName = list.Name,
                DueDate = card.Due,
                StartDate = card.Start,
                Labels = card.Labels.Select(TrelloLabel.FromLabel).ToArray(),
                Members = card.Members.Select(TrelloMember.FromMember).ToArray(),
                Url = card.ShortUrl,
                IsAssignedToUser = card.Members.Any(x => x.Id == currentUser.Id),
                _dueComplete = card.DueComplete
            };
        }
        catch (Exception e)
        {
            throw;
        }
    }

    public override string ToString()
    {
        return $"{Name}";
    }
}