using System;
using System.Collections.Generic;
using System.Net;
using System.Text.RegularExpressions;

// Deliberately small HTML dialect: no browser, CSS, scripts or arbitrary TMP tags.
public static class NpcDialogueMarkup
{
    public sealed class Option { public string Label = ""; public string Action = ""; }
    static readonly Regex Attributes = new Regex("([a-z][a-z0-9_-]*)\\s*=\\s*(?:\"([^\"]*)\"|'([^']*)')", RegexOptions.IgnoreCase);
    public static string Normalize(string value)
    {
        value = Regex.Replace(WebUtility.HtmlDecode(value).Trim(), @"\s+", " ");
        if (value.StartsWith("bypass -h ", StringComparison.OrdinalIgnoreCase)) value = value.Substring(10);
        else if (value.StartsWith("bypass ", StringComparison.OrdinalIgnoreCase)) value = value.Substring(7);
        return value.ToLowerInvariant();
    }
    static string Clean(string html) => Regex.Replace(html ?? "", @"<!--.*?-->|<(script|style)\b[^>]*>.*?</\1\s*>", "", RegexOptions.IgnoreCase | RegexOptions.Singleline);
    public static List<Option> Buttons(string html)
    {
        var result = new List<Option>();
        foreach (Match button in Regex.Matches(Clean(html), @"<button\b[^>]*>", RegexOptions.IgnoreCase))
        {
            string label = "", action = "";
            foreach (Match attr in Attributes.Matches(button.Value))
            {
                string value = attr.Groups[2].Success ? attr.Groups[2].Value : attr.Groups[3].Value;
                if (attr.Groups[1].Value.Equals("value", StringComparison.OrdinalIgnoreCase)) label = WebUtility.HtmlDecode(value);
                if (attr.Groups[1].Value.Equals("action", StringComparison.OrdinalIgnoreCase)) action = Normalize(value);
            }
            if (label.Length > 0) result.Add(new Option { Label = label, Action = action });
            if (result.Count == 128) break;
        }
        return result;
    }
    public static string Text(string html)
    {
        string value = Regex.Replace(Clean(html), @"<button\b[^>]*>(?:\s*</button\s*>)?", "", RegexOptions.IgnoreCase);
        value = Regex.Replace(value, @"\s+", " ");
        value = Regex.Replace(value, @"<[^>]*>", m =>
        {
            string tag = m.Value.ToLowerInvariant();
            if (Regex.IsMatch(tag, @"^<br\s*/?>$")) return "\n";
            if (Regex.IsMatch(tag, @"^</p\s*>$|^</div\s*>$")) return "\n\n";
            if (Regex.IsMatch(tag, @"^</?(b|i|u)>$")) return tag;
            if (Regex.IsMatch(tag, @"^<h[1-6][^>]*>$")) return "\n<b><color=#D8B879>";
            if (Regex.IsMatch(tag, @"^</h[1-6]>$")) return "</color></b>\n\n";
            if (tag == "</font>" || tag == "</color>") return "</color>";
            Match color = Regex.Match(tag, "^<(?:font\\s+color\\s*=|color=)[\"']?(#[0-9a-f]{6}|#[0-9a-f]{8}|red|green|blue|white|yellow|gold)[\"']?\\s*>$");
            return color.Success ? "<color=" + color.Groups[1].Value + ">" : "";
        });
        // Decode entities only after stripping source tags, then escape decoded angle brackets.
        value = Regex.Replace(value, @"&(?:#\d+|#x[0-9a-f]+|[a-z]+);", m =>
            WebUtility.HtmlDecode(m.Value).Replace("<", "\uFF1C").Replace(">", "\uFF1E"), RegexOptions.IgnoreCase);
        return Regex.Replace(Regex.Replace(value, @" *\n *", "\n"), @"\n{3,}", "\n\n").Trim();
    }
}
