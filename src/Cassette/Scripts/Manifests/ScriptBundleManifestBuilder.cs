using Cassette.Manifests;

namespace Cassette.Scripts.Manifests
{
    class ScriptBundleManifestBuilder<TBundle, TManifest> : BundleManifestBuilder<TBundle, TManifest> 
        where TBundle : ScriptBundle
        where TManifest : ScriptBundleManifest, new()
    {
    }

    class ScriptBundleManifestBuilder : BundleManifestBuilder<ScriptBundle, ScriptBundleManifest>
    {
    }
}