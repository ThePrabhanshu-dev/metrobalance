using MetroBalance.Core.Interfaces;
using MetroBalance.Core.Models;
using MetroBalance.Infrastructure.Data;

namespace MetroBalance.Infrastructure.Providers;

/// <summary>
/// Simulated provider backed by in-memory demo data.
/// <para>
/// This implementation intentionally performs no DMRC scraping and calls no external
/// endpoint. It exists so the architecture can be swapped for an authorised DMRC
/// integration later by implementing <see cref="IMetroCardProvider"/>.
/// </para>
/// </summary>
public sealed class MockMetroCardProvider : IMetroCardProvider
{
    private readonly InMemoryCardStore _store;

    public MockMetroCardProvider(InMemoryCardStore store)
    {
        _store = store;
    }

    public string ProviderName => "Simulated (DEMO DATA)";

    public bool IsSimulated => true;

    public Task<SmartCard?> GetCardAsync(string cardNumber, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var card = _store.Find(cardNumber);
        return Task.FromResult(card);
    }

    public Task<IReadOnlyList<string>> GetDemoCardNumbersAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(_store.CardNumbers);
    }
}
