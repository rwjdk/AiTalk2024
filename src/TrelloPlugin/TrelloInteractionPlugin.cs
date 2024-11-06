using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using JetBrains.Annotations;
using Microsoft.SemanticKernel;
using TrelloDotNet;
using TrelloDotNet.Model;
using TrelloDotNet.Model.Options;
using TrelloDotNet.Model.Options.GetBoardOptions;
using TrelloDotNet.Model.Options.GetCardOptions;
using TrelloPlugin.Models;

namespace TrelloPlugin;

[UsedImplicitly]
internal class TrelloInteractionPlugin(TrelloClient trelloClient, Member currentUser, TrelloBoard? currentBoard = null, bool debugMode = true)
{
    [UsedImplicitly]
    public TrelloBoard? CurrentBoard { get; set; } = currentBoard;

    [UsedImplicitly]
    [KernelFunction("get_skills")]
    [Description("Get a list of things the user can do (aka what skills are available) with the current permission set")]
    public string[] GetSkills()
    {
        List<string> result =
        [
            "Information: Get what boards you can access",
            "Information: Get what cards are on a board",
            "Information: Get what lists are on a board",
            "Information: Get what members are on a board",
            "Information: Get what labels are on a board",
            "Information: Get the current board",
            "Action: Add a Card",
            "Action: Update a card's start date",
            "Action: Update a card's due date",
            "Action: Update a card's list",
            "Action: Mark cards as complete/incomplete",
            "Action: Add/Remove Covers on cards",
            "Action: Add/Remove Labels on cards",
            "Action: Add/Remove Members on cards",
            "Action: Archive cards"
        ];

        return result.ToArray();
    }

    [UsedImplicitly]
    [KernelFunction("get_trello_boards")]
    [Description("Get the trello boards the user have access to")]
    public async Task<TrelloBoard[]> GetTrelloBoards()
    {
        var boards = await GetBoardsRaw(nameof(GetTrelloBoards));
        return boards.Select(TrelloBoard.FromBoard).ToArray();
    }

    [UsedImplicitly]
    [KernelFunction("get_trello_lists")]
    [Description("Get the lists on the selected board")]
    public async Task<TrelloList[]> GetTrelloListsOnBoard()
    {
        var data = await GetListsRaw(nameof(GetTrelloListsOnBoard));
        return data.Select(TrelloList.FromList).ToArray();
    }

    [UsedImplicitly]
    [KernelFunction("get_trello_labels")]
    [Description("Get the labels on the selected board")]
    public async Task<TrelloLabel[]> GetTrelloLabelsOnBoard()
    {
        var data = await GetLabelsRaw(nameof(GetTrelloLabelsOnBoard));
        return data.Select(TrelloLabel.FromLabel).ToArray();
    }

    [UsedImplicitly]
    [KernelFunction("get_trello_members")]
    [Description("Get the members on the selected board")]
    public async Task<TrelloMember[]> GetTrelloMembersOnBoard()
    {
        var data = await GetMembersRaw(nameof(GetTrelloMembersOnBoard));
        return data.Select(TrelloMember.FromMember).ToArray();
    }

    [UsedImplicitly]
    [KernelFunction("get_trello_cards_count")]
    [Description("Get the number of cards on the selected board")]
    public async Task<int> GetTrelloCardsOnBoardCount(
        [Description("Set to true if you only wish current user's cards")]
        bool only_assigned_to_current_user = false,
        [Description("Set to true if you only wish cards that are due today")]
        bool is_due_today = false,
        [Description("Set to true if you only want overdue cards")]
        bool is_overdue = false)
    {
        var cards = await GetTrelloCardsRaw(nameof(GetTrelloCardsOnBoardCount), only_assigned_to_current_user, is_due_today, is_overdue);
        return cards.Length;
    }

    [UsedImplicitly]
    [KernelFunction("get_trello_cards")]
    [Description("Get the cards on the selected board")]
    public async Task<TrelloCard[]> GetTrelloCardsOnBoard(
        [Description("Set to true if you only wish current user's cards")]
        bool only_assigned_to_current_user = false,
        [Description("Set to true if you only wish cards that are due today")]
        bool is_due_today = false,
        [Description("Set to true if you only want overdue cards")]
        bool is_overdue = false)
    {
        var cards = await GetTrelloCardsRaw(nameof(GetTrelloCardsOnBoard), only_assigned_to_current_user, is_due_today, is_overdue);
        return cards;
    }

