using Azure.Core;
using Azure.Identity;
using GenAI.Domain.Entities;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;
using System.Text.Json;
using GenAI.Application.Interfaces;

namespace GenAI.Infrastructure.Storage
{
	public class AzureSqlVectorSearchService : IVectorSearchService
	{
		private readonly string _connectionString;
		private readonly TokenCredential _credential;

		public AzureSqlVectorSearchService(IConfiguration configuration)
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

		public async Task<IReadOnlyList<DocumentChunk>> SearchAsync(
			float[] queryEmbedding,
			int topK,
			CancellationToken cancellationToken)
		{
			if (queryEmbedding == null || queryEmbedding.Length != 1536)
			{
				throw new ArgumentException(
					"Query embedding must contain exactly 1536 dimensions.");
			}

			if (topK <= 0)
			{
				throw new ArgumentOutOfRangeException(
					nameof(topK), "TopK must be greater than zero.");
			}

			const string sql = """
            SELECT TOP (@TopK)
                c.ChunkId,
                c.DocumentId,
                c.ChunkIndex,
                c.ChunkText,
                CAST(c.Embedding AS NVARCHAR(MAX)) AS EmbeddingJson,
                d.FileName AS Source,
                VECTOR_DISTANCE(
                    'cosine',
                    c.Embedding,
                    CAST(@QueryEmbedding AS VECTOR(1536))
                ) AS Distance
            FROM dbo.DocumentChunks c
            INNER JOIN dbo.Documents d
                ON c.DocumentId = d.DocumentId
            ORDER BY Distance ASC;
            """;

			var embeddingJson = JsonSerializer.Serialize(queryEmbedding);

			var results = new List<DocumentChunk>();

			await using var connection =
				await GetConnectionAsync(cancellationToken);

			await using var command = new SqlCommand(sql, connection);

			command.Parameters.Add("@TopK", SqlDbType.Int).Value = topK;

			command.Parameters.Add(
				"@QueryEmbedding",
				SqlDbType.NVarChar,
				-1).Value = embeddingJson;

			await using var reader =
				await command.ExecuteReaderAsync(cancellationToken);

			while (await reader.ReadAsync(cancellationToken))
			{
				var storedEmbeddingJson = reader.GetString(4);

				var embedding =
					JsonSerializer.Deserialize<float[]>(storedEmbeddingJson)
					?? Array.Empty<float>();

				results.Add(new DocumentChunk
				{
					Id = reader.GetGuid(0),
					DocumentId = reader.GetGuid(1),
					ChunkIndex = reader.GetInt32(2),
					Text = reader.GetString(3),
					Embedding = embedding,
					Source = reader.GetString(5)
				});
			}

			return results;
		}
	}
}
