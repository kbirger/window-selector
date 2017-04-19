using System;

namespace WindowSelector.Common.Exceptions
{
    public class WindowNotFoundException : Exception
    {
        private const string MESSAGE = "A window was queried but could not be found.";
        public WindowNotFoundException() : base(MESSAGE)
        {
        }

        public WindowNotFoundException(Exception innerException) : base(MESSAGE, innerException)
        {
            
        }
    }
}
