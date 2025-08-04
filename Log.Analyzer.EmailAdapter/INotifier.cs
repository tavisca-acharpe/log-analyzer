using System.Collections.Generic;
using System.Threading.Tasks;

namespace Log.Analyzer.EmailAdapter
{
    public interface INotifier
    {
        Task SendNotification(string report, List<string> toAddressesEmail);
    }
}
