using System.Text.RegularExpressions;

namespace MetroBalance.Core.Interfaces;

/// <summary>
/// Validates and normalises Delhi Metro Smart Card numbers.
/// </summary>
public interface ICardValidator
{
    /// <summary>Returns true when the supplied number is a plausible card number.</summary>
    bool IsValid(string? cardNumber);

    /// <summary>Normalises the number (trim + upper case) for dictionary lookups.</summary>
    string Normalize(string cardNumber);

    /// <summary>Returns a human readable validation message, or null when valid.</summary>
    string? GetValidationMessage(string? cardNumber);
}

/// <summary>Default card number validator used by the application.</summary>
public sealed partial class CardValidator : ICardValidator
{
    // Demo cards use readable identifiers such as "DMRC-DEMO-1001"; real DMRC card
    // numbers are numeric strings, and bank card numbers can be up to 19 digits.
    // The pattern is applied to the NORMALIZED value (spaces removed), so the
    // 6-19 limit counts digits only. The JavaScript validator in
    // wwwroot/js/site.js must stay identical to this pattern.
    [GeneratedRegex(@"^(?=.{6,19}$)[A-Za-z0-9]+([-\s][A-Za-z0-9]+)*$", RegexOptions.CultureInvariant)]
    private static partial Regex CardNumberPattern();

    public bool IsValid(string? cardNumber)
    {
        if (string.IsNullOrWhiteSpace(cardNumber))
        {
            return false;
        }

        var normalized = Normalize(cardNumber);
        return CardNumberPattern().IsMatch(normalized);
    }

    public string Normalize(string cardNumber) =>
        (cardNumber ?? string.Empty).Trim().ToUpperInvariant().Replace(" ", string.Empty);

    public string? GetValidationMessage(string? cardNumber)
    {
        if (string.IsNullOrWhiteSpace(cardNumber))
        {
            return "Please enter your Metro Smart Card number.";
        }

        var normalized = Normalize(cardNumber);
        if (!CardNumberPattern().IsMatch(normalized))
        {
            return "Card number must be 6-19 characters and may contain letters, digits and hyphens only.";
        }

        return null;
    }
}
