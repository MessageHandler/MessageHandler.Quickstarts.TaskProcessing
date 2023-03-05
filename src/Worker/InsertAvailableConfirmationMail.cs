using System.Data.SqlClient;

namespace Worker
{
    public class InsertAvailableConfirmationMail : IHostedService
    {
        private readonly string createSalesOrderConfirmationsSqlCommand = @"if not exists (select * from sysobjects where name='SalesOrderConfirmations' and xtype='U') CREATE TABLE SalesOrderConfirmations ([OrderId] nvarchar(255), [BuyerId] nvarchar(255), [SenderEmailAddress] nvarchar(255), [EmailSubject] nvarchar(255), [EmailBody] nvarchar(255), [Status] nvarchar(255), CONSTRAINT PK_SalesOrderConfirmations PRIMARY KEY ([OrderId]));";
        private readonly string createNotificationPreferencesSqlCommand = @"if not exists (select * from sysobjects where name='NotificationPreferences' and xtype='U') CREATE TABLE NotificationPreferences ([BuyerId] nvarchar(255), [EmailAddress] nvarchar(255), CONSTRAINT PK_NotificationPreferences PRIMARY KEY ([BuyerId]));";

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

            using var createSalesOrderConfirmationsCommand = new SqlCommand(createSalesOrderConfirmationsSqlCommand, connection);
            await createSalesOrderConfirmationsCommand.ExecuteNonQueryAsync();

            using var createNotificationPreferencesCommand = new SqlCommand(createNotificationPreferencesSqlCommand, connection);
            await createNotificationPreferencesCommand.ExecuteNonQueryAsync();

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