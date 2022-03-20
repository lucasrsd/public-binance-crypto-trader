using System;
using System.Collections.Generic;
using System.Text;

namespace CriptoTrade.Domain.Exceptions
{
    public class BinanceAPIInvalid : Exception
    {
        public BinanceAPIInvalid()
        {
        }

        public BinanceAPIInvalid(string message)
            : base(message)
        {
        }

        public BinanceAPIInvalid(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
