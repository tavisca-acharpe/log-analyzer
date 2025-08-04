using Amazon.SimpleEmail.Model;
using System.Collections.Generic;

namespace Log.Analyzer.EmailAdapter
{
    public static class AmazonSimpleEmailTranslator
    {
        public static SendEmailRequest ToAmazonSimpleEmailModel(this string messageBody, List<string> toAddressesEmail)
        {
            return new SendEmailRequest
            {
                Source = "cart-nextgen@tavisca.com",
                Destination = new Destination
                {
                    ToAddresses = toAddressesEmail
                },
                Message = new Message
                {
                    Subject = new Content("Daily Exception And Failure Analysis"),
                    Body = new Body
                    {
                        Html = new Content
                        {
                            Charset = "UTF-8",
                            Data = messageBody
                        }
                    }
                }
            };
        }
    }
}
