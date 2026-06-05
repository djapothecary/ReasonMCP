using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace ReasonMCP.Utilities
{
    public static class PdfMarkupSanitizer
    {
        //  Matches numbered lists like "1. ", "12. "
        private static readonly Regex _numberedListRegex = new Regex(@"^\d+\.\s", RegexOptions.Compiled);

        public static string SanitizePdfMarkdown(string rawMarkdown)
        {
            if (string.IsNullOrWhiteSpace(rawMarkdown))
                return rawMarkdown;

            var lines = rawMarkdown.Replace("\r\n", "\n").Split('\n');
            var result = new List<string>();
            var currentParagraph = new StringBuilder();
            bool inCodeBlock = false;

            foreach (var rawLine in lines)
            {
                string line = rawLine.TrimEnd();

                // RULE 1: Preserve Code Blocks exactly as they are
                if (line.StartsWith("```"))
                {
                    FlushParagraph(result, currentParagraph);
                    inCodeBlock = !inCodeBlock;
                    result.Add(line);
                    continue;
                }

                if (inCodeBlock)
                {
                    result.Add(rawLine); // Keep exact formatting inside code fences
                    continue;
                }

                // RULE 2: Kill the double-spacing artifact.
                // We ignore empty lines completely. We control the paragraph spacing later.
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                // RULE 3: Detect Structural Markdown
                bool isStructural = line.StartsWith("#") ||
                                    line.StartsWith("-") ||
                                    line.StartsWith("*") ||
                                    line.StartsWith(">") ||
                                    _numberedListRegex.IsMatch(line);

                if (isStructural)
                {
                    FlushParagraph(result, currentParagraph);
                    result.Add(line);
                }
                else
                {
                    // RULE 4: Stitch broken sentences back together (De-Wrapping)
                    if (currentParagraph.Length > 0)
                    {
                        currentParagraph.Append(" "); // Replace the PDF hard-return with a space
                    }

                    currentParagraph.Append(line.TrimStart());

                    // RULE 5: Semantic Paragraph Breaks
                    // If the line ends with strong punctuation, we assume the paragraph is done.
                    if (line.EndsWith(".") || line.EndsWith("!") ||
                        line.EndsWith("?") || line.EndsWith(":"))
                    {
                        FlushParagraph(result, currentParagraph);
                    }
                }
            }

            FlushParagraph(result, currentParagraph);

            // Re-join the sanitized blocks with proper, semantic double-spacing
            return string.Join("\n\n", result);
        }

        private static void FlushParagraph(List<string> result, StringBuilder currentParagraph)
        {
            if (currentParagraph.Length > 0)
            {
                result.Add(currentParagraph.ToString());
                currentParagraph.Clear();
            }
        }
    }
}