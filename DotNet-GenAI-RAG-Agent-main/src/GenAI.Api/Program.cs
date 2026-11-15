using GenAI.Infrastructure;
using GenAI.Application.Interfaces;
using GenAI.Infrastructure.AI;
using GenAI.Infrastructure.Context;
using GenAI.Infrastructure.Prompting;
using GenAI.Infrastructure.Text;
using Microsoft.Extensions.DependencyInjection;
using GenAI.Infrastructure.Similarity;
using GenAI.Infrastructure.Storage;
using GenAI.Infrastructure.Documents;
using GenAI.Infrastructure.Search;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<IChatService>(sp =>
{   
    var configuration = sp.GetRequiredService<IConfiguration>();

    var apiKey = configuration["OpenAI:ApiKey"]
        ?? throw new InvalidOperationException("OpenAI API key is not configured.");

    var promptBuilder = sp.GetRequiredService<IPromptBuilder>();

    return new OpenAIChatService(apiKey, promptBuilder);
});
builder.Services.AddScoped<IPromptBuilder>(sp =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();

    var promptOptions = configuration
        .GetSection("OpenAI:SystemPrompt")
        .Get<PromptOptions>()
        ?? throw new InvalidOperationException(
            "OpenAI prompt configuration is not configured.");

    return new PromptBuilder(promptOptions);
});

builder.Services.AddScoped<IContextProvider, SimpleContextProvider>();
builder.Services.AddScoped<ITextChunker, SimpleTextChunker>();
builder.Services.AddScoped<IEmbeddingService>(sp =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();
    var apiKey = configuration["OpenAI:ApiKey"]
        ?? throw new InvalidOperationException("OpenAI API key is not configured.");
    return new OpenAIEmbeddingService(apiKey);
});
builder.Services.AddScoped<ISimilarityService, CosineSimilarityService>();
builder.Services.AddSingleton<IDocumentChunkStore, InMemoryDocumentChunkStore>();

//builder.Services.AddScoped<IDocumentIngestionService, DocumentIngestionService>();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddScoped<ISemanticSearchService, SemanticSearchService>();
builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
//builder.Services.AddOpenApi();

builder.Services.AddCors(options =>
{
	options.AddPolicy("ReactApp", policy =>
	{
		policy.WithOrigins("http://localhost:5173")
			  .AllowAnyHeader()
			  .AllowAnyMethod();
	});
});

var app = builder.Build();

app.UseCors("ReactApp");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    //app.MapOpenApi();
	 app.UseSwagger();
    app.UseSwaggerUI();
}


app.UseHttpsRedirection();
app.UseAuthorization();

app.MapControllers();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast =  Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
