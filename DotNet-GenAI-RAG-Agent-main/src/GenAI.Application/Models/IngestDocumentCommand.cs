using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenAI.Application.Models
{
	public class IngestDocumentCommand
	{
		public Stream FileStream { get; set; } = default!;
		public string FileName { get; set; } = string.Empty;
		public string ContentType { get; set; } = string.Empty;
		public long FileSize { get; set; }
		public string Source { get; set; } = string.Empty;
	}
}
