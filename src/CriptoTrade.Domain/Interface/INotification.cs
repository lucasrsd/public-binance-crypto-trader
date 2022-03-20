using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CriptoTrade.Domain.Interface
{
    public interface INotification
    {
        Task SendMessage(string group, string message);
    }
}
