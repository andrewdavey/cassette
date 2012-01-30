using System;

namespace Cassette.Manifests
{
    class InvalidCassetteManifestException : Exception
    {
        public InvalidCassetteManifestException(string message) : base(message)
        {   
        }

        public InvalidCassetteManifestException(string message, Exception innerException) : base(message, innerException)
        {   
        }
    }
}