using Azure.Core;
using Azure.Identity;
using GenAI.Application.Interfaces;
using GenAI.Domain.Documents;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace GenAI.Infrastructure.Documents
{
	public class AzureSqlDocumentRepository : IDocumentRepository
	{
		private readonly string _connectionString;
		private readonly TokenCredential _credential;

		public AzureSqlDocumentRepository(IConfiguration configuration)
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

			var connection = new SqlConnection(_connectionString);

			connection.AccessToken = token.Token;

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

		public async Task AddAsync(
			Document document,
			CancellationToken cancellationToken)
		{
			const string sql = """
            INSERT INTO dbo.Documents
            (
                DocumentId,
                FileName,
                Source,
                BlobContainer,
                BlobName,
                ContentType,
                FileSize,
                Status
            )
            VALUES
            (
                @DocumentId,
                @FileName,
                @Source,
                @BlobContainer,
                @BlobName,
                @ContentType,
                @FileSize,
                @Status
            );
            """;

			await using var connection =
				await GetConnectionAsync(cancellationToken);

			await using var command = new SqlCommand(sql, connection);

			command.Parameters.Add("@DocumentId", SqlDbType.UniqueIdentifier)
				.Value = document.DocumentId;

			command.Parameters.Add("@FileName", SqlDbType.NVarChar, 255)
				.Value = document.FileName;

			command.Parameters.Add("@Source", SqlDbType.NVarChar, 250)
				.Value = document.Source;

			command.Parameters.Add("@BlobContainer", SqlDbType.NVarChar, 100)
				.Value = document.BlobContainer;

			command.Parameters.Add("@BlobName", SqlDbType.NVarChar, 500)
				.Value = document.BlobName;

			command.Parameters.Add("@ContentType", SqlDbType.NVarChar, 150)
				.Value = (object?)document.ContentType ?? DBNull.Value;

			command.Parameters.Add("@FileSize", SqlDbType.BigInt)
				.Value = document.FileSize;

			command.Parameters.Add("@Status", SqlDbType.NVarChar, 30)
				.Value = document.Status;

			await command.ExecuteNonQueryAsync(cancellationToken);
		}

		public async Task UpdateStatusAsync(
			Guid documentId,
			string status,
			CancellationToken cancellationToken)
		{
			const string sql = """
            UPDATE dbo.Documents
            SET Status = @Status,
                UpdatedAtUtc = SYSUTCDATETIME()
            WHERE DocumentId = @DocumentId;
            """;

			await using var connection =
				await GetConnectionAsync(cancellationToken);

			await using var command = new SqlCommand(sql, connection);

			command.Parameters.Add("@Status", SqlDbType.NVarChar, 30)
				.Value = status;

			command.Parameters.Add("@DocumentId", SqlDbType.UniqueIdentifier)
				.Value = documentId;

			var rowsAffected =
				await command.ExecuteNonQueryAsync(cancellationToken);

			if (rowsAffected == 0)
			{
				throw new InvalidOperationException(
					$"Document {documentId} was not found.");
			}
		}
	}
}