    [UsedImplicitly]
    [KernelFunction("add_trello_card")]
    [Description("Add a new trello card")]
    public async Task AddTrelloCard(
        [Required]
        string name,
        [Required, Description("Call 'get_trello_lists' to get valid values")]
        TrelloList list,
        [Description("Call 'get_trello_label' to get valid values")]
        TrelloLabel[]? labels = null,
        [Description("Call 'get_trello_member' to get valid values")]
        TrelloMember[]? members = null,
        DateTimeOffset? start_date = null,
        DateTimeOffset? due_date = null)
    {
        LogDebug(nameof(AddTrelloCard), new()
        {
            { nameof(name), name },
            { nameof(list), list.Name },
            { nameof(labels), labels },
            { nameof(members), members },
            { nameof(start_date), start_date },
            { nameof(due_date), due_date },
        });

        try
        {
            var input = new Card(list.Id, name);
            if (due_date != null)
            {
                input.Due = due_date;
            }

            if (start_date != null)
            {
                input.Start = start_date;
            }

            Card card = await trelloClient.AddCardAsync(input);
            if (labels != null)
            {
                await trelloClient.AddLabelsToCardAsync(card.Id, labels.Select(x => x.Id).ToArray());
            }

            if (members != null)
            {
                await trelloClient.AddLabelsToCardAsync(card.Id, members.Select(x => x.Id).ToArray());
            }
        }
        catch (Exception e)
        {
            LogError(e);
            throw;
        }
    }

    [UsedImplicitly]
    [KernelFunction("update_due_date_on_card")]
    [Description("Update the due_date on a specific card")]
    public async Task UpdateDueDateOnTrelloCard([Required] TrelloCard card, [Required] DateTimeOffset? due_date)
    {
        LogDebug(nameof(UpdateDueDateOnTrelloCard), new()
        {
            { nameof(card), card.Id },
            { nameof(due_date), due_date },
        });

        try
        {
            if (!due_date.HasValue || due_date.Value == DateTimeOffset.MinValue || due_date.Value == DateTimeOffset.MaxValue)
            {
                await trelloClient.UpdateCardAsync(card.Id, [new QueryParameter("due", (DateTimeOffset?)null)]);
            }
            else
            {
                await trelloClient.SetDueDateOnCardAsync(card.Id, due_date.Value);
            }
        }
        catch (Exception e)
        {
            LogError(e);
            throw;
        }
    }

    [UsedImplicitly]
    [KernelFunction("update_start_date_on_card")]
    [Description("Update the start_date on a specific card")]
    public async Task UpdateStartDateOnTrelloCard([Required] TrelloCard card, [Required] DateTimeOffset? start_date)
    {
        LogDebug(nameof(UpdateStartDateOnTrelloCard), new()
        {
            { nameof(card), card.Id },
            { nameof(start_date), start_date },
        });

        try
        {
            if (!start_date.HasValue || start_date.Value == DateTimeOffset.MinValue || start_date.Value == DateTimeOffset.MaxValue)
            {
                await trelloClient.UpdateCardAsync(card.Id, [new QueryParameter("start", (DateTimeOffset?)null)]);
            }
            else
            {
                await trelloClient.SetStartDateOnCardAsync(card.Id, start_date.Value);
            }
        }
        catch (Exception e)
        {
            LogError(e);
            throw;
        }
    }

    [UsedImplicitly]
    [KernelFunction("add_label_to_trello_card")]
    [Description("Add a label to a specific card")]
    public async Task AddLabelToTrelloCard([Required] TrelloCard card, [Required, Description("Call 'get_trello_labels' to get valid values")] TrelloLabel label)
    {
        LogDebug(nameof(AddLabelToTrelloCard), new()
        {
            { nameof(card), card.Id },
            { nameof(label), label.Id },
        });

        try
        {
            await trelloClient.AddLabelsToCardAsync(card.Id, label.Id);
        }
        catch (Exception e)
        {
            LogError(e);
            throw;
        }
    }

    [UsedImplicitly]
    [KernelFunction("remove_label_from_trello_card")]
    [Description("Remove a label from a specific card")]
    public async Task RemoveLabelFromTrelloCard([Required] TrelloCard card, [Required, Description("Call 'get_trello_labels' to get valid values")] TrelloLabel label)
    {
        LogDebug(nameof(RemoveLabelFromTrelloCard), new()
        {
            { nameof(card), card.Id },
            { nameof(label), label.Id },
        });

        try
        {
            await trelloClient.RemoveLabelsFromCardAsync(card.Id, label.Id);
        }
        catch (Exception e)
        {
            LogError(e);
            throw;
        }
    }

