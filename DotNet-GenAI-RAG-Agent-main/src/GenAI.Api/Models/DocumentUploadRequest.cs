using Microsoft.AspNetCore.Http;
namespace GenAI.Api.Models
{
	public class DocumentUploadRequest
	{
		public IFormFile File { get; set; } = default!;
		public string Source { get; set; } = string.Empty;
	}
}
