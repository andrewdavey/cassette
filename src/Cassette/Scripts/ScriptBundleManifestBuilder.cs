namespace Cassette.Scripts
{
    class ScriptBundleManifestBuilder<TBundle, TManifest> : BundleManifestBuilder<TBundle, TManifest> 
        where TBundle : ScriptBundle
        where TManifest : BundleManifest, new()
    {
    }

    class ScriptBundleManifestBuilder : BundleManifestBuilder<ScriptBundle, BundleManifest>
    {
    }
}