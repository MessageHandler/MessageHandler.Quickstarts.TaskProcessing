using System.Data.SqlClient;

namespace Worker
{
    public class InsertAvailableConfirmationMail : IHostedService
    {
        private readonly string insertSalesOrderConfirmationsSqlCommand = @"INSERT INTO [dbo].[SalesOrderConfirmations] ([OrderId], [BuyerId], [SenderEmailAddress], [EmailSubject], [EmailBody], [Status]) VALUES (@orderId, @buyerId, @senderEmailAddress, @emailSubject, @emailBody, @status);";
        private readonly string insertNotificationPreferencesSqlCommand = @"INSERT INTO [dbo].[NotificationPreferences] ([BuyerId], [EmailAddress]) VALUES (@buyerId, @emailAddress);";
        private string connectionstring;

        private ILogger<InsertAvailableConfirmationMail> logger;

        public InsertAvailableConfirmationMail(string connectionstring, ILogger<InsertAvailableConfirmationMail> logger)
        {
            this.connectionstring = connectionstring;
            this.logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            this.logger?.LogInformation("Inserting an available confirmation order");

            var connection = new SqlConnection(connectionstring);
            connection.Open();

            var buyerId = Guid.NewGuid().ToString();
            var orderId = Guid.NewGuid().ToString();

            using var notificationPreferencesCommand = new SqlCommand(insertNotificationPreferencesSqlCommand, connection);
            notificationPreferencesCommand.Parameters.AddWithValue("@buyerId", buyerId);
            notificationPreferencesCommand.Parameters.AddWithValue("@emailAddress", "buyer@buyer.com");
            await notificationPreferencesCommand.ExecuteNonQueryAsync();

            using var salesOrderConfirmationsCommand = new SqlCommand(insertSalesOrderConfirmationsSqlCommand, connection);
            salesOrderConfirmationsCommand.Parameters.AddWithValue("@orderId", orderId);
            salesOrderConfirmationsCommand.Parameters.AddWithValue("@buyerId", buyerId);
            salesOrderConfirmationsCommand.Parameters.AddWithValue("@senderEmailAddress", "seller@seller.com");
            salesOrderConfirmationsCommand.Parameters.AddWithValue("@emailSubject", "Your order has been confirmed");
            salesOrderConfirmationsCommand.Parameters.AddWithValue("@emailBody", "Thank you for your business");
            salesOrderConfirmationsCommand.Parameters.AddWithValue("@status", "Pending");
            await salesOrderConfirmationsCommand.ExecuteNonQueryAsync();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}