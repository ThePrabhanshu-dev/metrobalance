using MetroBalance.Core.Models;

namespace MetroBalance.Infrastructure.Data;

/// <summary>
/// In-memory store of Smart Cards backed by a <see cref="Dictionary{TKey,TValue}"/>.
/// Data lives for the lifetime of the process only - there is no database.
/// </summary>
public sealed class InMemoryCardStore
{
    private readonly Dictionary<string, SmartCard> _cards;

    public InMemoryCardStore()
    {
        _cards = DemoCardDataFactory.Create().ToDictionary(
            card => card.CardNumber,
            StringComparer.OrdinalIgnoreCase);
    }

    public IReadOnlyDictionary<string, SmartCard> Cards => _cards;

    public SmartCard? Find(string cardNumber) =>
        _cards.TryGetValue(cardNumber, out var card) ? card : null;

    public bool Contains(string cardNumber) => _cards.ContainsKey(cardNumber);

    public IReadOnlyList<string> CardNumbers => [.. _cards.Keys.OrderBy(k => k)];
}
