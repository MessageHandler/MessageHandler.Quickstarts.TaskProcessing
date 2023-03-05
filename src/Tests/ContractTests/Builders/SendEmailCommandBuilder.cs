using Contract;
using System;
using System.Collections.Generic;

namespace ContractTests
{
    public class SendEmailCommandBuilder
    {
        private SendEmailCommand _command;

        public SendEmailCommandBuilder()
        {
            _command = new SendEmailCommand
            {
                OrderId = Guid.NewGuid().ToString(),
                BuyerId = Guid.NewGuid().ToString(),
                SenderEmailAddress = string.Empty,
                BuyerEmailAddress = string.Empty,
                EmailSubject = string.Empty,
                EmailBody = string.Empty
            };
        }

        public SendEmailCommandBuilder WellknownEmail(string bookingId)
        {
            if (_wellknownCommands.ContainsKey(bookingId))
            {
                _command = _wellknownCommands[bookingId]();
            }

            return this;
        }

        public SendEmailCommand Build()
        {
            return _command;
        }

        private readonly Dictionary<string, Func<SendEmailCommand>> _wellknownCommands = new()
        {
            {
                "a6c5018e-5a3f-4722-9b85-0c2d4eeccb2e",
                () =>
                {
                    return new SendEmailCommand()
                    {
                        OrderId = "a6c5018e-5a3f-4722-9b85-0c2d4eeccb2e",
                        BuyerId = "buyer1",
                        SenderEmailAddress = "seller@seller.com",
                        BuyerEmailAddress = "buyer@buyer.com",
                        EmailSubject = "Your order has been confirmed",
                        EmailBody = "Thank you for your business"
                    };
                }
            }
        };
    }
}