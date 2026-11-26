namespace MetroBalance.Core.Exceptions;

/// <summary>Raised when the supplied card number fails validation.</summary>
public sealed class InvalidCardNumberException(string message) : Exception(message);

/// <summary>Raised when no card exists for the supplied number.</summary>
public sealed class CardNotFoundException(string cardNumber)
    : Exception($"No Metro Smart Card found for number '{cardNumber}'.");

/// <summary>Raised when the card exists but cannot be used for balance enquiries.</summary>
public sealed class CardNotActiveException(string cardNumber, string status)
    : Exception($"Card '{cardNumber}' is {status.ToLowerInvariant()} and cannot be used for a balance enquiry.");
