using System;
using Serilog;

namespace stranitza.Utility
{
    public class StranitzaException : Exception
    {
        public StranitzaException(string message) : base(message)
        {
            Log.Logger.Error(message);
        }

        public StranitzaException(string message, Exception innerException) : base(message, innerException)
        {
            Log.Logger.Error(innerException, message);
        }
    }
}
