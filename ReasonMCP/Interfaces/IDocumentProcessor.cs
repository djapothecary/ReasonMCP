using ReasonMCP.Records;

namespace ReasonMCP.Interfaces
{
    public interface IDocumentProcessor
    {
        //  The Bouncer: Does this specific processor know how to handle this file?
        bool CanProcess(string filePath);

        //  The Engine: Parse the text and attach the specific Metadata tags
        Task<IEnumerable<ParsedChunk>> ProcessAsync(string filePath);
    }
}