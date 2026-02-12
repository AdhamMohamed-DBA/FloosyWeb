using System.Collections.ObjectModel;

namespace FloosyWeb.Models;

public class AppData
{
    public ObservableCollection<Account> Accounts { get; set; } = [];
    public ObservableCollection<Bill> Bills { get; set; } = [];
    public ObservableCollection<Transaction> History { get; set; } = [];

    // People balances (who owes you / who you owe)
    public ObservableCollection<Person> People { get; set; } = [];
    public ObservableCollection<PersonTransaction> PeopleHistory { get; set; } = [];
    public ObservableCollection<string> SavedContactNames { get; set; } = [];

    public ObservableCollection<string> IncomeCategories { get; set; } = [];
    public ObservableCollection<string> ExpenseCategories { get; set; } = [];
    public ObservableCollection<string> BillCategories { get; set; } = [];

    public int FinancialStartDay { get; set; } = 1;

    // Legacy (kept for backward compatibility with old saved payloads)
    public string UpdateVersion { get; set; } = "";
    public string UpdateMessage { get; set; } = "";
    public bool IsUpdateRequired { get; set; } = false;
}

public class UpdateBroadcast
{
    public string Version { get; set; } = "";
    public string Message { get; set; } = "";
    public string MessageAlignment { get; set; } = "ltr";
    public bool IsRequired { get; set; } = false;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public class Account
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string? Name { get; set; }
    public decimal Balance { get; set; }
    public string ColorHex { get; set; } = "#1f1f1f";
    public int SortOrder { get; set; } = 0;
}

public class Bill
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string? Name { get; set; }
    public decimal Amount { get; set; }
    public bool IsPaid { get; set; } = false;
    public DateTime DueDate { get; set; } = DateTime.Now;
    public DateTime? PaymentDate { get; set; }
    public string Category { get; set; } = "Bills";
    public string Notes { get; set; } = "";

    public string Frequency { get; set; } = "OneTime";
    public bool IsRecurring => Frequency != "OneTime";
    public bool IsShared { get; set; } = false;
    public string SharedWith { get; set; } = "";
    public ObservableCollection<BillParticipant> Participants { get; set; } = [];
}

public class BillParticipant
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = "";
    public decimal Amount { get; set; }
    public bool IsPaid { get; set; }
    public DateTime? PaidAt { get; set; }
}

public class Transaction
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string? Desc { get; set; }
    public string Category { get; set; } = "Other";
    public decimal Amount { get; set; }
    public string? AmountDisplay { get; set; }
    public string? ColorHex { get; set; }
    public DateTime Date { get; set; } = DateTime.Now;
}

public class Person
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = "";

    /// <summary>
    /// Net balance with this person:
    ///  - Positive  => they owe you money (فلوس ليك).
    ///  - Negative  => you owe them money (فلوس عليك).
    /// </summary>
    public decimal Balance { get; set; }
}

public class PersonTransaction
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string PersonId { get; set; } = "";
    public string PersonName { get; set; } = "";
    public decimal Amount { get; set; }

    /// <summary>
    /// true  => money coming from the person to you (collect).
    /// false => money going from you to the person (pay).
    /// </summary>
    public bool IsFromPerson { get; set; }

    public string? Note { get; set; }
    public DateTime Date { get; set; } = DateTime.Now;

    /// <summary>
    /// Optional link to the main Transaction entry in History.
    /// </summary>
    public string? TransactionId { get; set; }
}

public class ChartItem
{
    public string Name { get; set; } = "";
    public double Pct { get; set; }
    public string AmountStr { get; set; } = "";
    public string Color { get; set; } = "";
}