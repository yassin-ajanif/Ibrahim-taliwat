using System.Text.Json;

namespace GestionCommerciale.Shared.Helpers;

/// <summary>
/// Persists per-prefix, per-year "last number used outside the app" floors for document numbering.
/// JSON shape: { "FAC": { "2026": 55 }, "BL": { "2026": 10 } }
/// </summary>
public static class DocumentNumberingFloorsStorage
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = false };

    public static Dictionary<string, Dictionary<int, int>> Parse(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return new Dictionary<string, Dictionary<int, int>>(StringComparer.OrdinalIgnoreCase);

        try
        {
            var raw = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, int>>>(json, JsonOptions);
            if (raw is null)
                return new Dictionary<string, Dictionary<int, int>>(StringComparer.OrdinalIgnoreCase);

            var result = new Dictionary<string, Dictionary<int, int>>(StringComparer.OrdinalIgnoreCase);
            foreach (var (prefix, years) in raw)
            {
                var yearMap = new Dictionary<int, int>();
                foreach (var (yearKey, value) in years)
                {
                    if (int.TryParse(yearKey, out var year) && value > 0)
                        yearMap[year] = value;
                }

                if (yearMap.Count > 0)
                    result[prefix] = yearMap;
            }

            return result;
        }
        catch
        {
            return new Dictionary<string, Dictionary<int, int>>(StringComparer.OrdinalIgnoreCase);
        }
    }

    public static string Serialize(Dictionary<string, Dictionary<int, int>> floors)
    {
        var raw = floors.ToDictionary(
            kv => kv.Key,
            kv => kv.Value.ToDictionary(y => y.Key.ToString(), y => y.Value),
            StringComparer.OrdinalIgnoreCase);
        return JsonSerializer.Serialize(raw, JsonOptions);
    }

    public static int GetLastUsedOutside(IReadOnlyDictionary<string, Dictionary<int, int>> floors, string prefix, int year)
    {
        if (!floors.TryGetValue(prefix, out var years))
            return 0;
        return years.TryGetValue(year, out var value) ? Math.Max(0, value) : 0;
    }

    public static void SetLastUsedOutside(Dictionary<string, Dictionary<int, int>> floors, string prefix, int year, int lastUsedOutside)
    {
        if (lastUsedOutside <= 0)
        {
            if (floors.TryGetValue(prefix, out var years))
            {
                years.Remove(year);
                if (years.Count == 0)
                    floors.Remove(prefix);
            }

            return;
        }

        if (!floors.TryGetValue(prefix, out var map))
        {
            map = new Dictionary<int, int>();
            floors[prefix] = map;
        }

        map[year] = lastUsedOutside;
    }
}
