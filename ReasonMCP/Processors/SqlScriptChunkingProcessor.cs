using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ReasonMCP.Interfaces;
using ReasonMCP.Records;

namespace ReasonMCP.Processors
{
    public class SqlScriptChunkingProcessor : ICodeChunkingProcessor
    {
        private readonly ILogger<SqlScriptChunkingProcessor> _logger;
        private static readonly Regex _goBatchRegex = new Regex(@"(?im)^\s*GO\s\s*$", RegexOptions.Compiled);
        // Matches CREATE TABLE, PROCEDURE, VIEW, FUNCTION, TRIGGER to grab the object name
        private static readonly Regex _sqlObjectRegex = new Regex(@"(?i)CREATE\s+(OR\s+ALTER\s+)?(TABLE|PROCEDURE|VIEW|FUNCTION|TRIGGER)\s+(?:\[?[a-zA-Z0-9_]+\]?\.)?\[?(?<name>[a-zA-Z0-9_]+)\]?", RegexOptions.Compiled);


        public SqlScriptChunkingProcessor
        (
            ILogger<SqlScriptChunkingProcessor> logger
        )
        {
            _logger = logger;
        }

        public async Task<IEnumerable<CodeChunk>> ChunkFileAsync(
            string filePath,
            CancellationToken cancellationToken = default
        )
        {
            if (!File.Exists(filePath))
            {
                _logger.LogWarning("File not found: {FilePath}", filePath);
                return [];
            }

            var rawContent = await File.ReadAllTextAsync(filePath, cancellationToken);
            var chunks = new List<CodeChunk>();

            //  1.  Split the script into executable batches by 'GO'
            var batches = _goBatchRegex.Split(rawContent);

            int startLine = 1;

            foreach (var batch in batches)
            {
                var trimmedBatch = batch.TrimEnd();
                if (string.IsNullOrWhiteSpace(trimmedBatch))
                    continue;

                //  2.  Calculate line numbers for metadata
                var lineCount = trimmedBatch.Split('\n').Length;
                var endLine = startLine + lineCount - 1;

                //  3.  Extract the SQL Object Name (if it exists in this batch)
                string nodeUri = Path.GetFileName(filePath);
                string nodeType = "SqlBatch";

                var objectMatch = _sqlObjectRegex.Match(trimmedBatch);
                if (objectMatch.Success)
                {
                    nodeUri = objectMatch.Groups["name"].Value;
                    nodeType = objectMatch.Groups[2].Value; //  Capture the type (TABLE, PROCEDURE, etc)
                }

                chunks.Add(new CodeChunk(
                    Content: trimmedBatch,
                    FilePath: filePath,
                    NodeUri: nodeUri,
                    NodeType: nodeType,
                    StartLine: startLine,
                    EndLine: endLine,
                    Metadata: new Dictionary<string, string> { { "FileType", ".sql" } }
                ));

                //  Advance the line counter (+1 for the FO statement we removed)
                startLine = endLine + 2;
            }

            return chunks;
        }
    }
}