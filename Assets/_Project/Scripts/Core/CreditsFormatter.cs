using System.Text;
using System.Text.RegularExpressions;

namespace ThreeMatches.Core
{
    /// <summary>Turns the docs/CREDITS.md markdown into TextMeshPro rich text for the credits screen.</summary>
    public static class CreditsFormatter
    {
        static readonly Regex Link = new Regex(@"\[([^\]]+)\]\(([^)]+)\)", RegexOptions.Compiled);

        public static string ToRichText(string markdown, int titleSize = 64, int headingSize = 44)
        {
            if (string.IsNullOrEmpty(markdown)) return "";

            var sb = new StringBuilder(markdown.Length + 128);
            string[] lines = markdown.Replace("\r", "").Split('\n');
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i].TrimEnd();
                if (line.Length == 0)
                {
                    sb.Append('\n');
                    continue;
                }

                line = Link.Replace(line, "$1 ($2)");

                if (line.StartsWith("# ")) sb.Append("<size=").Append(titleSize).Append("><b>").Append(line.Substring(2)).Append("</b></size>\n");
                else if (line.StartsWith("## ")) sb.Append("\n<size=").Append(headingSize).Append("><b>").Append(line.Substring(3)).Append("</b></size>\n");
                else if (line.StartsWith("|")) AppendTableRow(sb, line);
                else if (line.StartsWith("- ")) sb.Append("• ").Append(line.Substring(2)).Append('\n');
                else sb.Append(line).Append('\n');
            }
            return sb.ToString();
        }

        static void AppendTableRow(StringBuilder sb, string line)
        {
            if (Regex.IsMatch(line, @"^\|[\s\-|:]+\|?$")) return; // separator row

            string[] cells = line.Trim('|').Split('|');
            for (int c = 0; c < cells.Length; c++)
            {
                if (c > 0) sb.Append("  -  ");
                sb.Append(cells[c].Trim());
            }
            sb.Append('\n');
        }
    }
}
