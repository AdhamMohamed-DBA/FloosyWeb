using System.Collections.ObjectModel;

namespace FloosyWeb.Models;

public class AppData
{
    public ObservableCollection<Account> Accounts { get; set; } = [];
    public ObservableCollection<CreditCard> CreditCards { get; set; } = [];
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
    // News/update version used for one-time popup tracking
    public string Version { get; set; } = "";

    // Minimum required app version for force-update prompt
    public string RequiredVersion { get; set; } = "";

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

public class CreditCard
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = "";

    /// <summary>
    /// Total credit limit for this card.
    /// </summary>
    public decimal CreditLimit { get; set; }

    /// <summary>
    /// Current used amount on the card (amount you owe to the bank).
    /// </summary>
    public decimal OutstandingBalance { get; set; }

    /// <summary>
    /// Statement closing day in month. Kept between 1..28 for date safety.
    /// </summary>
    public int StatementDay { get; set; } = 25;

    /// <summary>
    /// Number of days after statement date until payment due date.
    /// Usually around 20~25 days (55-day style total cycle behavior).
    /// </summary>
    public int GraceDays { get; set; } = 25;

    public string ColorHex { get; set; } = "#7E57C2";
    public int SortOrder { get; set; }

    public decimal AvailableCredit => CreditLimit - OutstandingBalance;
}

public class Bill
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string? Name { get; set; }
    public decimal Amount { get; set; }
    public bool IsPaid { get; set; } = false;
    public DateTime DueDate { get; set; } = DateTime.Now;
    public DateTime? EndDate { get; set; }
    public DateTime? PaymentDate { get; set; }
    public string Category { get; set; } = "Bills";
    public string Notes { get; set; } = "";

    public string Frequency { get; set; } = "OneTime";
    public bool IsRecurring => Frequency != "OneTime";
    public bool IsShared { get; set; } = false;
    public string SharedWith { get; set; } = "";
    public ObservableCollection<BillParticipant> Participants { get; set; } = [];

    // Installment metadata (used for auto-generated credit cash-out installments)
    public bool IsInstallment { get; set; } = false;
    public string InstallmentPlanId { get; set; } = "";
    public int InstallmentNumber { get; set; }
    public int InstallmentTotalCount { get; set; }
    public string? LinkedCreditCardId { get; set; }
    public string? LinkedCreditCardName { get; set; }
    public string? LinkedTransactionId { get; set; }
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
    public string Kind { get; set; } = "";
    public string? Desc { get; set; }
    public string Category { get; set; } = "Other";
    public decimal Amount { get; set; }
    public string? AmountDisplay { get; set; }
    public string? ColorHex { get; set; }

    // Account metadata for analytics/history filtering.
    // Backward compatible: old records simply keep these null/empty.
    public string? PrimaryAccountId { get; set; }
    public string? PrimaryAccountName { get; set; }
    public string? TargetAccountId { get; set; }
    public string? TargetAccountName { get; set; }

    // Credit-card metadata (for isolated credit tracking and filtering)
    public string? PrimaryCreditCardId { get; set; }
    public string? PrimaryCreditCardName { get; set; }
    public string? TargetCreditCardId { get; set; }
    public string? TargetCreditCardName { get; set; }

    /// <summary>
    /// The statement date this credit transaction belongs to.
    /// </summary>
    public DateTime? CreditStatementDate { get; set; }

    /// <summary>
    /// Due date to pay this credit transaction cycle to avoid interest.
    /// </summary>
    public DateTime? CreditDueDate { get; set; }

    /// <summary>
    /// True for purchase-like operations that benefit from statement + grace window.
    /// False for cash-advance like operations (interest can start immediately).
    /// </summary>
    public bool IsCreditGraceEligible { get; set; } = true;

    // Installment linkage metadata
    public bool IsInstallmentRelated { get; set; } = false;
    public string? InstallmentPlanId { get; set; }
    public int? InstallmentNumber { get; set; }
    public int? InstallmentTotalCount { get; set; }
    public string? LinkedBillId { get; set; }

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

public static class FinancialMonthHelper
{
    public static int NormalizeStartDay(int startDay)
    {
        if (startDay < 1) return 1;
        if (startDay > 28) return 28;
        return startDay;
    }

    /// <summary>
    /// Returns the financial month key as first day of month.
    /// Example: startDay=25, date=2026-02-25 => key=2026-03-01.
    /// </summary>
    public static DateTime GetFinancialMonthKey(DateTime date, int startDay)
    {
        var normalizedStartDay = NormalizeStartDay(startDay);
        var key = new DateTime(date.Year, date.Month, 1);

        if (date.Day >= normalizedStartDay)
        {
            key = key.AddMonths(1);
        }

        return key;
    }
}

public static class CreditCycleHelper
{
    public static int NormalizeDay(int day)
    {
        if (day < 1) return 1;
        if (day > 28) return 28;
        return day;
    }

    public static int NormalizeGraceDays(int graceDays)
    {
        if (graceDays < 0) return 0;
        if (graceDays > 90) return 90;
        return graceDays;
    }

    /// <summary>
    /// Returns statement date for transaction date.
    /// If tx day <= statement day => same month statement.
    /// Else => next month statement.
    /// </summary>
    public static DateTime GetStatementDateForTransaction(DateTime transactionDate, int statementDay)
    {
        var d = NormalizeDay(statementDay);
        var sameMonthStatement = new DateTime(transactionDate.Year, transactionDate.Month, d);
        if (transactionDate.Date <= sameMonthStatement.Date)
            return sameMonthStatement;

        var nextMonth = transactionDate.AddMonths(1);
        return new DateTime(nextMonth.Year, nextMonth.Month, d);
    }

    public static DateTime GetDueDateForTransaction(DateTime transactionDate, int statementDay, int graceDays)
    {
        var statementDate = GetStatementDateForTransaction(transactionDate, statementDay);
        return statementDate.AddDays(NormalizeGraceDays(graceDays));
    }

    /// <summary>
    /// Cash-advance style due date (no purchase grace days).
    /// The amount lands on the statement close date directly.
    /// </summary>
    public static DateTime GetCashAdvanceDueDateForTransaction(DateTime transactionDate, int statementDay)
    {
        return GetStatementDateForTransaction(transactionDate, statementDay);
    }

    public static DateTime GetDueDateForCreditTransaction(
        DateTime transactionDate,
        int statementDay,
        int graceDays,
        bool isGraceEligible)
    {
        return isGraceEligible
            ? GetDueDateForTransaction(transactionDate, statementDay, graceDays)
            : GetCashAdvanceDueDateForTransaction(transactionDate, statementDay);
    }
}