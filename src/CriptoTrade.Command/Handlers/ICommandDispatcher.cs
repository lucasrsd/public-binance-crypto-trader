using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CriptoTrade.Command.Handlers
{
    public interface ICommandDispatcher
    {
        Task<ICommandOutput<TCommandInput>> DispatchAsync<TCommandInput>(TCommandInput input) where TCommandInput : ICommandInput;
    }
}
