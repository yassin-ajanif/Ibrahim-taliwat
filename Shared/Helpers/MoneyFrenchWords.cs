using System.Globalization;

namespace GestionCommerciale.Shared.Helpers;

/// <summary>Convert monetary amounts to French words (dirhams / centimes).</summary>
public static class MoneyFrenchWords
{
    private static readonly string[] Under20 =
    [
        "zéro", "un", "deux", "trois", "quatre", "cinq", "six", "sept", "huit", "neuf", "dix",
        "onze", "douze", "treize", "quatorze", "quinze", "seize"
    ];

    private static readonly string[] Tens = ["", "", "vingt", "trente", "quarante", "cinquante", "soixante"];

    public static string Format(decimal amount, string devisePlural = "dirhams")
    {
        var v = Math.Round(amount, 2, MidpointRounding.AwayFromZero);
        if (v <= 0)
            return $"zéro {devisePlural}";

        var whole = (long)Math.Floor(v);
        var cents = (int)Math.Round((v - whole) * 100, MidpointRounding.AwayFromZero);
        if (cents >= 100)
        {
            whole += 1;
            cents -= 100;
        }

        var w = WholeToFrench(whole);
        var curr = whole == 1 ? devisePlural.TrimEnd('s') : devisePlural;
        if (string.IsNullOrEmpty(curr))
            curr = devisePlural;

        if (cents == 0)
            return $"{w} {curr}";

        var c = WholeToFrench(cents);
        var centWord = cents == 1 ? "centime" : "centimes";
        return $"{w} {curr} et {c} {centWord}";
    }

    /// <summary>Arabic fallback for legal line when UI is Arabic (full speller out of scope).</summary>
    public static string FormatArabicFallback(decimal amount, string devise)
    {
        var v = Math.Round(amount, 2, MidpointRounding.AwayFromZero);
        return $"{v.ToString("N2", CultureInfo.InvariantCulture)} {devise}";
    }

    private static string WholeToFrench(long n)
    {
        if (n < 0) return "moins " + WholeToFrench(-n);
        if (n < 17) return Under20[(int)n];
        if (n < 20) return "dix-" + Under20[(int)(n - 10)];
        if (n < 70) return TensUnder70((int)n);
        if (n < 80)
        {
            var rest = n - 60;
            if (rest == 1) return "soixante-et-un";
            if (rest == 11) return "soixante-et-onze";
            return "soixante-" + WholeToFrench(rest);
        }
        if (n < 100)
        {
            if (n == 80) return "quatre-vingts";
            return "quatre-vingt-" + WholeToFrench(n - 80);
        }

        if (n < 200)
        {
            if (n == 100) return "cent";
            return "cent " + WholeToFrench(n - 100);
        }

        if (n < 1000)
        {
            var h = n / 100;
            var r = n % 100;
            string head;
            if (h == 1)
                head = "cent";
            else if (r == 0)
                head = Under20[(int)h] + " cents";
            else
                head = Under20[(int)h] + " cent";
            return r == 0 ? head : head + " " + WholeToFrench(r);
        }

        if (n < 1_000_000)
        {
            var t = n / 1000;
            var r = n % 1000;
            var head = t == 1 ? "mille" : WholeToFrench(t) + " mille";
            return r == 0 ? head : head + " " + WholeToFrench(r);
        }

        if (n < 1_000_000_000)
        {
            var m = n / 1_000_000;
            var r = n % 1_000_000;
            var head = m == 1 ? "un million" : WholeToFrench(m) + " millions";
            return r == 0 ? head : head + " " + WholeToFrench(r);
        }

        return n.ToString(CultureInfo.InvariantCulture);
    }

    private static string TensUnder70(int n)
    {
        var ten = n / 10;
        var u = n % 10;
        var t = Tens[ten];
        if (u == 0) return t;
        if (u == 1) return t + "-et-un";
        return t + "-" + Under20[u];
    }
}
