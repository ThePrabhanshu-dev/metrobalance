using MetroBalance.Core.Models;

namespace MetroBalance.Infrastructure.Data;

/// <summary>
/// Builds the demo Smart Cards used by the application. All values are simulated
/// and clearly labelled as DEMO DATA by the provider and API layer.
/// </summary>
public static class DemoCardDataFactory
{
    private static readonly string[] Stations =
    [
        "Rajiv Chowk", "Central Secretariat", "Udyog Bhawan", "Lok Kalyan Marg", "Jor Bagh",
        "INA", "AIIMS", "Green Park", "Hauz Khas", "Malviya Nagar", "Saket", "Qutab Minar",
        "Chhatarpur", "Sultanpur", "Ghitorni", "Arjan Garh", "Guru Dronacharya", "Sikanderpur",
        "MG Road", "Bahadur Shah Zafar Marg", "Lajpat Nagar", "Jangpura", "Patel Chowk",
        "Moolchand", "Nehru Place", "Kalkaji Mandir", "Govind Puri", "Hauz Khas", "Azadpur",
        "Model Town", "Shastri Nagar", "Kanhaiya Nagar", "Chandni Chowk", "Chawri Bazar"
    ];

    private static readonly string[] Lines =
    [
        "Blue Line", "Yellow Line", "Red Line", "Violet Line", "Pink Line", "Green Line"
    ];

    private static readonly string[] Modes = ["UPI", "Net Banking", "Debit Card", "Credit Card", "Cash (FCB)", "Wallet"];

    private static readonly string[] Channels =
    [
        "Delhi Metro App", "Internet Banking", "Paytm Wallet", "Axis Bank ATM", "DMRC Top-up Counter"
    ];

    private sealed record CardSeed(
        string CardNumber,
        string Holder,
        CardType Type,
        CardStatus Status,
        decimal StartingBalance,
        int JourneyCount,
        int RechargeCount,
        int Seed);

    private static readonly CardSeed[] Seeds =
    [
        new("DMRC-DEMO-1001", "Aarav Sharma", CardType.Silver, CardStatus.Active, 425.50m, 12, 4, 1101),
        new("DMRC-DEMO-1002", "Priya Nair", CardType.Gold, CardStatus.Active, 1280.75m, 18, 6, 1102),
        new("DMRC-DEMO-1003", "Rahul Verma", CardType.Silver, CardStatus.Active, 62.00m, 9, 2, 1103),
        new("DMRC-DEMO-1004", "Sneha Iyer", CardType.Tourist, CardStatus.Active, 350.00m, 6, 3, 1104),
        new("DMRC-DEMO-1005", "Vikram Singh", CardType.Staff, CardStatus.Active, 2450.00m, 24, 8, 1105),
        new("DMRC-DEMO-1006", "Ananya Gupta", CardType.Gold, CardStatus.Blocked, 310.25m, 7, 3, 1106),
        new("DMRC-DEMO-1007", "Manish Yadav", CardType.Silver, CardStatus.Expired, 15.00m, 4, 1, 1107),
        new("DMRC-DEMO-1008", "Kavya Menon", CardType.Silver, CardStatus.Active, 890.25m, 15, 5, 1108)
    ];

    public static IReadOnlyList<SmartCard> Create()
    {
        var now = DateTime.Now;
        var cards = new List<SmartCard>(Seeds.Length);

        foreach (var seed in Seeds)
        {
            cards.Add(BuildCard(seed, now));
        }

        return cards;
    }

    private static SmartCard BuildCard(CardSeed seed, DateTime now)
    {
        var random = new Random(seed.Seed);
        var card = new SmartCard
        {
            CardNumber = seed.CardNumber,
            CardHolderName = seed.Holder,
            CardType = seed.Type,
            Status = seed.Status,
            IssuedOn = now.AddDays(-random.Next(120, 900)),
            Balance = seed.StartingBalance
        };

        // Build the journey history newest-first, walking backwards in time.
        var journeyDates = BuildTimestamps(now, seed.JourneyCount, random, minGapHours: 6, maxGapHours: 60);
        var balance = seed.StartingBalance + seed.RechargeCount * 200m;

        for (var i = 0; i < journeyDates.Count; i++)
        {
            var fare = FareSlabs[random.Next(FareSlabs.Length)];
            var from = Stations[random.Next(Stations.Length)];
            var to = Stations[random.Next(Stations.Length)];
            while (to == from)
            {
                to = Stations[random.Next(Stations.Length)];
            }

            balance -= fare;

            card.Transactions.Add(new CardTransaction
            {
                TransactionId = $"TXN{seed.Seed}{i + 1:000}",
                Timestamp = journeyDates[i],
                FromStation = from,
                ToStation = to,
                Line = Lines[random.Next(Lines.Length)],
                Amount = fare,
                BalanceAfter = balance,
                FareSlab = Array.IndexOf(FareSlabs, fare) + 1,
                EntryGate = $"G{random.Next(1, 60):00}",
                ExitGate = $"G{random.Next(1, 60):00}"
            });
        }

        // Recharges are credited on top of the journey spend.
        var rechargeDates = BuildTimestamps(now, seed.RechargeCount, random, minGapHours: 24, maxGapHours: 240);
        for (var i = 0; i < rechargeDates.Count; i++)
        {
            var amount = new[] { 100m, 200m, 250m, 500m, 1000m }[random.Next(5)];
            balance += amount;

            card.Recharges.Add(new RechargeTransaction
            {
                TransactionId = $"RCH{seed.Seed}{i + 1:000}",
                Timestamp = rechargeDates[i],
                Amount = amount,
                BalanceAfter = balance,
                Mode = Modes[random.Next(Modes.Length)],
                Channel = Channels[random.Next(Channels.Length)],
                Remarks = i % 2 == 0 ? "Online top-up" : "Scheduled auto top-up"
            });
        }

        card.Transactions.Sort((a, b) => b.Timestamp.CompareTo(a.Timestamp));
        card.Recharges.Sort((a, b) => b.Timestamp.CompareTo(a.Timestamp));

        card.Balance = balance < 0 ? 0 : balance;
        card.LastUpdatedOn = card.Transactions.Count > 0
            ? card.Transactions.Max(t => t.Timestamp)
            : now;

        return card;
    }

    private static readonly decimal[] FareSlabs = [10m, 20m, 30m, 40m, 50m, 60m];

    private static List<DateTime> BuildTimestamps(DateTime now, int count, Random random, int minGapHours, int maxGapHours)
    {
        var result = new List<DateTime>(count);
        var cursor = now.AddHours(-random.Next(2, 12));

        for (var i = 0; i < count; i++)
        {
            cursor = cursor.AddHours(-random.Next(minGapHours, maxGapHours));
            result.Add(cursor);
        }

        return result;
    }
}
