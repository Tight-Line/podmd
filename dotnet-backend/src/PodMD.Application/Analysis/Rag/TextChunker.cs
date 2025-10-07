using System.Text;
using System.Text.RegularExpressions;

namespace PodMD.Application.Analysis.Rag;

public class Chunk
{
    public string Text { get; set; } = string.Empty;
    public int StartIndex { get; set; }
    public int EndIndex { get; set; }
    public string SourceFile { get; set; } = string.Empty;
    public string ChunkId => $"{SourceFile}_{StartIndex}_{EndIndex}";
}

public class TextChunker
{
    private const int DEFAULT_CHUNK_SIZE = 512;
    private const int DEFAULT_OVERLAP = 50;

    private readonly int _chunkSize;
    private readonly int _overlap;
    private readonly bool _respectSentenceBoundaries;

    public TextChunker(int chunkSize = DEFAULT_CHUNK_SIZE, int overlap = DEFAULT_OVERLAP, bool respectSentenceBoundaries = true)
    {
        if (chunkSize <= 0)
        {
            throw new ArgumentException("Chunk size must be greater than 0", nameof(chunkSize));
        }

        if (overlap < 0 || overlap >= chunkSize)
        {
            throw new ArgumentException("Overlap must be >= 0 and < chunk size", nameof(overlap));
        }

        _chunkSize = chunkSize;
        _overlap = overlap;
        _respectSentenceBoundaries = respectSentenceBoundaries;
    }

    public List<Chunk> ChunkText(string text, string sourceFile = "")
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return new List<Chunk>();
        }

        var chunks = new List<Chunk>();
        var words = text.Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
        var currentPosition = 0;

        while (currentPosition < words.Length)
        {
            var chunkText = BuildChunkText(words, ref currentPosition);
            var wordCount = chunkText.Split(' ').Length;
            var chunkStart = Math.Max(0, currentPosition - wordCount);

            var chunk = new Chunk
            {
                Text = chunkText.Trim(),
                StartIndex = chunkStart,
                EndIndex = currentPosition,
                SourceFile = sourceFile
            };

            // Skip empty chunks
            if (!string.IsNullOrWhiteSpace(chunk.Text))
            {
                chunks.Add(chunk);
            }

            // Move position forward considering overlap
            // Ensure we don't go backward and prevent infinite loops
            if (currentPosition <= chunkStart + _overlap)
            {
                // If overlap would cause us to stay in same position, move forward minimally
                currentPosition = chunkStart + 1;
            }
            else
            {
                currentPosition = Math.Max(currentPosition, chunk.EndIndex + 1 - _overlap);
            }
        }

        return chunks;
    }

    private string BuildChunkText(string[] words, ref int position)
    {
        var chunkBuilder = new StringBuilder();
        var wordsAdded = 0;
        var targetChunkSize = Math.Min(_chunkSize, words.Length - position);

        // Try to build chunk respecting sentence boundaries if enabled
        if (_respectSentenceBoundaries)
        {
            var candidateChunkSize = targetChunkSize;

            // Look for sentence boundary within target window
            while (candidateChunkSize < words.Length - position)
            {
                var testChunk = BuildChunkFromWords(words, position, candidateChunkSize);
                var lastSentenceEnd = FindLastSentenceEnd(testChunk);

                if (lastSentenceEnd >= 0)
                {
                    // Found sentence boundary, adjust chunk size to end at sentence
                    targetChunkSize = candidateChunkSize;
                    break;
                }

                candidateChunkSize += Math.Min(50, words.Length - position - candidateChunkSize);

                if (candidateChunkSize >= targetChunkSize + 100) // Don't search too far
                {
                    break;
                }
            }
        }

        // Build final chunk
        for (var i = 0; i < targetChunkSize && position < words.Length; i++)
        {
            if (chunkBuilder.Length > 0)
            {
                chunkBuilder.Append(' ');
            }

            chunkBuilder.Append(words[position]);
            position++;
            wordsAdded++;
        }

        return chunkBuilder.ToString();
    }

    private static int FindLastSentenceEnd(string text)
    {
        // Look for sentence endings with some context
        var sentenceEndings = new[] { ". ", "! ", "? ", ".\n", "!\n", "?\n" };
        var lastFound = -1;

        foreach (var ending in sentenceEndings)
        {
            var index = text.LastIndexOf(ending, StringComparison.OrdinalIgnoreCase);
            if (index > lastFound)
            {
                lastFound = index + ending.Length - 1; // Position of the punctuation
            }
        }

        return lastFound;
    }

    private static string BuildChunkFromWords(string[] words, int startPosition, int count)
    {
        var builder = new StringBuilder();
        for (var i = 0; i < count && startPosition + i < words.Length; i++)
        {
            if (builder.Length > 0)
            {
                builder.Append(' ');
            }
            builder.Append(words[startPosition + i]);
        }
        return builder.ToString();
    }

    /// <summary>
    /// Estimates the number of tokens in a text string using a simple heuristic
    /// </summary>
    public static int EstimateTokenCount(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return 0;
        }

        // Simple heuristic: ~4 characters per token for English text
        // This is approximate but good enough for chunking decisions
        return Math.Max(1, text.Length / 4);
    }
}
