using GenAI.Application.Interfaces;
using GenAI.Application.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenAI.Application.Services
{
	public class RagAnswerService : IRagAnswerService
	{
		private readonly IRagRetrievalService _retrievalService;
		private readonly IChatService _chatService;

		public RagAnswerService(
	  IRagRetrievalService retrievalService,
	  IChatService chatService)
		{
			_retrievalService = retrievalService;
			_chatService = chatService;
		}


		public async Task<RagAnswerResponse> AskAsync(
	   string question,
	   int topK,
	   CancellationToken cancellationToken)
		{
			if (string.IsNullOrWhiteSpace(question))
			{
				throw new ArgumentException(
					"Question cannot be empty.",
					nameof(question));
			}

			// Step 1: Retrieve relevant document chunks.
			var chunks = await _retrievalService.RetrieveAsync(
				question,
				topK,
				cancellationToken);

			if (chunks.Count == 0)
			{
				return new RagAnswerResponse
				{
					Question = question,
					Answer = "I could not find relevant information in the uploaded documents."
				};
			}

			cancellationToken.ThrowIfCancellationRequested();

			// Step 2: Build context from retrieved chunks.
			var context = string.Join(
				"\n\n",
				chunks.Select((chunk, index) =>
					$"[Source {index + 1}: {chunk.Source}, Chunk {chunk.ChunkIndex}]\n{chunk.Text}"));

			// Step 3: Generate answer using OpenAI.
			var answer = await _chatService.GetResponseAsync(
				question,
				context);

			// Step 4: Prepare source references.
			var sources = chunks.Select(chunk => new RagSource
			{
				DocumentId = chunk.DocumentId,
				FileName = chunk.Source,
				ChunkIndex = chunk.ChunkIndex,
				Content = chunk.Text
			}).ToList();

			return new RagAnswerResponse
			{
				Question = question,
				Answer = answer,
				Sources = sources
			};
		}
	}
}
