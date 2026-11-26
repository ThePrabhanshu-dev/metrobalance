using MetroBalance.Core.Models;

namespace MetroBalance.Core.Interfaces;

/// <summary>
/// Abstraction over the source of Metro Smart Card data.
/// Implementations may talk to an authorised DMRC/DMRC-authorised service or,
/// as in this project, return clearly-labelled simulated data.
/// </summary>
public interface IMetroCardProvider
{
    /// <summary>Human readable name of the provider (e.g. "Simulated Provider").</summary>
    string ProviderName { get; }

    /// <summary>True when the returned data is simulated rather than live.</summary>
    bool IsSimulated { get; }

    /// <summary>Looks up a single card by its number. Returns null when not found.</summary>
    Task<SmartCard?> GetCardAsync(string cardNumber, CancellationToken cancellationToken = default);

    /// <summary>Returns the list of demo card numbers that can be used for a trial.</summary>
    Task<IReadOnlyList<string>> GetDemoCardNumbersAsync(CancellationToken cancellationToken = default);
}
