namespace MetroBalance.Web.Models;

/// <summary>
/// View model for the homepage hero card. The sample card is read from the same
/// in-memory provider the API uses, so the homepage can never drift from the
/// numbers the API actually returns.
/// </summary>
public sealed class HomeIndexViewModel
{
    /// <summary>Card holder of the sample card.</summary>
    public string CardHolderName { get; set; } = "Aarav Sharma";

    /// <summary>Card type of the sample card.</summary>
    public string CardType { get; set; } = "Silver";

    /// <summary>Balance of the sample card, formatted as Indian currency.</summary>
    public string BalanceFormatted { get; set; } = "₹ 2,725.50";

    /// <summary>Last journey of the sample card, e.g. "Jangpura → Moolchand".</summary>
    public string LastJourney { get; set; } = "Jangpura → Moolchand";

    /// <summary>When the sample card data was last refreshed.</summary>
    public string LastUpdatedFormatted { get; set; } = "—";
}
