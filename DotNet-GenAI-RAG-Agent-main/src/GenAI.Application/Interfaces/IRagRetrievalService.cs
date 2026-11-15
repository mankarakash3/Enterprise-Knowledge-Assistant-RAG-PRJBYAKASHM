using GenAI.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenAI.Application.Interfaces
{
	public interface IRagRetrievalService
	{
		Task<IReadOnlyList<DocumentChunk>> RetrieveAsync(
		string question,
		int topK,
		CancellationToken cancellationToken);
	}
}
