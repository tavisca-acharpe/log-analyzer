using Amazon;
using Amazon.SimpleEmail;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Log.Analyzer.EmailAdapter
{
    public class EmailNotifier : INotifier
    {
        public EmailNotifier()
        {
        }

        public async Task SendNotification(string report)
        {
            System.Console.WriteLine("Email detailed fetched. : " + string.Join(" | ", "acharpe@tavisca.com"));
            try
            {
                using (var client = new AmazonSimpleEmailServiceClient(RegionEndpoint.USEast1))
                {
                    var request = report.ToAmazonSimpleEmailModel();
                    System.Console.WriteLine("Send email request created");
                    await client.SendEmailAsync(request, CancellationToken.None);
                    System.Console.WriteLine("Email has been sent.");
                }
            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ex.Message);
                throw;
            }
        }
    }
}
