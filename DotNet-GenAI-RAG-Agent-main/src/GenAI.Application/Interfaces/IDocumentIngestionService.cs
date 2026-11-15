using GenAI.Application.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenAI.Application.Interfaces
{
    public interface IDocumentIngestionService
    {	
			Task<Guid> IngestAsync(
				IngestDocumentCommand request,
				CancellationToken cancellationToken);
	
	}
}
