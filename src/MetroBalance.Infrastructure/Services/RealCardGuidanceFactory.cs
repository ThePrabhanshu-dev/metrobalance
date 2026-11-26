using MetroBalance.Core.Contracts;

namespace MetroBalance.Infrastructure.Services;

/// <summary>
/// Builds the guidance shown for a card that MetroBalance cannot read online.
/// Every link points at an official DMRC property - nothing here is scraped,
/// invented or simulated as if it were a live balance.
/// </summary>
public static class RealCardGuidanceFactory
{
    public const string DataSource = "Official DMRC channels (no online balance service published by DMRC)";

    private const string PolicyNote =
        "DMRC does not publish a service that returns a card balance over the internet, so no website - including " +
        "MetroBalance - can show a live balance for your card. MetroBalance never invents a balance. The balance " +
        "is read from the card itself at an Add Value Machine, at a Customer Care counter, or from a journey receipt.";

    public static IReadOnlyList<string> BuildSteps(CardNumberKind kind)
    {
        var steps = new List<string>
        {
            "Walk into any Delhi Metro station - the Airport Express line included.",
            "Find the Add Value Machine (AVM) next to the Customer Care / EFO counter.",
            "Tap your card on the AVM reader and press the Inquiry button.",
            "The machine prints and displays your available balance in rupees.",
            "Need a printed record? Ask at the Customer Care counter - staff can read the balance and the last transactions from the card.",
            "Need to top up? Pay on the official DMRC top-up portal using your Engraved ID, then tap the card on an AVM to collect the value."
        };

        if (kind == CardNumberKind.BankStyleNumber)
        {
            steps.Insert(0,
                "Double-check the number: a DMRC Smart Card carries a 10-11 digit Engraved ID on the back and a 12 digit " +
                "number on the front. The 4-4-4-4 number you entered looks like a bank card number.");
        }
        else if (kind == CardNumberKind.EngravedId)
        {
            steps.Insert(0,
                "The number you entered matches a DMRC Engraved ID - that is the number you use for web top-ups.");
        }

        return steps;
    }

    public static IReadOnlyList<OfficialLink> BuildLinks() =>
    [
        new OfficialLink
        {
            Title = "Top up your card (official DMRC portal)",
            Description = "Pay for a recharge using your Engraved ID, then collect the value at an AVM.",
            Url = "https://www.dmrcsmartcard.com/Citrus_Payment/QuickTopUp.aspx",
            IsPrimary = true
        },
        new OfficialLink
        {
            Title = "DMRC Smart Card portal",
            Description = "Register your card, view registered cards and manage your smart card profile.",
            Url = "https://www.dmrcsmartcard.com/",
            IsPrimary = false
        },
        new OfficialLink
        {
            Title = "Delhi Metro official website",
            Description = "Network map, stations, fares and announcements.",
            Url = "https://www.delhimetrorail.com/",
            IsPrimary = false
        },
        new OfficialLink
        {
            Title = "Delhi Metro Sarthi app (Google Play)",
            Description = "Official app for journey planning, station info and ticket booking.",
            Url = "https://play.google.com/store/apps/details?id=com.sraoss.dmrc",
            IsPrimary = false
        },
        new OfficialLink
        {
            Title = "Delhi Metro Sarthi app (App Store)",
            Description = "The same official app for iPhone and iPad.",
            Url = "https://apps.apple.com/app/id691105308",
            IsPrimary = false
        },
        new OfficialLink
        {
            Title = "Chetna - DMRC chatbot",
            Description = "Official virtual assistant for card and fare queries.",
            Url = "https://chatbot.delhimetrorail.com/",
            IsPrimary = false
        },
        new OfficialLink
        {
            Title = "DMRC customer care",
            Description = "Helpline 011-23417910 / 011-23417911 / 011-23417912, Metro Bhawan, Barakhamba Road, New Delhi 110001.",
            Url = "https://www.delhimetrorail.com/pages/en/contact-us",
            IsPrimary = false
        }
    ];

    public static string PolicyNoteFor(CardNumberKind kind) => kind switch
    {
        CardNumberKind.BankStyleNumber =>
            PolicyNote + " A bank card number cannot be used for metro travel - please use the number on your metro card.",
        _ => PolicyNote
    };
}
