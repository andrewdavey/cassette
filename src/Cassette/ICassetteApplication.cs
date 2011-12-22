using System;
using Cassette.Configuration;

namespace Cassette
{
    public interface ICassetteApplication : IDisposable
    {
        CassetteSettings Settings { get; }

        T FindBundleContainingPath<T>(string path) where T : Bundle;

        IReferenceBuilder GetReferenceBuilder();
    }
}