using System;

namespace Cassette
{
    class InvalidBundleManifestException : Exception
    {
        public InvalidBundleManifestException(string message) : base(message)
        {   
        }

        public InvalidBundleManifestException(string message, Exception innerException) : base(message, innerException)
        {   
        }
    }
}