using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Cassette.BundleProcessing;
using Cassette.IO;
using Moq;
using Should;
using Xunit;


namespace Cassette.Configuration
{
    public class DiskBackedBundleCache_Tests
    {

        #region Setup and Helper Methods

        Mock<IFileHelper> fileHelper;
        Mock<IDictionary<string, string>> uncachedToCachedFiles;
        IAsset fileAsset, concatenatedAsset;
        TestableBundle fileBundle, concatenatedBundle, emptyBundle;
        DiskBackedBundleCache diskBackedBundleCache;
        List<string> unprocessedAssetPaths;
        MethodInfo AddToDiskMethodInfo, GetFromDiskMethodInfo;

        public DiskBackedBundleCache_Tests()
        {
            fileHelper = new Mock<IFileHelper>(MockBehavior.Strict);
            fileHelper.Setup(fh => fh.PrepareCachingDirectory(It.IsAny<string>(), It.IsAny<string>()));
            uncachedToCachedFiles = new Mock<IDictionary<string, string>>(MockBehavior.Strict);
            diskBackedBundleCache = new DiskBackedBundleCache(fileHelper.Object);
            unprocessedAssetPaths = new List<string>();
            fileBundle = new TestableBundle("~/file");
            concatenatedBundle = new TestableBundle("~/concatenated");
            emptyBundle = new TestableBundle("~/empty");
            var stubFileCreation = typeof(Asset_Tests).GetMethod("StubFile", BindingFlags.NonPublic | BindingFlags.Instance);
            fileAsset = new FileAsset((IFile)stubFileCreation.Invoke(new Asset_Tests(), new object[] { "asset content", Type.Missing }), fileBundle);
            fileBundle.Assets.Add(fileAsset);
            concatenatedAsset = new ConcatenatedAsset(new List<IAsset> {fileAsset} );
            concatenatedBundle.Assets.Add(concatenatedAsset);
            AddToDiskMethodInfo = typeof(DiskBackedBundleCache).GetMethod("AddToDisk", BindingFlags.NonPublic | BindingFlags.Instance);
            GetFromDiskMethodInfo = typeof(DiskBackedBundleCache).GetMethod("GetFromDisk", BindingFlags.NonPublic | BindingFlags.Instance);
        }

        void AddToDisk(IFileHelper fileHelper, IDictionary<string, string> uncachedToCachedFiles, Bundle bundle, 
            IEnumerable<string> unprocessedAssetPaths)
        {
            AddToDiskMethodInfo.Invoke(diskBackedBundleCache, new object[]
                                       { fileHelper, uncachedToCachedFiles, bundle, unprocessedAssetPaths });
        }

        void GetFromDisk(IFileHelper fileHelper, IDictionary<string, string> uncachedToCachedFiles, Bundle bundle)
        {
            GetFromDiskMethodInfo.Invoke(diskBackedBundleCache,
                                         new object[] { fileHelper, uncachedToCachedFiles, bundle });
        }

        #endregion

        #region AddBundle

        [Fact]
        public void AddBundle_EmptyBundleCachedAndNotCached()
        {
            fileHelper.Setup(fh => fh.Write(It.IsAny<string>(), It.IsAny<string>()))
                                                                 .Throws(new Exception("tried to write an asset when none exists"));
            uncachedToCachedFiles.Setup(d => d.ContainsKey(It.IsAny<string>()))
                                                                .Returns(true)
                                                                .Verifiable();
            unprocessedAssetPaths.Add("wasd");
            diskBackedBundleCache.AddBundle(fileHelper.Object, uncachedToCachedFiles.Object, emptyBundle.Path, emptyBundle, unprocessedAssetPaths);
            diskBackedBundleCache.AddBundle(fileHelper.Object, uncachedToCachedFiles.Object, emptyBundle.Path, emptyBundle, unprocessedAssetPaths);
            uncachedToCachedFiles.Verify(d => d.ContainsKey(unprocessedAssetPaths.First()), Times.Once());
        }

        [Fact]
        public void AddBundle_FileAsset()
        {
            fileHelper.Setup(fh => fh.Write(It.IsAny<string>(), It.IsAny<string>()))
                                                                .Verifiable();
            uncachedToCachedFiles.Setup(d => d.ContainsKey(fileAsset.SourceFile.FullPath))
                .Returns(false)
                .Verifiable();
            uncachedToCachedFiles.Setup(d => d.Add(fileAsset.SourceFile.FullPath, fileAsset.SourceFile.FullPath))
                .Verifiable();
            diskBackedBundleCache.AddBundle(fileHelper.Object, uncachedToCachedFiles.Object, fileBundle.Path, fileBundle, unprocessedAssetPaths);
            fileHelper.Verify();
        }

