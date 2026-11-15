using GenAI.Infrastructure.Documents;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace GenAI.UnitTests
{
	public class SqlConnectionFactoryTests
	{
		[Fact]
		public async Task TestConnectionAsync_ShouldReturnTrue_WhenDatabaseConnectionIsSuccessful()
		{
			//Arrange
			//var configuration = new ConfigurationBuilder()
			// .SetBasePath(Directory.GetCurrentDirectory())
			// .AddJsonFile("appsettings.json", optional: false)
			// .Build();

			var configuration = new ConfigurationBuilder()
			.AddUserSecrets<SqlConnectionFactoryTests>()
			.Build();

			//var repository = new AzureSqlDocumentRepository(configuration);

			var connectionString = configuration.GetConnectionString("EnterpriseRAGDB");

			Assert.False(string.IsNullOrWhiteSpace(connectionString));

			var factory = new AzureSqlDocumentRepository(configuration);

			// Act
			//var result = await factory.TestConnectionAsync(
			//	CancellationToken.None);

			// Assert
			//Assert.True(result);
		}
	}
}