    [UsedImplicitly]
    [KernelFunction("add_member_to_trello_card")]
    [Description("Add a member to a specific card")]
    public async Task AddMemberToTrelloCard([Required] TrelloCard card, [Required, Description("Call 'get_trello_members' to get valid values")] TrelloMember member)
    {
        LogDebug(nameof(AddMemberToTrelloCard), new()
        {
            { nameof(card), card.Id },
            { nameof(member), member.Id },
        });

        try
        {
            await trelloClient.AddMembersToCardAsync(card.Id, member.Id);
        }
        catch (Exception e)
        {
            LogError(e);
            throw;
        }
    }

    [UsedImplicitly]
    [KernelFunction("remove_member_from_trello_card")]
    [Description("Remove a member from a specific card")]
    public async Task RemoveMemberFromTrelloCard([Required] TrelloCard card, [Required, Description("Call 'get_trello_members' to get valid values")] TrelloMember member)
    {
        LogDebug(nameof(RemoveMemberFromTrelloCard), new()
        {
            { nameof(card), card.Id },
            { nameof(member), member.Id },
        });

        try
        {
            await trelloClient.RemoveMembersFromCardAsync(card.Id, member.Id);
        }
        catch (Exception e)
        {
            LogError(e);
            throw;
        }
    }

    [UsedImplicitly]
    [KernelFunction("move_card_to_list")]
    [Description("Move a card to a new list")]
    public async Task MoveCardToList([Required] TrelloCard card, [Required, Description("Call 'get_trello_lists' to get valid values")] TrelloList list)
    {
        LogDebug(nameof(MoveCardToList), new()
        {
            { nameof(card), card.Id },
            { nameof(list), list.Id },
        });

        try
        {
            await trelloClient.MoveCardToListAsync(card.Id, list.Id);
        }
        catch (Exception e)
        {
            LogError(e);
            throw;
        }
    }

    [UsedImplicitly]
    [KernelFunction("mark_due_card_as_complete")]
    [Description("Mark a due card as complete")]
    public async Task MarkCardAsComplete([Required] TrelloCard card)
    {
        LogDebug(nameof(MarkCardAsComplete), new()
        {
            { nameof(card), card.Id },
        });

        try
        {
            await trelloClient.UpdateCardAsync(card.Id, [CardUpdate.DueComplete(true)]);
        }
        catch (Exception e)
        {
            LogError(e);
            throw;
        }
    }

    [UsedImplicitly]
    [KernelFunction("mark_due_card_as_incomplete")]
    [Description("Mark a due card as incomplete")]
    public async Task MarkCardAsInComplete([Required] TrelloCard card)
    {
        LogDebug(nameof(MarkCardAsInComplete), new()
        {
            { nameof(card), card.Id },
        });

        try
        {
            await trelloClient.UpdateCardAsync(card.Id, [CardUpdate.DueComplete(false)]);
        }
        catch (Exception e)
        {
            LogError(e);
            throw;
        }
    }


    [UsedImplicitly]
    [KernelFunction("archive_card")]
    [Description("Archive a card")]
    public async Task ArchiveCard([Required] TrelloCard card)
    {
        LogDebug(nameof(ArchiveCard), new()
        {
            { nameof(card), card.Id },
        });

        try
        {
            await trelloClient.ArchiveCardAsync(card.Id);
        }
        catch (Exception e)
        {
            LogError(e);
            throw;
        }
    }

    [UsedImplicitly]
    [KernelFunction("add_cover_to_card")]
    [Description("Add a cover to a card")]
    public async Task AddCoverToCard([Required] TrelloCard card, CardCoverColor color, CardCoverSize size)
    {
        LogDebug(nameof(AddCoverToCard), new()
        {
            { nameof(card), card.Id },
        });

        try
        {
            await trelloClient.AddCoverToCardAsync(card.Id, new CardCover(color, size));
        }
        catch (Exception e)
        {
            LogError(e);
            throw;
        }
    }

    [UsedImplicitly]
    [KernelFunction("remove_cover_from_card")]
    [Description("Remove a cover to a card")]
    public async Task RemoveCoverFromCard([Required] TrelloCard card)
    {
        LogDebug(nameof(RemoveCoverFromCard), new()
        {
            { nameof(card), card.Id },
        });

        try
        {
            await trelloClient.RemoveCoverFromCardAsync(card.Id);
        }
        catch (Exception e)
        {
            LogError(e);
            throw;
        }
    }

