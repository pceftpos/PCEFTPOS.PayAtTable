using System;

namespace PayAtTable.Server.Data
{
    /// <summary>
    /// A request was made for a resource that does not exist
    /// </summary>
    public class ResourceNotFoundException: Exception
    {
        public ResourceNotFoundException() : 
            base()
        {
        }


        public ResourceNotFoundException(string message) : 
            base(message)
        {
        }

        public ResourceNotFoundException(string message, Exception innerException) :
            base(message, innerException)
        {
        }
    }

    /// <summary>
    /// The request contains invalid data
    /// </summary>
    public class InvalidRequestException : Exception
    {
        public InvalidRequestException(): 
            base()
        {
        }

        public InvalidRequestException(string message) : 
            base(message)
        {
        }

        public InvalidRequestException(string message, Exception innerException) :
            base(message, innerException)
        {
        }
    }
}