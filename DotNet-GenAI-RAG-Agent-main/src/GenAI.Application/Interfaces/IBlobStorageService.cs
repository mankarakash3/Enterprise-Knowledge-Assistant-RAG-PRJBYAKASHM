using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenAI.Application.Interfaces
{
	public interface IBlobStorageService
	{
		Task<string> UploadAsync(
		Stream fileStream,
		string fileName,
		CancellationToken cancellationToken);
	}
}
