using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CriptoTrade.Command.Handlers
{
    public interface ICommandHandler<TInput> where TInput : ICommandInput
    {
        Task<ICommandOutput<TInput>> HandleAsync(TInput input); 
    }
}
