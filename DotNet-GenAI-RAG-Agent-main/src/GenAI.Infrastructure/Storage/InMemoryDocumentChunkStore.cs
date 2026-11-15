using GenAI.Application.Interfaces;
using GenAI.Domain.Entities;
using System.Collections.Concurrent;
namespace GenAI.Infrastructure.Storage
{
    public class InMemoryDocumentChunkStore : IDocumentChunkStore
    {
		private readonly ConcurrentBag<DocumentChunk> _chunks = new();

		public Task AddChunksAsync(
	   IEnumerable<DocumentChunk> chunks,
	   CancellationToken cancellationToken)
		{
			foreach (var chunk in chunks)
			{
				cancellationToken.ThrowIfCancellationRequested();

				_chunks.Add(chunk);
			}

			return Task.CompletedTask;
		}
		public Task<IReadOnlyList<DocumentChunk>> GetAllChunksAsync()
		{
			IReadOnlyList<DocumentChunk> result = _chunks.ToList();
			return Task.FromResult(result);
		}
	}
}
