namespace GrznarAi.Web.Core.Utils
{
    /// <summary>
    /// Helper class for generating URL-friendly slugs
    /// </summary>
    public static class SlugGenerator
    {
        /// <summary>
        /// Generates a URL-friendly slug from the provided title
        /// </summary>
        /// <param name="title">The title to convert to a slug</param>
        /// <returns>URL-friendly slug</returns>
        public static string GenerateSlug(string? title)
        {
            if (string.IsNullOrEmpty(title))
                return string.Empty;

            string slug = title.ToLower()
                .Replace(" ", "-")
                .Replace("č", "c")
                .Replace("ř", "r")
                .Replace("ž", "z")
                .Replace("š", "s")
                .Replace("ý", "y")
                .Replace("á", "a")
                .Replace("í", "i")
                .Replace("é", "e")
                .Replace("ú", "u")
                .Replace("ů", "u")
                .Replace("ň", "n")
                .Replace("ť", "t")
                .Replace("ď", "d");

            return slug;
        }
    }
} 