    private async Task<List[]> GetListsRaw(string caller)
    {
        try
        {
            LogDebug(caller);
            var data = await trelloClient.GetListsOnBoardAsync(GetBoardToUse());
            return data.ToArray();
        }
        catch (Exception e)
        {
            LogError(e);
            throw;
        }
    }

    private async Task<Board[]> GetBoardsRaw(string caller)
    {
        try
        {
            LogDebug(caller);
            IEnumerable<Board> data = await trelloClient.GetBoardsCurrentTokenCanAccessAsync(new GetBoardOptions
            {
                Filter = GetBoardOptionsFilter.Open,
                BoardFields = new BoardFields(BoardFieldsType.Name, BoardFieldsType.ShortUrl)
            });
            return data.ToArray();
        }
        catch (Exception e)
        {
            LogError(e);
            throw;
        }
    }

    private void LogError(Exception exception)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(exception);
        Console.ForegroundColor = ConsoleColor.White;
    }

    private async Task<TrelloCard[]> GetTrelloCardsRaw(string caller, bool onlyAssignedToCurrentUser = false, bool isDueToday = false, bool isOverdue = false)
    {
        try
        {
            LogDebug(caller, new()
            {
                { nameof(onlyAssignedToCurrentUser), onlyAssignedToCurrentUser },
                { nameof(isDueToday), onlyAssignedToCurrentUser },
                { nameof(isOverdue), isOverdue }
            });

            IEnumerable<Card> cards = await trelloClient.GetCardsOnBoardAsync(GetBoardToUse(), new GetCardOptions
            {
                CardFields = new CardFields(CardFieldsType.Due, CardFieldsType.Start, CardFieldsType.DueComplete, CardFieldsType.Name, CardFieldsType.LabelIds, CardFieldsType.Labels, CardFieldsType.MemberIds, CardFieldsType.ShortUrl),
                IncludeList = true,
                IncludeMembers = true,
                MemberFields = new MemberFields(MemberFieldsType.FullName)
            });

            var trelloCards = cards.Select(x => TrelloCard.FromCard(x, currentUser));
            if (onlyAssignedToCurrentUser)
            {
                trelloCards = trelloCards.Where(x => x.IsAssignedToUser);
            }

            if (isDueToday)
            {
                trelloCards = trelloCards.Where(x => x.IsDueToday);
            }

            if (isOverdue)
            {
                trelloCards = trelloCards.Where(x => x.IsOverDue);
            }

            return trelloCards.ToArray();
        }
        catch (Exception e)
        {
            LogError(e);
            throw;
        }
    }

    private async Task<Label[]> GetLabelsRaw(string caller)
    {
        LogDebug(caller);

        var data = await trelloClient.GetLabelsOfBoardAsync(GetBoardToUse());
        return data.ToArray();
    }

    private async Task<Member[]> GetMembersRaw(string caller)
    {
        LogDebug(caller);
        var data = await trelloClient.GetMembersOfBoardAsync(GetBoardToUse());
        return data.ToArray();
    }

    private string GetBoardToUse()
    {
        if (CurrentBoard == null)
        {
            throw new Exception("Settings indicate that it is not allowed to change board, but no board was defined");
        }

        return CurrentBoard.Id;
    }

    private void LogDebug(string? message, Dictionary<string, object?>? args = null)
    {
        if (debugMode)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(args == null
                ? $"-- {message} called"
                : $"-- {message} called (args: {string.Join(", ", args.Where(x => x.Value != null).Select(x =>
                {
                    if (x.Value!.GetType() == typeof(string[]))
                    {
                        return $"'{x.Key}'='{string.Join(",", (string[])x.Value!)}'";
                    }

                    if (x.Value!.GetType() == typeof(TrelloLabel[]))
                    {
                        return $"'{x.Key}'='{string.Join(",", ((TrelloLabel[])x.Value!).Select(y => y.ToString()))}'";
                    }

                    if (x.Value!.GetType() == typeof(TrelloList[]))
                    {
                        return $"'{x.Key}'='{string.Join(",", ((TrelloList[])x.Value!).Select(y => y.ToString()))}'";
                    }

                    if (x.Value!.GetType() == typeof(TrelloMember[]))
                    {
                        return $"'{x.Key}'='{string.Join(",", ((TrelloMember[])x.Value!).Select(y => y.ToString()))}'";
                    }

                    return $"'{x.Key}'='{x.Value}'";
                }))})");
            Console.ForegroundColor = ConsoleColor.White;
        }
    }
}