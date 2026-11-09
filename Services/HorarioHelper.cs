using System.Text.RegularExpressions;

namespace ProyectoFinal_MonteroIsaac.Services
{
    /// <summary>Interpreta horarios con formato "Lun y Mié 14:00-16:00" y detecta choques.</summary>
    public static class HorarioHelper
    {
        public const string Patron =
            @"^(Lun|Mar|Mié|Jue|Vie|Sáb|Dom)( y (Lun|Mar|Mié|Jue|Vie|Sáb|Dom))* \d{2}:\d{2}-\d{2}:\d{2}$";

        private static readonly Regex Rx =
            new(@"^(?<dias>.+?)\s+(?<i>\d{1,2}:\d{2})-(?<f>\d{1,2}:\d{2})$", RegexOptions.Compiled);

        public static bool TryParse(string horario, out HashSet<string> dias, out TimeSpan inicio, out TimeSpan fin)
        {
            dias = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            inicio = fin = TimeSpan.Zero;
            if (string.IsNullOrWhiteSpace(horario)) return false;
            var m = Rx.Match(horario.Trim());
            if (!m.Success) return false;
            foreach (var d in m.Groups["dias"].Value.Split(new[] { " y ", ",", " " }, StringSplitOptions.RemoveEmptyEntries))
                dias.Add(d.Trim());
            return TimeSpan.TryParse(m.Groups["i"].Value, out inicio) && TimeSpan.TryParse(m.Groups["f"].Value, out fin);
        }

        public static bool Choca(string a, string b)
        {
            if (!TryParse(a, out var da, out var ia, out var fa)) return false;
            if (!TryParse(b, out var db, out var ib, out var fb)) return false;
            return da.Overlaps(db) && ia < fb && ib < fa;
        }

        private static bool Overlaps(this HashSet<string> x, HashSet<string> y) => x.Any(y.Contains);
    }
}
