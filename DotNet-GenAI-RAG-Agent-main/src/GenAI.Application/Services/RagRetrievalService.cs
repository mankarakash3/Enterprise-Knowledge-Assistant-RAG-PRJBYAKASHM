using GenAI.Application.Interfaces;
using GenAI.Domain.Entities;

namespace GenAI.Application.Services
{
	public class RagRetrievalService : IRagRetrievalService
	{
		private readonly IEmbeddingService _embeddingService;
		private readonly IVectorSearchService _vectorSearchService;

		public RagRetrievalService(
			IEmbeddingService embeddingService,
			IVectorSearchService vectorSearchService)
		{
			_embeddingService = embeddingService;
			_vectorSearchService = vectorSearchService;
		}

		public async Task<IReadOnlyList<DocumentChunk>> RetrieveAsync(
			string question,
			int topK,
			CancellationToken cancellationToken)
		{
			if (string.IsNullOrWhiteSpace(question))
			{
				throw new ArgumentException(
					"Question cannot be empty.",
					nameof(question));
			}

			if (topK <= 0)
			{
				throw new ArgumentOutOfRangeException(
					nameof(topK));
			}

			cancellationToken.ThrowIfCancellationRequested();

			// Step 1: Generate embedding for user question.
			var queryEmbedding =
				await _embeddingService.GetEmbeddingAsync(question);

			cancellationToken.ThrowIfCancellationRequested();

			// Step 2: Search relevant chunks in Azure SQL.
			var relevantChunks =
				await _vectorSearchService.SearchAsync(
					queryEmbedding,
					topK,
					cancellationToken);

			return relevantChunks;
		}
	}
}
