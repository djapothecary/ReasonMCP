using Polly.CircuitBreaker;
using ReasonMCP.Interfaces;

namespace ReasonMCP.Utilities
{
    public class RAGChunkingUtility : IRAGChunkingUtility
    {
        private readonly int _chunkSize;
        private readonly int _chunkOverlap;

        public RAGChunkingUtility(
            int chunkSize = 1000,
            int chunkOverlap = 200)
        {
            _chunkSize = chunkSize;
            _chunkOverlap = chunkOverlap;
        }

        public async IAsyncEnumerable<string> CreateChunks(string text)
        {
            if (string.IsNullOrEmpty(text)) yield break;

            ReadOnlyMemory<char> textMemory = text.AsMemory();
            int cursor = 0;
            int chunkCount = 0;
            char[] breakChars = { '.', '!', '?', ';', ':', '\n' };

            while (cursor < textMemory.Length)
            {
                // 1. Calculate the 'ideal' end point
                int end = Math.Min(cursor + _chunkSize, textMemory.Length);
                bool foundNaturalBreak = false;
                int previousCursor = cursor;

                if (end < textMemory.Length)
                {
                    var searchSpan = textMemory.Span.Slice(cursor, end - cursor);
                    int lastBreak = searchSpan.LastIndexOfAny(breakChars);

                    // 2. Only snap to a natural break if it's in the 'back half' of the chunk
                    // This prevents the "stuck" behavior if a period is found too early.
                    if (lastBreak != -1 && lastBreak > (_chunkSize / 2))
                    {
                        end = cursor + lastBreak + 1;
                        foundNaturalBreak = true;
                    }
                }

                string currentChunk = textMemory.Slice(cursor, end - cursor).ToString().Trim();

                // 3. Skip empty results (whitespace-only lines)
                if (!string.IsNullOrWhiteSpace(currentChunk))
                {
                    chunkCount++;
                    yield return currentChunk;
                }

                // 4. HARDENED PROGRESS LOGIC
                // Ensure the next cursor position is at least further than the current one
                int nextCursor = end - _chunkOverlap;

                // Safety Valve: If overlap pushes us back or keeps us still, force advance
                if (nextCursor <= cursor)
                {
                    cursor = end; // No overlap for this segment to break the loop
                }
                else
                {
                    cursor = nextCursor;
                }

                // Exit conditions
                if (cursor >= textMemory.Length || end >= textMemory.Length) break;
            }
        }
    }
}