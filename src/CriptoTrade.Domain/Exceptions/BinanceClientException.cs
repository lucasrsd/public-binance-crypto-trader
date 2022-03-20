using System;
using System.Collections.Generic;
using System.Text;

namespace CriptoTrade.Domain.Exceptions
{
    public class BinanceClientException : Exception
    {
        public BinanceClientException()
        {
        }

        public BinanceClientException(string message)
            : base(message)
        {
        }

        public BinanceClientException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
