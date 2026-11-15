namespace GenAI.Api.Models
{
	public class RagAskRequest
	{
		public string Question { get; set; } = string.Empty;
		public int TopK { get; set; } = 5;
	}
}
