using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ReasonMCP.Interfaces;
using ReasonMCP.Records;

namespace ReasonMCP.Processors
{
    public class MarkupChunkingProcessor : ICodeChunkingProcessor
    {
        private readonly ILogger<MarkupChunkingProcessor> _logger;

        //  Matches Razor/Blazor code blocks
        private static readonly Regex _razorCodeRegex = new Regex(@"(code|function)\s\s*\{", RegexOptions.Compiled);

        public MarkupChunkingProcessor(
            ILogger<MarkupChunkingProcessor> logger
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
            var extension = Path.GetExtension(filePath).ToLower();

            //  1.  If it's Blazor/Razor, extract the C# @code block first
            if (extension == ".razor" || extension == ".cshtml")
            {
                rawContent = ExtractAndChunkRazorCode(rawContent, filePath, chunks);
            }

            //  2.  Chunk the remaining HTML/CSS using the Whitespace Heuristic (Max 150 lines per chunk)
            ChunkByDeveloperWhitespace(rawContent, filePath, extension, chunks);

            return chunks;
        }

        private string ExtractAndChunkRazorCode(
            string content,
            string filePath,
            List<CodeChunk> chunks
        )
        {
            var match = _razorCodeRegex.Match(content);

            //  We use a simple brace counter to find the end of the @code block
            var startIndex = match.Index;
            var openBraces = 0;
            bool insideBlock = false;
            int endIndex = startIndex;

            for (int i = startIndex; i < content.Length; i++)
            {
                if (content[i] == '{')
                {
                    openBraces++;
                    insideBlock = true;
                }
                else if (content[i] == '}')
                {
                    openBraces--;
                }

                if (insideBlock && openBraces == 0)
                {
                    endIndex = i;
                    break;
                }
            }

            var codeBlock = content.Substring(startIndex, endIndex - startIndex + 1);

            //  Add the C# logic as it's own high-value chunk
            chunks.Add(new CodeChunk(
                Content: codeBlock,
                FilePath: filePath,
                NodeUri: "Razor.CodeBehind",
                NodeType: "CodeBlock",
                StartLine: 0,   //  Simplified for brevity
                EndLine: 0,
                Metadata: new Dictionary<string, string> { { "FileType", "C#" } }
            ));

            //  Return the markup WITHOUT the code block so we don't duplicate it
            return content.Remove(startIndex, endIndex - startIndex + 1);
        }

        private void ChunkByDeveloperWhitespace(
            string content,
            string filePath,
            string extension,
            List<CodeChunk> chunks
        )
        {
            if (string.IsNullOrWhiteSpace(content))
                return;

            //  Split by double newlines (Developer paragraphs)
            var paragraphs = content.Split(new[] { "\r\n\r\n", "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

            var currentChunk = new StringBuilder();
            int currentLineCount = 0;
            const int MaxLinesPerChunk = 150;   //  A safe limit for LLM context density

            foreach (var paragraph in paragraphs)
            {
                var paragraphLines = paragraph.Split('\n').Length;

                //  If adding this paragraph exceeds our limit, save the current chunk and start a new one
                if (currentLineCount + paragraphLines > MaxLinesPerChunk && currentChunk.Length > 0)
                {
                    chunks.Add(CreateChunk(currentChunk.ToString(), filePath, extension));
                    currentChunk.Clear();
                    currentLineCount = 0;
                }

                currentChunk.AppendLine(paragraph.Trim());
                currentChunk.AppendLine();  //  restore the visual spacing
                currentLineCount += paragraphLines;
            }

            //  Flush the final chunk
            if (currentChunk.Length > 0)
            {
                chunks.Add(CreateChunk(currentChunk.ToString(), filePath, extension));
            }
        }

        private CodeChunk CreateChunk(
            string content,
            string filePath,
            string extension
        )
        {
            return new CodeChunk(
                Content: content.Trim(),
                FilePath: filePath,
                NodeUri: Path.GetFileName(filePath),
                NodeType: "MarkupBlock",
                StartLine: 0,
                EndLine: 0,
                Metadata: new Dictionary<string, string> { { "FileType", extension } }
            );
        }
    }
}