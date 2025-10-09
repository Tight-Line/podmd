using PodMD.Application.Interfaces;
using PodMD.Domain.Entities;
using PodMD.Domain.Interfaces;
using PodMD.Application.Analysis.Rag;

namespace PodMD.Application.Analysis;

public class RagService : IRagService
{
    private readonly IKnowledgeBasesRepository _knowledgeBasesRepository;
    private readonly IKnowledgeFileRepository _knowledgeFileRepository;
    private readonly IFileStorage _fileStorage;
    private readonly ILogger<RagService> _logger;

    public RagService(
        IKnowledgeBasesRepository knowledgeBasesRepository,
        IKnowledgeFileRepository knowledgeFileRepository,
        IFileStorage fileStorage,
        ILogger<RagService> logger)
    {
        _knowledgeBasesRepository = knowledgeBasesRepository ?? throw new ArgumentNullException(nameof(knowledgeBasesRepository));
        _knowledgeFileRepository = knowledgeFileRepository ?? throw new ArgumentNullException(nameof(knowledgeFileRepository));
        _fileStorage = fileStorage ?? throw new ArgumentNullException(nameof(fileStorage));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<AnalysisRagContext> GetRelevantContextAsync(
        Guid clusterId,
        int maxContextTokens = 1024,
        CancellationToken cancellationToken = default)
    {
        var context = new AnalysisRagContext();
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        try
        {
            // Step 1: Query connected knowledge bases
            var knowledgeBases = await _knowledgeBasesRepository.GetKnowledgeBasesForSourceAsync(clusterId);
            if (!knowledgeBases.Any())
            {
                _logger.LogInformation("No knowledge bases connected to cluster {ClusterId}, RAG disabled", clusterId);
                return context; // RAG not enabled
            }

            context.SourcesUsed = knowledgeBases.Count();
            context.IsRagEnabled = true;

            _logger.LogInformation("Found {Count} knowledge bases connected to cluster {ClusterId}",
                knowledgeBases.Count(), clusterId);

            // Step 2: Extract and chunk all documents
            var allChunks = new List<Chunk>();
            foreach (var kb in knowledgeBases)
            {
                var fileChunks = await ExtractAndChunkKnowledgeBaseAsync(kb.Id, kb.Name, cancellationToken);
                allChunks.AddRange(fileChunks);
            }

            if (!allChunks.Any())
            {
                _logger.LogWarning("No chunks generated from knowledge bases for cluster {ClusterId}", clusterId);
                return context;
            }

            context.ChunksRetrieved = allChunks.Count;

            // Step 3: Assemble all available context within token limit
            var contextBuilder = new System.Text.StringBuilder();
            var totalTokens = 0;
            var chunksIncluded = 0;

            foreach (var chunk in allChunks)
            {
                var chunkTokens = TextChunker.EstimateTokenCount(chunk.Text);

                // Check if adding this chunk would exceed token limit
                if (totalTokens + chunkTokens > maxContextTokens)
                {
                    break; // Stop when we'd exceed token limit
                }

                // Add chunk to context
                contextBuilder.AppendLine($"[Source: {chunk.SourceFile}]");
                contextBuilder.AppendLine(chunk.Text);
                contextBuilder.AppendLine(); // Add spacing between chunks

                totalTokens += chunkTokens;
                chunksIncluded++;
            }

            if (chunksIncluded == 0)
            {
                _logger.LogWarning("No chunks could fit within token limit for cluster {ClusterId}", clusterId);
                return context;
            }

            if (chunksIncluded < allChunks.Count)
            {
                _logger.LogInformation("Included {Included} of {Total} chunks in context for cluster {ClusterId} (token limit)",
                    chunksIncluded, allChunks.Count, clusterId);
            }

            context.RetrievedKnowledge = contextBuilder.ToString().Trim();
            context.KnowledgeTokens = totalTokens;

            // Log performance metrics
            stopwatch.Stop();
            context.RagPerformanceMetrics = $"chunks_processed:{allChunks.Count},search_time_ms:{stopwatch.ElapsedMilliseconds}";

            _logger.LogInformation(
                "RAG context retrieved for cluster {ClusterId}: {Chunks} chunks, {Tokens} tokens, {Time}ms",
                clusterId, context.ChunksRetrieved, context.KnowledgeTokens, stopwatch.ElapsedMilliseconds);

            return context;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in RAG processing for cluster {ClusterId}", clusterId);
            context.RagPerformanceMetrics = $"error:unexpected_{ex.GetType().Name}";
            return context; // Return empty context, RAG disabled
        }
    }

    private async Task<List<Chunk>> ExtractAndChunkKnowledgeBaseAsync(
        Guid knowledgeBaseId,
        string knowledgeBaseName,
        CancellationToken cancellationToken)
    {
        try
        {
            // Get all files for this knowledge base
            var files = await _knowledgeFileRepository.GetByKnowledgeBaseIdAsync(knowledgeBaseId);
            if (!files.Any())
            {
                _logger.LogDebug("No files in knowledge base {KbName} ({KbId})", knowledgeBaseName, knowledgeBaseId);
                return new List<Chunk>();
            }

            var allChunks = new List<Chunk>();
            var chunker = new TextChunker(); // Uses default 512 token chunks with 50 token overlap
            var documentProcessor = new DocumentProcessor(Microsoft.Extensions.Logging.Abstractions.NullLogger<DocumentProcessor>.Instance);

            foreach (var file in files)
            {
                try
                {
                    // Download file content from MinIO
                    await using var fileStream = await _fileStorage.DownloadAsync(file.StorageKey);

                    // Extract text content
                    var textContent = await documentProcessor.ExtractTextAsync(
                        fileStream,
                        file.ContentType,
                        file.FileName);

                    if (string.IsNullOrWhiteSpace(textContent))
                    {
                        _logger.LogWarning("No text content extracted from file {FileName} in KB {KbName}",
                            file.FileName, knowledgeBaseName);
                        continue;
                    }

                    // Chunk the text
                    var chunks = chunker.ChunkText(textContent, $"{knowledgeBaseName}/{file.FileName}");
                    allChunks.AddRange(chunks);

                    _logger.LogDebug("Extracted {ChunkCount} chunks from file {FileName} in KB {KbName}",
                        chunks.Count, file.FileName, knowledgeBaseName);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to process file {FileName} in KB {KbName}, skipping",
                        file.FileName, knowledgeBaseName);
                    // Continue with other files
                }
            }

            _logger.LogDebug("Total chunks extracted from KB {KbName}: {TotalChunks}",
                knowledgeBaseName, allChunks.Count);

            return allChunks;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to extract chunks from knowledge base {KbName} ({KbId})",
                knowledgeBaseName, knowledgeBaseId);
            return new List<Chunk>();
        }
    }
}
