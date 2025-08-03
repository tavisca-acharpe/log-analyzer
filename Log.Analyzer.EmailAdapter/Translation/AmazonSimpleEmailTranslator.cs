using Amazon.SimpleEmail.Model;

namespace Log.Analyzer.EmailAdapter
{
    public static class AmazonSimpleEmailTranslator
    {
        public static SendEmailRequest ToAmazonSimpleEmailModel(this string messageBody)
        {
            return new SendEmailRequest
            {
                Source = "cart-nextgen@tavisca.com",
                Destination = new Destination
                {
                    ToAddresses = new System.Collections.Generic.List<string> { "acharpe@tavisca.com" }
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
