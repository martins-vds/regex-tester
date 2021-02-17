using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace RegexTester
{
    [Serializable]
    public class InvalidMatchRequestException : Exception
    {
        public InvalidMatchRequestException()
        {
        }

        public InvalidMatchRequestException(string message) : base(message)
        {
        }

        public InvalidMatchRequestException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected InvalidMatchRequestException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
