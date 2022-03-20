using System;
using System.Collections.Generic;
using System.Text;

namespace CriptoTrade.Domain.Exceptions
{
    public class NullCommandInputException : Exception
    {
        public NullCommandInputException()
        {
        }

        public NullCommandInputException(string message)
            : base(message)
        {
        }

        public NullCommandInputException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
