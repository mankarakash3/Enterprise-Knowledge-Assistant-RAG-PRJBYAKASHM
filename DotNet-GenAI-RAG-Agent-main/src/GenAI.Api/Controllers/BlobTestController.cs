using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using GenAI.Application.Interfaces;

namespace GenAI.Api.Controllers
{
	[Route("api/blob-test")]
	[ApiController]
	public class BlobTestController : ControllerBase
	{
		private readonly IBlobStorageService _blobStorageService;

		public BlobTestController(
		IBlobStorageService blobStorageService)
		{
			_blobStorageService = blobStorageService;
		}

		[HttpPost("upload")]
		[Consumes("multipart/form-data")]
		public async Task<IActionResult> Upload(
		IFormFile file,
		CancellationToken cancellationToken)
		{
			if (file == null || file.Length == 0)
				return BadRequest("Please select a file.");

			await using var stream = file.OpenReadStream();

			var blobName = await _blobStorageService.UploadAsync(
				stream,
				file.FileName,
				cancellationToken);

			return Ok(new
			{
				Message = "File uploaded successfully",
				BlobName = blobName,
				OriginalFileName = file.FileName,
				FileSize = file.Length
			});
		}
	}
}
