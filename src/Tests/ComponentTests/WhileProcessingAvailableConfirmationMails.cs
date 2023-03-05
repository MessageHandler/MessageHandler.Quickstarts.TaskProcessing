using Contract;
using ContractTests;
using MessageHandler.Runtime.AtomicProcessing;
using Moq;
using System.Threading;
using System.Threading.Tasks;
using Worker;
using Xunit;

namespace ComponentTests
{
    public class WhileProcessingAvailableConfirmationMails
    {
        [Fact]
        public async Task GivenAvailableConfirmationMail_WhenProcessingTask_ThenShouldDispatchSendEmailCommand()
        {
            // given
            var orderId = "a6c5018e-5a3f-4722-9b85-0c2d4eeccb2e";
            var command = new SendEmailCommandBuilder()
                                .WellknownEmail(orderId)
                                .Build();

            var confirmationMail = new ConfirmationMail()
            {
                OrderId = "a6c5018e-5a3f-4722-9b85-0c2d4eeccb2e",
                BuyerId = "buyer1",
                SenderEmailAddress = "seller@seller.com",
                BuyerEmailAddress = "buyer@buyer.com",
                EmailSubject = "Your order has been confirmed",
                EmailBody = "Thank you for your business",
                Status = "Pending"
            };

            var storageMock = new Mock<IPersistAvailableConfirmationMails>();
            storageMock.Setup(storage => storage.GetAvailableConfirmationMail())
                .ReturnsAsync(confirmationMail);

            var dispatcherMock = new Mock<IDispatchMessages>();

            dispatcherMock.Setup(dispatcher => dispatcher.Dispatch(It.IsAny<SendEmailCommand>()));

            //when
            var processor = new SendAvailableConfirmationMails(storageMock.Object, dispatcherMock.Object);

            await processor.ProcessAsync(CancellationToken.None);

            //then          
            storageMock.Verify(storage => storage.GetAvailableConfirmationMail(), Times.Once());
            storageMock.Verify(storage => storage.MarkAsSent(confirmationMail), Times.Once());
            dispatcherMock.Verify(dispatcher => dispatcher.Dispatch(It.Is<SendEmailCommand>(cmd => 
                        cmd.OrderId == command.OrderId && 
                        cmd.BuyerId == command.BuyerId &&
                        cmd.SenderEmailAddress == command.SenderEmailAddress &&
                        cmd.BuyerEmailAddress == command.BuyerEmailAddress &&
                        cmd.EmailSubject == command.EmailSubject &&
                        cmd.EmailBody == command.EmailBody
            )), Times.Once());
        }
    }
}