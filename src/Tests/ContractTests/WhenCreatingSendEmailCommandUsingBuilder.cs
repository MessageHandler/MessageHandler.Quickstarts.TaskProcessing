using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace ContractTests
{
    public class WhenCreatingSendEmailCommandUsingBuilder
    {
        [Fact]
        public async Task ShouldAdhereToContract()
        {
            var command = new SendEmailCommandBuilder()
                                        .WellknownEmail("a6c5018e-5a3f-4722-9b85-0c2d4eeccb2e")
                                        .Build();

            string csOutput = JsonSerializer.Serialize(command);

            await File.WriteAllTextAsync(@"./.verification/a6c5018e-5a3f-4722-9b85-0c2d4eeccb2e/actual.sendemailcommand.command.cs.json", csOutput);

            // output provided by similar tests on the receiver side
            var receiverOutput = await File.ReadAllTextAsync(@"./.verification/a6c5018e-5a3f-4722-9b85-0c2d4eeccb2e/verified.sendemailcommand.command.cs.json");

            Assert.Equal(receiverOutput, csOutput);
        }
    }
}