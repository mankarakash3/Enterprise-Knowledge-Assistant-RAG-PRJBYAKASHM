using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GenAI.Domain.Documents;

namespace GenAI.Application.Interfaces
{
	public interface IDocumentRepository
	{
		Task AddAsync(
		Document document,
		CancellationToken cancellationToken);

		Task UpdateStatusAsync(
		Guid documentId,
		string status,
		CancellationToken cancellationToken);
	}
}
