using System;
using System.Text.RegularExpressions;

namespace GrznarAi.Web.Services
{
    public class MarkdownService
    {
        public string ConvertToHtml(string markdown)
        {
            if (string.IsNullOrEmpty(markdown))
                return string.Empty;

            // Konvertuje nadpisy (# Heading)
            markdown = Regex.Replace(markdown, @"^# (.+)$", "<h1>$1</h1>", RegexOptions.Multiline);
            markdown = Regex.Replace(markdown, @"^## (.+)$", "<h2>$1</h2>", RegexOptions.Multiline);
            markdown = Regex.Replace(markdown, @"^### (.+)$", "<h3>$1</h3>", RegexOptions.Multiline);
            markdown = Regex.Replace(markdown, @"^#### (.+)$", "<h4>$1</h4>", RegexOptions.Multiline);
            markdown = Regex.Replace(markdown, @"^##### (.+)$", "<h5>$1</h5>", RegexOptions.Multiline);
            markdown = Regex.Replace(markdown, @"^###### (.+)$", "<h6>$1</h6>", RegexOptions.Multiline);

            // Konvertuje zvýraznění (bold, italic)
            markdown = Regex.Replace(markdown, @"\*\*(.+?)\*\*", "<strong>$1</strong>");
            markdown = Regex.Replace(markdown, @"\*(.+?)\*", "<em>$1</em>");
            markdown = Regex.Replace(markdown, @"__(.+?)__", "<strong>$1</strong>");
            markdown = Regex.Replace(markdown, @"_(.+?)_", "<em>$1</em>");

            // Konvertuje odkazy [text](url)
            markdown = Regex.Replace(markdown, @"\[(.+?)\]\((.+?)\)", "<a href=\"$2\">$1</a>");

            // Konvertuje obrázky ![alt](url)
            markdown = Regex.Replace(markdown, @"!\[(.+?)\]\((.+?)\)", "<img src=\"$2\" alt=\"$1\" class=\"img-fluid\">");

            // Konvertuje seznamy
            // Nečíslované
            markdown = Regex.Replace(markdown, @"^\* (.+)$", "<li>$1</li>", RegexOptions.Multiline);
            markdown = Regex.Replace(markdown, @"((?:<li>.+?</li>\n)+)", "<ul>$1</ul>", RegexOptions.Singleline);
            
            // Číslované
            markdown = Regex.Replace(markdown, @"^\d+\. (.+)$", "<li>$1</li>", RegexOptions.Multiline);
            markdown = Regex.Replace(markdown, @"((?:<li>.+?</li>\n)+)", "<ol>$1</ol>", RegexOptions.Singleline);

            // Konvertuje kód ``` code ```
            markdown = Regex.Replace(markdown, @"```(.+?)```", "<pre><code>$1</code></pre>", RegexOptions.Singleline);
            
            // Konvertuje inline kód `code`
            markdown = Regex.Replace(markdown, @"`(.+?)`", "<code>$1</code>");

            // Konvertuje horizontální čáry
            markdown = Regex.Replace(markdown, @"^---+$", "<hr>", RegexOptions.Multiline);
            markdown = Regex.Replace(markdown, @"^\*\*\*+$", "<hr>", RegexOptions.Multiline);

            // Konvertuje odstavce
            markdown = Regex.Replace(markdown, @"^(?!<h|<ul|<ol|<li|<pre)(.+)$", "<p>$1</p>", RegexOptions.Multiline);
            
            // Odstraní prázdné odstavce
            markdown = Regex.Replace(markdown, @"<p>\s*</p>", "");

            return markdown;
        }
    }
} 