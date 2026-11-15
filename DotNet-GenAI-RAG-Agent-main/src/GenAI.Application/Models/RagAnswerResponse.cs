using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenAI.Application.Models
{
	public class RagAnswerResponse
	{
		public string Question { get; set; } = string.Empty;
		public string Answer { get; set; } = string.Empty;
		public List<RagSource> Sources { get; set; } = new();
	}

	public class RagSource
	{
		public Guid DocumentId { get; set; }
		public string FileName { get; set; } = string.Empty;
		public int ChunkIndex { get; set; }
		public string Content { get; set; } = string.Empty;
	}
}