        #endregion 

        #region GetAssetPaths

        [Fact]
        public void GetAssetPaths_EmptyConcatenatedFile()
        {
            diskBackedBundleCache.GetAssetPaths(emptyBundle).ShouldBeEmpty();
            diskBackedBundleCache.GetAssetPaths(fileBundle).ShouldContain(fileAsset.SourceFile.FullPath);
            //diskBackedBundleCache.GetAssetPaths(concatenatedBundle).ShouldBeEmpty();
        }

        #endregion

        #region GetBundle

        [Fact]
        public void GetBundle_NotPresent()
        {
            fileHelper.Setup(fh => fh.Exists(It.IsAny<string>()))
                .Returns(false)
                .Verifiable();
            diskBackedBundleCache.GetBundle(fileHelper.Object, uncachedToCachedFiles.Object, fileBundle.Path, fileBundle).ShouldBeNull();
            fileHelper.Verify();
        }

        [Fact]
        public void GetBundle_PresentIfNoAssets()
        {
            uncachedToCachedFiles.Setup(d => d.ContainsKey(It.IsAny<string>()))
                .Returns(false)
                .Verifiable();
            diskBackedBundleCache.GetBundle(fileHelper.Object, uncachedToCachedFiles.Object, emptyBundle.Path, emptyBundle).ShouldNotBeNull();
            fileHelper.Verify();
        }
        
        [Fact]
        public void GetBundle_PresentWithFileBundle()
        {
            /*fileHelper.Setup(fh => fh.GetAssetReferencesFromDisk(It.IsAny<FileAsset>(), It.IsAny<string>()))
                                                                .Verifiable();*/
            fileHelper.Setup(fh => fh.Exists(It.IsAny<string>()))
                .Returns(true)
                .Verifiable();
            fileHelper.Setup(fh => fh.GetLastAccessTime(It.IsAny<string>()))
                .Returns(DateTime.Today)
                .Verifiable();
            fileHelper.Setup(fh => fh.ReadAllText(It.IsAny<string>()))
                .Returns("[]");
            uncachedToCachedFiles.Setup(d => d.ContainsKey(It.IsAny<string>()))
                .Returns(false)
                .Verifiable();
            uncachedToCachedFiles.Setup(d => d.Add(It.IsAny<string>(), It.IsAny<string>()))
                .Verifiable();
            //File asset should have same file, but it should not point be the same one, as fileBundle.Assets should have been cleared
            Path.GetFileName(diskBackedBundleCache.GetBundle(fileHelper.Object, uncachedToCachedFiles.Object, fileBundle.Path, fileBundle)
                .Assets.First().SourceFile.FullPath).Trim().ShouldContain(Path.GetFileNameWithoutExtension(fileAsset.SourceFile.FullPath).Trim());
            fileBundle.Assets.First().ShouldNotBeSameAs(fileAsset);
            fileHelper.Verify();
            uncachedToCachedFiles.Verify();
        }

        #endregion

        #region ContainsKey
        /// Will not check if a bundle is present or not present as that is covered with GetBundle testing.
      
        [Fact]
        public void ContainsKey_PresentInMemory()
        {
            fileHelper.Setup(fh => fh.Exists(It.IsAny<string>()))
                .Returns(true);
            fileHelper.Setup(fh => fh.GetLastAccessTime(It.IsAny<string>()))
                .Returns(DateTime.Today);
            uncachedToCachedFiles.Setup(d => d.ContainsKey(It.IsAny<string>()))
                .Returns(false);
            uncachedToCachedFiles.Setup(d => d.Add(It.IsAny<string>(), It.IsAny<string>()));
            fileHelper.Setup(fh => fh.ReadAllText(It.IsAny<string>()))
                .Returns("[]");
            diskBackedBundleCache.ContainsKey(fileHelper.Object, uncachedToCachedFiles.Object, fileBundle.Path, fileBundle);
            
            fileHelper.Setup(fh => fh.Exists(It.IsAny<string>()))
                .Throws(new Exception("Did not find bundle that should have been in memory."));
            fileHelper.Setup(fh => fh.GetLastAccessTime(It.IsAny<string>()))
                .Throws(new Exception("Did not find bundle that should have been in memory."));
            diskBackedBundleCache.ContainsKey(fileHelper.Object, uncachedToCachedFiles.Object, fileBundle.Path, fileBundle);
        }

