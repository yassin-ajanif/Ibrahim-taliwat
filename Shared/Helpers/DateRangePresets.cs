namespace GestionCommerciale.Shared.Helpers;

public enum DateRangePreset
{
    Today,
    Week,
    Month,
    Year
}

public static class DateRangePresets
{
    public static (DateTime From, DateTime To) GetRange(DateRangePreset preset)
    {
        var today = DateTime.Today;
        return preset switch
        {
            DateRangePreset.Today => (today, today),
            DateRangePreset.Week => (StartOfWeek(today), today),
            DateRangePreset.Month => (new DateTime(today.Year, today.Month, 1), today),
            DateRangePreset.Year => (new DateTime(today.Year, 1, 1), today),
            _ => (today, today)
        };
    }

    private static DateTime StartOfWeek(DateTime date)
    {
        var diff = (int)date.DayOfWeek - (int)DayOfWeek.Monday;
        if (diff < 0) diff += 7;
        return date.AddDays(-diff);
    }
}
