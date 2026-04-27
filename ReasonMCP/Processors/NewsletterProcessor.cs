using ReasonMCP.Interfaces;
using ReasonMCP.Records;

namespace ReasonMCP.Processors
{
    public class NewsletterProcessor : IDocumentProcessor
    {
        public bool CanProcess(string filePath)
        {
            //  Ensure file is a MarkDown and is ADR
            if (!filePath.EndsWith(".md", StringComparison.OrdinalIgnoreCase))
                return false;

            //  Using Path.DirectorySeparatorChar to safely handle Windows (\) and Linux (/)
            var adrFolder = $"{Path.DirectorySeparatorChar}Newsletters{Path.DirectorySeparatorChar}";
            return filePath.Contains(adrFolder, StringComparison.OrdinalIgnoreCase);
        }

        public Task<IEnumerable<ParsedChunk>> ProcessAsync(string filePath)
        {
            throw new NotImplementedException();
        }
    }
}