        #endregion

        #region FixReferences

        [Fact]
        public void FixReferences_FileAndEmptyBundles()
        {
            fileHelper.Setup(fh => fh.Write(It.IsAny<string>(), It.IsAny<string>()));
            uncachedToCachedFiles.Setup(d => d.ContainsKey(It.IsAny<string>()))
                                                                .Returns(true);
            diskBackedBundleCache.AddBundle(fileHelper.Object, uncachedToCachedFiles.Object, fileBundle.Path, fileBundle, unprocessedAssetPaths);
            unprocessedAssetPaths.Add("wasd");
            uncachedToCachedFiles.Setup(d => d.ContainsKey(It.IsAny<string>()))
                .Returns(true);
            diskBackedBundleCache.AddBundle(fileHelper.Object, uncachedToCachedFiles.Object, emptyBundle.Path, emptyBundle, unprocessedAssetPaths);
            var stubFileCreation = typeof(Asset_Tests).GetMethod("StubFile", BindingFlags.NonPublic | BindingFlags.Instance);
            var fileAssetChild = new FileAsset((IFile)stubFileCreation.Invoke(new Asset_Tests(), new object[] { "asset content", "~/file2" }), fileBundle);
            fileAsset.AddReference(fileAssetChild.SourceFile.FullPath, 1);
            fileBundle.Assets.Add(fileAssetChild);
            var containsDict = new Dictionary<string, string>();
            var originalPath = fileAsset.References.First().Path;

            containsDict.Add(fileAsset.References.First().Path, "good test result");
            diskBackedBundleCache.FixReferences(containsDict, new List<Bundle> { fileBundle });
            fileAsset.References.First().Path.ShouldEqual("good test result");

            containsDict.Add("good test result", "bad test result");
            diskBackedBundleCache.FixReferences(containsDict, new List<Bundle> { fileBundle });
            fileAsset.References.First().Path.ShouldEqual("good test result");

            fileAssetChild.AddReference("http://www.google.com", -1);
            containsDict.Add("http://www.google.com", "bad test result");
            diskBackedBundleCache.FixReferences(containsDict, new List<Bundle> { fileBundle });
            fileAssetChild.References.First().Path.ShouldEqual("http://www.google.com");

            //Just making sure it doesn't do anything with an empty bundle.
            diskBackedBundleCache.FixReferences(containsDict, new List<Bundle> { emptyBundle });
        }

        #endregion

        #region AddToDisk
        ///just checking unprocessedAssetPath as other code paths checked by AddBundle tests
        
        [Fact]
        public void AddToDisk_UnprocessedAssetPaths()
        {
            uncachedToCachedFiles.Setup(d => d.ContainsKey(It.Is<string>(s => s.Equals(fileAsset.SourceFile.FullPath))))
                .Returns(false)
                .Verifiable();
            uncachedToCachedFiles.Setup(d => d.Add(It.Is<string>(s => s.Equals(fileAsset.SourceFile.FullPath)),
                It.Is<string>(s => s.Equals(emptyBundle.Path))))
                .Verifiable();
            unprocessedAssetPaths.Add(fileAsset.SourceFile.FullPath);
            AddToDisk(fileHelper.Object, uncachedToCachedFiles.Object, emptyBundle, unprocessedAssetPaths);
            uncachedToCachedFiles.Verify();
            uncachedToCachedFiles.Verify(d => d.Add(It.IsAny<string>(), It.IsAny<string>()), Times.Once());
        }
        
        #endregion

        #region GetFromDisk

        [Fact]
        public void GetFromDisk_AllButOneAssetsFoundOnDisk()
        {
            var stubFileCreation = typeof(Asset_Tests).GetMethod("StubFile", BindingFlags.NonPublic | BindingFlags.Instance);
            var fileAsset2 = new FileAsset((IFile)stubFileCreation.Invoke(new Asset_Tests(), 
                new object[] { "asset content", "~/dont" }), fileBundle);
            fileBundle.Assets.Add(fileAsset2);

            fileHelper.Setup(fh => fh.Exists(It.Is<string>(s => s.Contains("\\pK35VRqYM2h2uRH9XIbkKnzqc8U"))))
                .Returns(false);
            fileHelper.Setup(fh => fh.Exists(It.Is<string>(s => !s.Contains("\\pK35VRqYM2h2uRH9XIbkKnzqc8U"))))
                .Returns(true);
            fileHelper.Setup(fh => fh.GetLastAccessTime(It.IsAny<string>()))
                .Returns(DateTime.Today);
            fileHelper.Setup(fh => fh.ReadAllText(It.IsAny<string>()))
                .Returns("[]");

            uncachedToCachedFiles.Setup(d => d.ContainsKey(It.IsAny<string>()))
                .Returns(false);
            uncachedToCachedFiles.Setup(d => d.Add(It.IsAny<string>(), It.IsAny<string>()))
                .Throws(new Exception(
                        "Tried add a cached asset to the lookup dictionary when the bundle was not fully cached"));
            GetFromDisk(fileHelper.Object, uncachedToCachedFiles.Object, fileBundle);
        }

