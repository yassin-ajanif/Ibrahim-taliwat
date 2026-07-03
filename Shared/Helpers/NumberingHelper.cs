namespace GestionCommerciale.Shared.Helpers;

public static class NumberingHelper
{
    public static string Generate(string prefix, int lastNumber, int year) =>
        $"{prefix}-{year}-{(lastNumber + 1):D4}";
}
