using System;
using System.Collections.Generic;
using System.Text;

namespace CriptoTrade.Domain.Exceptions
{
    public class TooManyRequestsException : Exception
    {
        public TooManyRequestsException()
        {
        }

        public TooManyRequestsException(string message)
            : base(message)
        {
        }

        public TooManyRequestsException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
