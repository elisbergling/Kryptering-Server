using System;

namespace Kryptering_Server
{
    public class NoMessagesException : Exception
    {
        string message;

        public override string Message
        {
            get => message;
        }

        public NoMessagesException()
        {
            message = "Det fanns inga meddelanden att hämta.";
        }
    }
}