        #endregion

        #region CreateFileOnDiskFromAsset
        ///Many code paths are checked by other tests

        [Fact]
        public void CreateFileOnDiskFromAsset_CheckProperDehydration()
        {
            fileHelper.Setup(fh => fh.Write(It.IsAny<string>(), "asset content"))
                .Verifiable();
            fileHelper.Setup(fh => fh.Write(It.IsAny<string>(), "[{\"AssetReferenceType\":1,\"LineNumber\":1,\"Path\":\"~/file2\"}]"))
                .Verifiable();
            fileHelper.Setup(fh => fh.Write(It.IsAny<string>(), "[]"))
                .Verifiable();
            fileHelper.Setup(fh => fh.Exists(It.IsAny<string>()))
                .Returns(true);
            fileHelper.Setup(fh => fh.GetLastAccessTime(It.IsAny<string>()))
                .Returns(DateTime.Today);
            uncachedToCachedFiles.Setup(d => d.ContainsKey(It.IsAny<string>()))
                .Returns(false);
            uncachedToCachedFiles.Setup(d => d.Add(It.IsAny<string>(), It.IsAny<string>()));
            unprocessedAssetPaths.Add("~/a");
            var stubFileCreation = typeof(Asset_Tests).GetMethod("StubFile", BindingFlags.NonPublic | BindingFlags.Instance);
            var fileAssetChild = new FileAsset((IFile)stubFileCreation.Invoke(new Asset_Tests(), new object[] { "asset content", "~/file2" }), fileBundle);
            fileAsset.AddReference(fileAssetChild.SourceFile.FullPath, 1);
            fileBundle.Assets.Add(fileAssetChild);
            AddToDisk(fileHelper.Object, uncachedToCachedFiles.Object, fileBundle, unprocessedAssetPaths);
            fileBundle.Assets.First().References.First().Path.Equals(fileAssetChild.SourceFile.FullPath);
            fileHelper.Verify();
            fileHelper.Verify(fh => fh.Write(It.IsAny<string>(), "[{\"AssetReferenceType\":1,\"LineNumber\":1,\"Path\":\"~/file2\"}]"), Times.Once());
            fileHelper.Verify(fh => fh.Write(It.IsAny<string>(), "[]"), Times.Once());
            fileHelper.Verify(fh => fh.Write(It.IsAny<string>(), "asset content"), Times.Exactly(2));
        }

        #endregion

        #region GetAssetReferencesFromDisk
        ///Many code paths are checked by the GetBundle and ContainsKey tests
         
        [Fact]
        public void GetAssetReferencesFromDisk_CheckProperRehydration()
        {
            fileHelper.Setup(fh => fh.ReadAllText(It.IsAny<string>()))
                .Returns("[{\"AssetReferenceType\":1,\"LineNumber\":1,\"Path\":\"~/file2\"}]")
                .Verifiable();
            fileHelper.Setup(fh => fh.Exists(It.IsAny<string>()))
                .Returns(true);
            fileHelper.Setup(fh => fh.GetLastAccessTime(It.IsAny<string>()))
                .Returns(DateTime.Today);
            uncachedToCachedFiles.Setup(fh => fh.ContainsKey(It.IsAny<string>()))
                .Returns(true);
            var stubFileCreation = typeof(Asset_Tests).GetMethod("StubFile", BindingFlags.NonPublic | BindingFlags.Instance);
            var fileAssetChild = new FileAsset((IFile)stubFileCreation.Invoke(new Asset_Tests(), new object[] { "asset content", "~/file2" }), fileBundle);
            fileAsset.AddReference(fileAssetChild.SourceFile.FullPath, 1);
            fileBundle.Assets.Add(fileAssetChild);
            GetFromDisk(fileHelper.Object, uncachedToCachedFiles.Object, fileBundle);
            fileBundle.Assets.First().References.First().Path.Equals(fileAssetChild.SourceFile.FullPath);
            fileHelper.Verify();
        }

        #endregion

    }
}
