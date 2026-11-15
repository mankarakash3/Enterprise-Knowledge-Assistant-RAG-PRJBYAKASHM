using GenAI.Api.Models;
using GenAI.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GenAI.Api.Controllers
{
	[ApiController]
	[Route("api/rag")]
	public class RagController : ControllerBase
	{
		private readonly IRagAnswerService _ragAnswerService;

		public RagController(IRagAnswerService ragAnswerService)
		{
			_ragAnswerService = ragAnswerService;
		}

		[HttpPost("ask")]
		public async Task<IActionResult> Ask(
			[FromBody] RagAskRequest request,
			CancellationToken cancellationToken)
		{
			if (string.IsNullOrWhiteSpace(request.Question))
			{
				return BadRequest("Question is required.");
			}

			if (request.TopK <= 0 || request.TopK > 20)
			{
				return BadRequest("TopK must be between 1 and 20.");
			}

			var response = await _ragAnswerService.AskAsync(
				request.Question,
				request.TopK,
				cancellationToken);

			return Ok(response);
		}
	}
}
