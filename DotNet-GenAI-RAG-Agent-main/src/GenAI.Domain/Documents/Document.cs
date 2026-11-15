using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenAI.Domain.Documents
{
	public class Document
	{
		public Guid DocumentId { get; set; }
		public string FileName { get; set; } = string.Empty;
		public string Source { get; set; } = string.Empty;
		public string BlobContainer { get; set; } = string.Empty;
		public string BlobName { get; set; } = string.Empty;
		public string? ContentType { get; set; }
		public long FileSize { get; set; }
		public string Status { get; set; } = "Uploaded";
		public DateTime CreatedAtUtc { get; set; }
		public DateTime? UpdatedAtUtc { get; set; }
	}
}
