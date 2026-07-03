namespace GestionCommerciale.Modules.Reporting.ViewModels;

public sealed class ReportRankRow
{
    public ReportRankRow(string label, string valueText, double barShare)
    {
        Label = label;
        ValueText = valueText;
        BarShare = Math.Clamp(barShare, 0, 1);
    }

    public string Label { get; }
    public string ValueText { get; }
    public double BarShare { get; }

    /// <summary>Width for horizontal bar (px).</summary>
    public double BarWidth => Math.Max(3, BarShare * 220);
}

public sealed class ReportStockAlertRow
{
    public ReportStockAlertRow(string reference, string detail)
    {
        Reference = reference;
        Detail = detail;
    }

    public string Reference { get; }
    public string Detail { get; }
}

public sealed class ReportUnpaidRow
{
    public ReportUnpaidRow(string numero, string reste, string dateEcheance, string dueStatus, bool isOverdue, bool isDueSoon)
    {
        Numero = numero;
        Reste = reste;
        DateEcheance = dateEcheance;
        DueStatus = dueStatus;
        IsOverdue = isOverdue;
        IsDueSoon = isDueSoon;
    }

    public string Numero { get; }
    public string Reste { get; }
    public string DateEcheance { get; }
    public string DueStatus { get; }
    public bool IsOverdue { get; }
    public bool IsDueSoon { get; }

    public bool ShowStripOverdue => IsOverdue;
    public bool ShowStripSoon => !IsOverdue && IsDueSoon;
    public bool ShowStripNeutral => !IsOverdue && !IsDueSoon;
}
