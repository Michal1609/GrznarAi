using System;
using Markdig;

namespace GrznarAi.Web.Services
{
    public class MarkdownService
    {
        private static readonly MarkdownPipeline Pipeline = new MarkdownPipelineBuilder()
            .UseAdvancedExtensions() // tables, footnotes, tasklists, atd.
            .UsePipeTables()
            .UseAutoIdentifiers()
            .UseEmphasisExtras()
            .UseBootstrap()
            .Build();

        public string ConvertToHtml(string markdown)
        {
            if (string.IsNullOrEmpty(markdown))
                return string.Empty;

            // Použijeme Markdig pro převod Markdownu na HTML
            return Markdig.Markdown.ToHtml(markdown, Pipeline);
        }
    }
} 