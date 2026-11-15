using GenAI.Application.Interfaces;
using GenAI.Application.Models;
using GenAI.Domain.Documents;
using GenAI.Domain.Entities;
using Microsoft.Extensions.Configuration;

namespace GenAI.Infrastructure.Documents
{
	public class DocumentIngestionService : IDocumentIngestionService
	{
		private readonly IBlobStorageService _blobStorageService;
		private readonly IDocumentRepository _documentRepository;
		private readonly IDocumentTextExtractor _textExtractor;
		private readonly ITextChunker _textChunker;
		private readonly IEmbeddingService _embeddingService;
		private readonly IDocumentChunkStore _chunkStore;
		private readonly IConfiguration _configuration;

		public DocumentIngestionService(
			IBlobStorageService blobStorageService,
			IDocumentRepository documentRepository,
			IDocumentTextExtractor textExtractor,
			ITextChunker textChunker,
			IEmbeddingService embeddingService,
			IDocumentChunkStore chunkStore,
			IConfiguration configuration)
		{
			_blobStorageService = blobStorageService;
			_documentRepository = documentRepository;
			_textExtractor = textExtractor;
			_textChunker = textChunker;
			_embeddingService = embeddingService;
			_chunkStore = chunkStore;
			_configuration = configuration;
		}

		public async Task<Guid> IngestAsync(
			IngestDocumentCommand request,
			CancellationToken cancellationToken)
		{
			if (request.FileStream == null)
				throw new ArgumentException("File stream is required.");

			if (string.IsNullOrWhiteSpace(request.FileName))
				throw new ArgumentException("File name is required.");

			if (string.IsNullOrWhiteSpace(request.Source))
				throw new ArgumentException("Source is required.");

			var documentId = Guid.NewGuid();

			var containerName =
				_configuration["Storage:ContainerName"]
				?? throw new InvalidOperationException(
					"Storage container is not configured.");

			string? blobName = null;
			var metadataSaved = false;

			try
			{
				// STEP 1: Copy uploaded stream into memory.
				// We need to read it twice: once for Blob upload
				// and once for text extraction.

				await using var memoryStream = new MemoryStream();

				await request.FileStream.CopyToAsync(
					memoryStream,
					cancellationToken);

				memoryStream.Position = 0;

				// STEP 2: Upload original document to Azure Blob.

				blobName = await _blobStorageService.UploadAsync(
					memoryStream,
					request.FileName,
					cancellationToken);

				// STEP 3: Save document metadata in Azure SQL.

				var document = new Document
				{
					DocumentId = documentId,
					FileName = request.FileName,
					Source = request.Source,
					BlobContainer = containerName,
					BlobName = blobName,
					ContentType = request.ContentType,
					FileSize = request.FileSize,
					Status = "Uploaded",
					CreatedAtUtc = DateTime.UtcNow
				};

				await _documentRepository.AddAsync(
					document,
					cancellationToken);

				metadataSaved = true;

				// STEP 4: Mark document as Processing.

				await _documentRepository.UpdateStatusAsync(
					documentId,
					"Processing",
					cancellationToken);

				// STEP 5: Extract document text.

				memoryStream.Position = 0;

				var documentText = await _textExtractor.ExtractTextAsync(
					memoryStream,
					request.FileName,
					cancellationToken);

				if (string.IsNullOrWhiteSpace(documentText))
				{
					throw new InvalidOperationException(
						"No readable text found in the document.");
				}

				// STEP 6: Split text into chunks.

				var textChunks = _textChunker.ChunkText(documentText);

				if (textChunks.Count == 0)
				{
					throw new InvalidOperationException(
						"No text chunks were generated.");
				}

				var documentChunks = new List<DocumentChunk>();

				// STEP 7: Generate embeddings.

				for (int i = 0; i < textChunks.Count; i++)
				{
					cancellationToken.ThrowIfCancellationRequested();

					var text = textChunks[i];

					var embedding = await _embeddingService
						.GetEmbeddingAsync(text);

					if (embedding.Length != 1536)
					{
						throw new InvalidOperationException(
							$"Expected 1536 embedding dimensions, got {embedding.Length}.");
					}

					documentChunks.Add(new DocumentChunk
					{
						Id = Guid.NewGuid(),
						DocumentId = documentId,
						ChunkIndex = i,
						Text = text,
						Embedding = embedding,
						Source = request.Source
					});
				}

				// STEP 8: Save chunks in the current in-memory store.

				await _chunkStore.AddChunksAsync(
						documentChunks,
						cancellationToken);

				// STEP 9: Mark ingestion as completed.

				await _documentRepository.UpdateStatusAsync(
					documentId,
					"Completed",
					cancellationToken);

				return documentId;
			}
			catch
			{
				// If metadata was created, record the failure.

				if (metadataSaved)
				{
					try
					{
						await _documentRepository.UpdateStatusAsync(
							documentId,
							"Failed",
							CancellationToken.None);
					}
					catch
					{
						// In production, log this secondary failure.
					}
				}

				throw;
			}
		}
	}
}
