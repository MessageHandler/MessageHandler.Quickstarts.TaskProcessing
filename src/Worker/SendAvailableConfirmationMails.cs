using Contract;
using MessageHandler.Runtime.AtomicProcessing;

namespace Worker
{
    public class SendAvailableConfirmationMails
    {
        private readonly ILogger<SendAvailableConfirmationMails> logger;
        private readonly IDispatchMessages dispatcher;
        private readonly IPersistAvailableConfirmationMails storage;

        public SendAvailableConfirmationMails(IPersistAvailableConfirmationMails storage, IDispatchMessages dispatcher, ILogger<SendAvailableConfirmationMails> logger = null!)
        {
            this.logger = logger;
            this.dispatcher = dispatcher;
            this.storage = storage;
        }

        public async Task ProcessAsync(CancellationToken stoppingToken)
        {
            var email = await storage.GetAvailableConfirmationMail();

            if (email != null)
            {
                try
                {
                    logger?.LogInformation("Confirmation mail available, instructing the system to send it...");

                    await this.dispatcher.Dispatch(new SendEmailCommand()
                    {
                        OrderId = email.OrderId,
                        BuyerId = email.BuyerId,
                        SenderEmailAddress = email.SenderEmailAddress,
                        BuyerEmailAddress = email.BuyerEmailAddress,
                        EmailSubject = email.EmailSubject,
                        EmailBody = email.EmailBody
                    });

                    await this.storage.MarkAsSent(email);

                    logger?.LogInformation("Confirmation mail marked as sent...");
                }
                catch (Exception)
                {
                    await this.storage.MarkAsPending(email);

                    logger?.LogInformation("Sending confirmation mail failed, marked as pending...");
                }
            }
            else
            {
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }
    }
}