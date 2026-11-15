using Azure.Core;
using Azure.Identity;
using GenAI.Application.Interfaces;
using GenAI.Domain.Entities;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace GenAI.Infrastructure.Storage
{
	public class AzureSqlDocumentChunkStore : IDocumentChunkStore
	{
		private readonly string _connectionString;
		private readonly TokenCredential _credential;

		public AzureSqlDocumentChunkStore(IConfiguration configuration)
		{
			_connectionString =
				configuration.GetConnectionString("EnterpriseRAGDB")
				?? throw new InvalidOperationException(
					"EnterpriseRAGDB connection string is not configured.");

			_credential = new AzureCliCredential();
		}

		private async Task<SqlConnection> GetConnectionAsync(
			CancellationToken cancellationToken)
		{
			var token = await _credential.GetTokenAsync(
				new TokenRequestContext(
					new[] { "https://database.windows.net/.default" }),
				cancellationToken);

			var connection = new SqlConnection(_connectionString)
			{
				AccessToken = token.Token
			};

			try
			{
				await connection.OpenAsync(cancellationToken);
				return connection;
			}
			catch
			{
				await connection.DisposeAsync();
				throw;
			}
		}

		public async Task AddChunksAsync(IEnumerable<DocumentChunk> chunks, CancellationToken cancellationToken)
		{
			const string sql = """
            INSERT INTO dbo.DocumentChunks
            (
                ChunkId,
                DocumentId,
                ChunkIndex,
                ChunkText,
                Embedding
            )
            VALUES
            (
                @ChunkId,
                @DocumentId,
                @ChunkIndex,
                @ChunkText,
                CAST(@Embedding AS VECTOR(1536))
            );
            """;

			await using var connection =
				await GetConnectionAsync(cancellationToken);

			foreach (var chunk in chunks)
			{
				if (chunk.Embedding.Length != 1536)
				{
					throw new InvalidOperationException(
						$"Expected 1536 embedding dimensions, got {chunk.Embedding.Length}.");
				}

				var embeddingJson = System.Text.Json.JsonSerializer.Serialize(
					chunk.Embedding);

				await using var command = new SqlCommand(sql, connection);

				command.Parameters.AddWithValue("@ChunkId", chunk.Id);
				command.Parameters.AddWithValue("@DocumentId", chunk.DocumentId);
				command.Parameters.AddWithValue("@ChunkIndex", chunk.ChunkIndex);
				command.Parameters.AddWithValue("@ChunkText", chunk.Text);
				command.Parameters.AddWithValue("@Embedding", embeddingJson);

				await command.ExecuteNonQueryAsync(cancellationToken);
			}
		}

		public async Task<IReadOnlyList<DocumentChunk>> GetAllChunksAsync()
		{
			const string sql = """
            SELECT
                ChunkId,
                DocumentId,
                ChunkIndex,
                ChunkText,
                CAST(Embedding AS NVARCHAR(MAX)) AS EmbeddingJson
            FROM dbo.DocumentChunks
            ORDER BY DocumentId, ChunkIndex;
            """;

			var chunks = new List<DocumentChunk>();

			await using var connection =
				await GetConnectionAsync(CancellationToken.None);

			await using var command = new SqlCommand(sql, connection);

			await using var reader =
				await command.ExecuteReaderAsync();

			while (await reader.ReadAsync())
			{
				var embeddingJson = reader.GetString(4);

				var embedding =
					System.Text.Json.JsonSerializer.Deserialize<float[]>(
						embeddingJson) ?? Array.Empty<float>();

				chunks.Add(new DocumentChunk
				{
					Id = reader.GetGuid(0),
					DocumentId = reader.GetGuid(1),
					ChunkIndex = reader.GetInt32(2),
					Text = reader.GetString(3),
					Embedding = embedding
				});
			}

			return chunks;
		}
	}
}

