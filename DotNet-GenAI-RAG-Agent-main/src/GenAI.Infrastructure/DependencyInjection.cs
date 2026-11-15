using GenAI.Application.Interfaces;
using GenAI.Application.Services;
using GenAI.Infrastructure.Documents;
using GenAI.Infrastructure.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;


namespace GenAI.Infrastructure
{
	public static class DependencyInjection
	{
		public static IServiceCollection AddInfrastructure(
		this IServiceCollection services,
		IConfiguration configuration)
		{
			services.AddScoped<
			IBlobStorageService,
			AzureBlobStorageService>();

			services.AddScoped<
				IDocumentRepository,
				AzureSqlDocumentRepository>();

			services.AddScoped<
				IDocumentTextExtractor,
				DocumentTextExtractor>();

			services.AddScoped<
				IDocumentIngestionService,
				DocumentIngestionService>();

			//services.AddSingleton<
			//	IDocumentChunkStore,
			//	InMemoryDocumentChunkStore>();

			services.AddScoped<IDocumentChunkStore,
			AzureSqlDocumentChunkStore>();

			
			services.AddScoped<IVectorSearchService,
			AzureSqlVectorSearchService>();

			services.AddScoped<IRagRetrievalService, RagRetrievalService>();

			services.AddScoped<IRagAnswerService, RagAnswerService>();


			// Keep your existing registrations for:
			// ITextChunker
			// IEmbeddingService
			// ISimilarityService

			return services;
		}
	}
}
