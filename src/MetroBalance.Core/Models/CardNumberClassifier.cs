using MetroBalance.Core.Contracts;

namespace MetroBalance.Core.Models;

/// <summary>
/// Works out what kind of identifier a user typed in. DMRC prints a 10-11 digit
/// "Engraved ID" on the back of every smart card and uses a 12 digit number on the
/// front, while 13-19 digit numbers in 4-4-4-4 groups are the shape of a bank card.
/// </summary>
public static class CardNumberClassifier
{
    public static CardNumberKind Classify(string? cardNumber)
    {
        var raw = (cardNumber ?? string.Empty).Trim();

        if (raw.Length == 0)
        {
            return CardNumberKind.Unknown;
        }

        if (raw.Contains("DEMO", StringComparison.OrdinalIgnoreCase))
        {
            return CardNumberKind.DemoCard;
        }

        var digits = new string(raw.Where(char.IsAsciiDigit).ToArray());

        if (digits.Length == 0)
        {
            return CardNumberKind.Unknown;
        }

        // DMRC identifiers are numeric only; letters only appear on demo cards.
        if (raw.Any(char.IsAsciiLetter))
        {
            return CardNumberKind.Unknown;
        }

        return digits.Length switch
        {
            >= 10 and <= 11 => CardNumberKind.EngravedId,
            12 => CardNumberKind.SmartCardNumber,
            >= 13 and <= 19 => CardNumberKind.BankStyleNumber,
            _ => CardNumberKind.Unknown
        };
    }

    public static string Describe(CardNumberKind kind) => kind switch
    {
        CardNumberKind.DemoCard =>
            "MetroBalance demo card (simulated data).",
        CardNumberKind.EngravedId =>
            "DMRC Engraved ID - 10 or 11 digits printed on the back of the card, used for web top-ups.",
        CardNumberKind.SmartCardNumber =>
            "DMRC Smart Card number - 12 digits printed on the front of the card.",
        CardNumberKind.BankStyleNumber =>
            "This looks like a bank card number (13-19 digits). A DMRC card carries a 10-11 digit Engraved ID, " +
            "so please check the number printed on the back of your metro card.",
        _ =>
            "Unrecognised card number format."
    };
}
