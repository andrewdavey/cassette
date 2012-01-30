namespace Cassette.Manifests
{
    interface ICassetteManifestCache
    {
        CassetteManifest LoadCassetteManifest();
        void SaveCassetteManifest(CassetteManifest cassetteManifest);
        void Clear();
    }
}