using Azure.Identity;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using GenAI.Application.Interfaces;
using Microsoft.Extensions.Configuration;

namespace GenAI.Infrastructure.Storage
{
	public class AzureBlobStorageService : IBlobStorageService
	{
		private readonly BlobContainerClient _containerClient;

		public AzureBlobStorageService(IConfiguration configuration)
		{
			var connectionString =
				configuration["Storage:ConnectionString"]
				?? throw new InvalidOperationException(
					"Storage connection string is not configured.");

			var containerName =
				configuration["Storage:ContainerName"]
				?? throw new InvalidOperationException(
					"Storage container name is not configured.");

			var blobServiceClient =
				new BlobServiceClient(connectionString);

			_containerClient =
				blobServiceClient.GetBlobContainerClient(containerName);
		}

		public async Task<string> UploadAsync(
			Stream fileStream,
			string fileName,
			CancellationToken cancellationToken)
		{
			var extension = Path.GetExtension(fileName);
			
			var blobName = $"{Guid.NewGuid()}{extension}";

			var blobClient = _containerClient.GetBlobClient(blobName);

			await blobClient.UploadAsync(
				fileStream,
				overwrite: false,
				cancellationToken);

			return blobName;
		}

		//private static string GetContentType(string extension)
		//{
		//	return extension.ToLowerInvariant() switch
		//	{
		//		".pdf" => "application/pdf",
		//		".docx" =>
		//			"application/vnd.openxmlformats-officedocument.wordprocessingml.document",
		//		".txt" => "text/plain",
		//		_ => "application/octet-stream"
		//	};
		//}

	}
}
