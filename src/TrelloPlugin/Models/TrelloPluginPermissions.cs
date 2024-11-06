namespace TrelloPlugin.Models;

internal class TrelloPluginPermissions
{
    public bool AllowBoardChange { get; set; }
    public bool AllowAddAndRemoveOfLabelsOnCards { get; set; }
    public bool AllowAddAndRemoveOfMembersToCards { get; set; }
    public bool AllowToAddCards { get; set; }
    public bool AllowUpdateOfCards { get; set; }
    public bool AllowArchiveOfCards { get; set; }
}