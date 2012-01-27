﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Cassette.Configuration;
using Cassette.IO;
using Cassette.Utilities;

namespace Cassette.Persistence
{
    class BundleCache : IBundleCache
    {
        public BundleCache(string version, CassetteSettings settings)
        {
            this.version = version;
            this.settings = settings;
            cacheDirectory = settings.CacheDirectory;
            sourceDirectory = settings.SourceDirectory;
            containerFile = cacheDirectory.GetFile(ContainerFilename);
        }

        const string ContainerFilename = "container.xml";

        readonly string version;
        readonly CassetteSettings settings;
        readonly IDirectory cacheDirectory;
        readonly IDirectory sourceDirectory;
        readonly IFile containerFile;

        public bool InitializeBundlesFromCacheIfUpToDate(IEnumerable<Bundle> unprocessedSourceBundles)
        {
            if (!containerFile.Exists) return false;

            var bundles = unprocessedSourceBundles.ToArray();
            if (CacheFileIsOlderThanAnyAsset(bundles)) return false;

            var containerXml = LoadContainerElement();
            if (!IsSameVersion(containerXml)) return false;
            if (!IsSameAssetCount(bundles, containerXml)) return false;
            if (!FilesAreUpToDate(containerXml)) return false;

            var bundleInitializationActions = (
                from bundle in bundles
                select CreateBundleInitializationAction(bundle, containerXml)
            ).ToArray();

            if (bundleInitializationActions.Any(c => c == null)) return false;

            foreach (var assignCachedAsset in bundleInitializationActions)
            {
                assignCachedAsset();
            }

            return true;
        }

        public void Clear()
        {
            DeleteExistingFiles();
        }

        bool FilesAreUpToDate(XElement containerXml)
        {
            var filePaths = (
                from e in containerXml.Descendants("File")
                let pathAttribute = e.Attribute("Path")
                where pathAttribute != null
                select pathAttribute.Value.Substring(2) // Removes the "~/" prefix
            );

            var files = filePaths
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Select(sourceDirectory.GetFile)
                .ToArray();

            if (files.Length == 0) return true;

            if (files.Any(f => !f.Exists)) return false;

            var maxLastWriteTimeUtc = files.Select(f => f.LastWriteTimeUtc).Max();
            return maxLastWriteTimeUtc <= containerFile.LastWriteTimeUtc;
        }

        bool CacheFileIsOlderThanAnyAsset(IEnumerable<Bundle> unprocessedSourceBundles)
        {
            var finder = new AssetLastWriteTimeFinder();
            finder.Visit(unprocessedSourceBundles);
            return containerFile.LastWriteTimeUtc < finder.MaxLastWriteTimeUtc;
        }

        bool IsSameVersion(XElement containerXml)
        {
            var versionAttribute = containerXml.Attribute("Version");
            if (versionAttribute == null) return false;

            return versionAttribute.Value == version;
        }

        bool IsSameAssetCount(IEnumerable<Bundle> unprocessedSourceBundles, XElement containerXml)
        {
            var assetCountAttribute = containerXml.Attribute("AssetCount");
            if (assetCountAttribute == null) return false;
            
            int assetCount;
            if (int.TryParse(assetCountAttribute.Value, out assetCount) == false) return false;
            
            var assetCounter = new AssetCounter();
            assetCounter.Visit(unprocessedSourceBundles);

            return assetCount == assetCounter.Count;
        }

        Action CreateBundleInitializationAction(Bundle bundle, XElement containerElement)
        {
            var bundleElement = GetBundleElement(bundle, containerElement);
            if (bundleElement == null) return null;

            var hash = GetHash(bundleElement);
            if (hash == null) return null;

            var filename = BundleAssetCacheFilename(bundle);
            var file = cacheDirectory.GetFile(filename);
            if (bundle.Assets.Count > 0 && !file.Exists) return null;

            var childAssets = bundle.Assets.ToArray();
            var references = GetBundleReferences(bundleElement).ToArray();
            return () =>
            {
                if (bundle.Assets.Count > 0)
                {
                    bundle.Assets.Clear();
                    bundle.Assets.Add(new CachedAsset(file, hash, childAssets));
                }
                foreach (var reference in references)
                {
                    bundle.AddReference(reference);                    
                }
                bundle.IsFromCache = true;
                bundle.Process(settings);
            };
        }

        IEnumerable<string> GetBundleReferences(XElement bundleElement)
        {
            return from e in bundleElement.Elements("Reference")
                   let pathAttribute = e.Attribute("Path")
                   where pathAttribute != null
                   select pathAttribute.Value;
        }

        XElement GetBundleElement(Bundle bundle, XElement containerElement)
        {
            return (
                from e in containerElement.Elements("Bundle")
                let pathAttribute = e.Attribute("Path")
                where pathAttribute != null
                   && pathAttribute.Value == bundle.Path
                select e
            ).FirstOrDefault();
        }

