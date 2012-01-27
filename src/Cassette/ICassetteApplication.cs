using System;
using System.Collections.Generic;
using Cassette.Configuration;

namespace Cassette
{
    public interface ICassetteApplication : IDisposable
    {
        CassetteSettings Settings { get; }

        IEnumerable<Bundle> Bundles { get; }

        T FindBundleContainingPath<T>(string path) where T : Bundle;

        IReferenceBuilder GetReferenceBuilder();
    }
}