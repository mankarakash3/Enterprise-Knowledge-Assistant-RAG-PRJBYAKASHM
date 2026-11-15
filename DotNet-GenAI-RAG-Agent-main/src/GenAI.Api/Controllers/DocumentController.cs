using GenAI.Api.Models;
using GenAI.Application.Interfaces;
using GenAI.Application.Models;
using GenAI.Infrastructure.Documents;
using Microsoft.AspNetCore.Mvc;

namespace GenAI.Api.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class DocumentController : ControllerBase
	{
		private readonly IDocumentIngestionService _documentIngestionService;

		public DocumentController(IDocumentIngestionService ingestionService)
		{
			_documentIngestionService = ingestionService;
		}

		[HttpPost("ingest")]
		[Consumes("multipart/form-data")]
		public async Task<IActionResult> Ingest(
			[FromForm] DocumentUploadRequest request,
			CancellationToken cancellationToken)
		{
			if (request.File == null || request.File.Length == 0)
				return BadRequest("Please upload a valid document.");

			var allowedExtensions = new[] { ".pdf", ".docx", ".txt" };

			var extension = Path.GetExtension(request.File.FileName);

			if (!allowedExtensions.Contains(extension,StringComparer.OrdinalIgnoreCase))
			{
				return BadRequest(
					"Only PDF, DOCX and TXT files are supported.");
			}

			if (string.IsNullOrWhiteSpace(request.Source))
				return BadRequest("Source is required.");


			await using var stream = request.File.OpenReadStream();

			var command = new IngestDocumentCommand
			{
				FileStream = stream,
				FileName = request.File.FileName,
				ContentType = request.File.ContentType,
				FileSize = request.File.Length,
				Source = request.File.FileName
			};

			var documentId = await _documentIngestionService
				.IngestAsync(command, cancellationToken);

			return Ok(new
			{
				DocumentId = documentId,
				Message = "Document ingested successfully."
			});
		}
	}

	public record IngestDocumentRequest(string Text, string Source);
}
