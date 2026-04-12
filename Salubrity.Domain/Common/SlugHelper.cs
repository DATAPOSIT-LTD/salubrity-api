using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Salubrity.Domain.Common;

public static class SlugHelper
{
    public static string Generate(string name, int year)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required for slug generation");

        var slug = name
            .Trim()
            .ToLowerInvariant()
            .Normalize(NormalizationForm.FormD)
            .Where(c => Char.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
            .Aggregate(new StringBuilder(), (sb, c) =>
            {
                if (char.IsLetterOrDigit(c))
                    sb.Append(c);
                else if (char.IsWhiteSpace(c) || c == '-' || c == '_')
                    sb.Append('-');

                return sb;
            })
            .ToString();

        slug = Regex.Replace(slug, "-{2,}", "-").Trim('-');

        return $"{slug}-{year}";
    }
}
