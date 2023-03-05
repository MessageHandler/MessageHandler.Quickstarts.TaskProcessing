using Microsoft.Extensions.Configuration;
using System.Threading;
using System.Threading.Tasks;
using Worker;
using Xunit;

namespace IntegrationTests
{
    public class UnitTest1
    {
        [Fact]
        public async Task GivenAvailableConfirmationMail_WhenGettingAvailableConfirmationMail_ThenCanMarkItAsSent()
        {
            var configuration = new ConfigurationBuilder()
            .AddUserSecrets<UnitTest1>()
            .Build();

            var connectionString = configuration.GetSection("sqlserverconnectionstring").Value;

            // given
            var inserter = new InsertAvailableConfirmationMail(connectionString);
            await inserter.StartAsync(CancellationToken.None);

            // when
            var persister = new PersistAvailableConfirmationMails(connectionString);
            var availableConfirmationMail = await persister.GetAvailableConfirmationMail();
            await persister.MarkAsSent(availableConfirmationMail);

            // then
            Assert.NotNull(availableConfirmationMail);            
        }
    }
}