        byte[] GetHash(XElement bundleElement)
        {
            var attribute = bundleElement.Attribute("Hash");
            if (attribute == null) return null;
            return ByteArrayExtensions.FromHexString(attribute.Value);
        }

        public void SaveBundleContainer(IBundleContainer bundleContainer)
        {
            Trace.Source.TraceInformation("Saving bundle container to cache.");
            DeleteExistingFiles();
            SaveContainerXml(bundleContainer);
            foreach (var bundle in bundleContainer.Bundles)
            {
                SaveBundle(bundle);
            }
            Trace.Source.TraceInformation("Saved bundle container to cache.");
        }

        void DeleteExistingFiles()
        {
            foreach (var file in cacheDirectory.GetFiles("*", SearchOption.AllDirectories))
            {
                try
                {
                    file.Delete();
                }
// ReSharper disable EmptyGeneralCatchClause
                catch 
// ReSharper restore EmptyGeneralCatchClause
                {
                    // Isolated storage files can get locked, when different user is accessing.
                    // We can't do anything about it, so just skip this error.
                }
            }
        }

        XElement LoadContainerElement()
        {
            using (var containerFileStream = containerFile.OpenRead())
            {
                return XDocument.Load(containerFileStream).Root;
            }
        }

        void SaveContainerXml(IBundleContainer bundleContainer)
        {
            var assetCounter = new AssetCounter();
            assetCounter.Visit(bundleContainer.Bundles);

            var xml = new XDocument(
                new XElement(
                    "Container",
                    new XAttribute("Version", version),
                    new XAttribute("AssetCount", assetCounter.Count),
                    from bundle in bundleContainer.Bundles
                    select CreateBundleElement(bundle, bundleContainer)
                )
            );
            using (var fileStream = containerFile.Open(FileMode.Create, FileAccess.Write, FileShare.None))
            {
                xml.Save(fileStream);
            }
        }

        XElement CreateBundleElement(Bundle bundle, IBundleContainer bundleContainer)
        {
            var referenceElements = CreateReferenceElements(bundle, bundleContainer);
            var fileElements = CreateFileElements(bundle);
            return new XElement(
                "Bundle",
                new XAttribute("Path", bundle.Path),
                bundle.Assets.Count > 0 ? new XAttribute("Hash", bundle.Hash.ToHexString()) : null,
                referenceElements,
                fileElements
            );
        }

        IEnumerable<XElement> CreateFileElements(Bundle bundle)
        {
            var fileReferences = (
                from asset in bundle.Assets
                from r in asset.References
                where r.Type == AssetReferenceType.RawFilename
                select r.Path
            ).Distinct(StringComparer.OrdinalIgnoreCase);

            return from file in fileReferences
                   select new XElement("File", new XAttribute("Path", file));
        }

        IEnumerable<XElement> CreateReferenceElements(Bundle bundle, IBundleContainer bundleContainer)
        {
            var paths = from asset in bundle.Assets
                        from r in asset.References
                        where r.Type == AssetReferenceType.DifferentBundle || r.Type == AssetReferenceType.Url
                        select r.Path;

            var references = paths
                .SelectMany(bundleContainer.FindBundlesContainingPath)
                .Select(b => b.Path)
                .Concat(bundle.References)
                .Distinct(StringComparer.OrdinalIgnoreCase);

            return from reference in references
                   select new XElement("Reference", new XAttribute("Path", reference));
        }

        void SaveBundle(Bundle bundle)
        {
            if (bundle.Assets.Count == 0) return;

            if (bundle.Assets.Count > 1)
            {
                throw new InvalidOperationException("Cannot cache a bundle when assets have not been concatenated into a single asset.");
            }
            
            Trace.Source.TraceInformation("Saving bundle \"{0}\" to cache.", bundle.Path);
            var file = cacheDirectory.GetFile(BundleAssetCacheFilename(bundle));
            using (var fileStream = file.Open(FileMode.Create, FileAccess.Write, FileShare.None))
            {
                using (var dataStream = bundle.Assets[0].OpenStream())
                {
                    dataStream.CopyTo(fileStream);
                }
                fileStream.Flush();
            }
            Trace.Source.TraceInformation("Saved bundle \"{0}\" to cache.", bundle.Path);
        }

        string BundleAssetCacheFilename(Bundle bundle)
        {
            if (bundle.Path.IsUrl())
            {
                throw new ArgumentException("Cannot cache bundle with a URL as its path.");
            }

            return bundle.PathWithoutPrefix
                         .Replace(Path.DirectorySeparatorChar, '`')
                         .Replace(Path.AltDirectorySeparatorChar, '`')
                   + ".bundle";
        }
    }
}
