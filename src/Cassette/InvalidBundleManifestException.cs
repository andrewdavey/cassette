using System;

namespace Cassette
{
    class InvalidBundleManifestException : Exception
    {
        public InvalidBundleManifestException(string message) : base(message)
        {   
        }
    }
}