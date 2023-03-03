using Contract;
using MessageHandler.Runtime.AtomicProcessing;

namespace Worker
{
    public class SendConfirmationMail : BackgroundService
    {
        private readonly ILogger<SendConfirmationMail> logger;
        private readonly IDispatchMessages dispatcher;
        private readonly IProcessAvailableConfirmationMails processor;

        public SendConfirmationMail(IProcessAvailableConfirmationMails processor, IDispatchMessages dispatcher, ILogger<SendConfirmationMail> logger = null!)
        {
            this.logger = logger;
            this.dispatcher = dispatcher;
            this.processor = processor;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    var email = await processor.StartProcessing();

                    if (email != null)
                    {
                        try
                        {
                            logger?.LogInformation("Sending confirmation mail...");

                            await this.dispatcher.Dispatch(new SendEmailCommand()
                            {
                                OrderId = email.OrderId,
                                BuyerId = email.BuyerId,
                                SenderEmailAddress = email.SenderEmailAddress,
                                BuyerEmailAddress = email.BuyerEmailAddress,
                                EmailSubject = email.EmailSubject,
                                EmailBody = email.EmailBody,
                                Status = email.Status
                            });

                            await this.processor.MarkAsSent(email);

                            logger?.LogInformation("Confirmation mail marked as sent...");
                        }
                        catch (Exception)
                        {
                            await this.processor.MarkAsPending(email);

                            logger?.LogInformation("Sending confirmation mail failed, marked as pending...");
                        }
                    }
                    else
                    {
                        await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                    }
                }
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "{Message}", ex.Message);
            }
        }
    }
}