using GenAI.Domain.Entities;


namespace GenAI.Application.Interfaces
{
	public interface IVectorSearchService
	{
	   Task<IReadOnlyList<DocumentChunk>> SearchAsync(
	   float[] queryEmbedding,
	   int topK,
	   CancellationToken cancellationToken);
	}
}
