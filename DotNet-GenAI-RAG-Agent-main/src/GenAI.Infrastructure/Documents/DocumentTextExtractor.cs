using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DocumentFormat.OpenXml.Packaging;
using GenAI.Application.Interfaces;
using UglyToad.PdfPig;

namespace GenAI.Infrastructure.Documents
{
	public class DocumentTextExtractor : IDocumentTextExtractor
	{
	public Task<string> ExtractTextAsync(
	   Stream fileStream,
	   string fileName,
	   CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();

			var extension = Path.GetExtension(fileName).ToLowerInvariant();

			string text;

			switch (extension)
			{
				case ".pdf":
					{
						using var pdf = PdfDocument.Open(fileStream);

						text = string.Join(
							Environment.NewLine,
							pdf.GetPages().Select(page => page.Text));

						break;
					}

				case ".docx":
					{
						using var document =
							WordprocessingDocument.Open(fileStream, false);

						text = document.MainDocumentPart?
							.Document.Body?.InnerText ?? string.Empty;

						break;
					}

				case ".txt":
					{
						using var reader = new StreamReader(
							fileStream,
							leaveOpen: true);

						text = reader.ReadToEnd();

						break;
					}

				default:
					throw new NotSupportedException(
						$"File format '{extension}' is not supported.");
			}

			return Task.FromResult(text);
		}
	}
}
