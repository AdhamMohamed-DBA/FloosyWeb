using System.Collections.ObjectModel;

namespace FloosyWeb.Models;

public class AppData
{
    public ObservableCollection<Account> Accounts { get; set; } = new();
    public ObservableCollection<Bill> Bills { get; set; } = new();
    public ObservableCollection<Transaction> History { get; set; } = new();

    public ObservableCollection<string> IncomeCategories { get; set; } = new();
    public ObservableCollection<string> ExpenseCategories { get; set; } = new();
    public ObservableCollection<string> BillCategories { get; set; } = new();
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

public class ChartItem
{
    public string Name { get; set; } = "";
    public double Pct { get; set; }
    public string AmountStr { get; set; } = "";
    public string Color { get; set; } = "";
}