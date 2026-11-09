using System.Text;

namespace ProyectoFinal_MonteroIsaac.Services
{
    /// <summary>Generador mínimo de PDF (una página, texto Helvetica) para el comprobante de matrícula.</summary>
    public static class PdfSimple
    {
        private static string Esc(string s) => s.Replace("\\", "\\\\").Replace("(", "\\(").Replace(")", "\\)");

        public static byte[] Crear(string titulo, IEnumerable<string> lineas)
        {
            var latin1 = Encoding.Latin1;
            var sb = new StringBuilder();
            sb.Append("BT /F1 18 Tf 50 790 Td (").Append(Esc(titulo)).Append(") Tj ET\n");
            sb.Append("BT /F1 11 Tf 50 760 Td 16 TL\n");
            foreach (var l in lineas.Take(40)) sb.Append('(').Append(Esc(l)).Append(") Tj T*\n");
            sb.Append("ET");
            var content = sb.ToString();

            var objs = new List<string>
            {
                "<< /Type /Catalog /Pages 2 0 R >>",
                "<< /Type /Pages /Kids [3 0 R] /Count 1 >>",
                "<< /Type /Page /Parent 2 0 R /MediaBox [0 0 595 842] /Contents 4 0 R /Resources << /Font << /F1 5 0 R >> >> >>",
                $"<< /Length {latin1.GetByteCount(content)} >>\nstream\n{content}\nendstream",
                "<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica /Encoding /WinAnsiEncoding >>"
            };

            using var ms = new MemoryStream();
            void W(string s) { var b = latin1.GetBytes(s); ms.Write(b, 0, b.Length); }
            W("%PDF-1.4\n");
            var offsets = new List<long>();
            for (int i = 0; i < objs.Count; i++)
            {
                offsets.Add(ms.Position);
                W($"{i + 1} 0 obj\n{objs[i]}\nendobj\n");
            }
            var xref = ms.Position;
            W($"xref\n0 {objs.Count + 1}\n0000000000 65535 f \n");
            foreach (var o in offsets) W($"{o:D10} 00000 n \n");
            W($"trailer\n<< /Size {objs.Count + 1} /Root 1 0 R >>\nstartxref\n{xref}\n%%EOF");
            return ms.ToArray();
        }
    }
}
