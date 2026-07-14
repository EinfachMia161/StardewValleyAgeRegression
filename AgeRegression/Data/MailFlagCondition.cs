namespace AgeRegression.Data;

/// <summary>
/// Immutable unlock condition requiring that the player has received the
/// specified mail flag.
/// </summary>
public sealed class MailFlagCondition : IUnlockCondition
{
    /// <summary>Required mail flag ID.</summary>
    public string MailFlag { get; }

    public MailFlagCondition(string mailFlag)
    {
        MailFlag = mailFlag;
    }
}
