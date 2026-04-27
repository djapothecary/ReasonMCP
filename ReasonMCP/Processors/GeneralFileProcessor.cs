using ReasonMCP.Interfaces;
using ReasonMCP.Records;

namespace ReasonMCP.Processors
{
    public class GeneralFileProcessor : IDocumentProcessor
    {
        public bool CanProcess(string filePath)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<ParsedChunk>> ProcessAsync(string filePath)
        {
            throw new NotImplementedException();
        }
    }
}