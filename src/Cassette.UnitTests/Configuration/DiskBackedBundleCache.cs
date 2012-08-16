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
            fileHelper = new Mock<IFileHelper>();
            uncachedToCachedFiles = new Mock<IDictionary<string, string>>(MockBehavior.Strict);
            diskBackedBundleCache = new DiskBackedBundleCache();
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

        bool GetFromDisk(IFileHelper fileHelper, IDictionary<string, string> uncachedToCachedFiles, Bundle bundle)
        {
            return (bool) GetFromDiskMethodInfo.Invoke(diskBackedBundleCache,
                                         new object[] { fileHelper, uncachedToCachedFiles, bundle });
        }

        #endregion

        #region AddBundle

        [Fact]
        public void DiskBackedBundleCache_AddBundle_EmptyBundleCachedAndNotCached()
        {
            fileHelper.Setup(fh => fh.CreateFileOnDiskFromAsset(It.IsAny<Bundle>(),
                                                                It.IsAny<IAsset>(), It.IsAny<string>()))
                                                                .Throws(new Exception("Try to load asset that's not on bundle off disk"));
            uncachedToCachedFiles.Setup(d => d.ContainsKey(It.IsAny<string>()))
                                                                .Returns(true)
                                                                .Verifiable();
            unprocessedAssetPaths.Add("wasd");
            diskBackedBundleCache.AddBundle(fileHelper.Object, uncachedToCachedFiles.Object, emptyBundle.Path, emptyBundle, unprocessedAssetPaths);
            uncachedToCachedFiles.Verify(d => d.ContainsKey(unprocessedAssetPaths.First()), Times.Once());
            diskBackedBundleCache.AddBundle(fileHelper.Object, uncachedToCachedFiles.Object, emptyBundle.Path, emptyBundle, unprocessedAssetPaths);
            uncachedToCachedFiles.Verify(d => d.ContainsKey(unprocessedAssetPaths.First()), Times.Once());
        }

        [Fact]
        public void DiskBackedBundleCache_AddBundle_ConcatenatedAsset()
        {
            fileHelper.Setup(fh => fh.CreateFileOnDiskFromAsset(It.IsAny<Bundle>(),
                                                                It.IsAny<IAsset>(), It.IsAny<string>()))
                                                                .Verifiable();
            uncachedToCachedFiles.Setup(d => d.ContainsKey(concatenatedBundle.Path))
                .Returns(false)
                .Verifiable();
            uncachedToCachedFiles.Setup(d => d.Add(concatenatedBundle.Path, concatenatedBundle.Path))
                .Verifiable();
            diskBackedBundleCache.AddBundle(fileHelper.Object, uncachedToCachedFiles.Object, concatenatedBundle.Path, concatenatedBundle, unprocessedAssetPaths);
            fileHelper.Verify();
        }

        [Fact]
        public void DiskBackedBundleCache_AddBundle_FileAsset()
        {
            fileHelper.Setup(fh => fh.CreateFileOnDiskFromAsset(It.IsAny<Bundle>(),
                                                                It.IsAny<IAsset>(), It.IsAny<string>()))
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
        public void DiskBackedBundleCache_GetAssetPaths_EmptyConcatenatedFile()
        {
            diskBackedBundleCache.GetAssetPaths(emptyBundle).ShouldBeEmpty();
            diskBackedBundleCache.GetAssetPaths(fileBundle).ShouldContain(fileAsset.SourceFile.FullPath);
            diskBackedBundleCache.GetAssetPaths(concatenatedBundle).ShouldBeEmpty();
        }

        #endregion

        #region GetBundle

        [Fact]
        public void DiskBackedBundleCache_GetBundle_NotPresent()
        {
            fileHelper.Setup(fh => fh.Exists(It.IsAny<string>()))
                .Returns(false)
                .Verifiable();
            diskBackedBundleCache.GetBundle(fileHelper.Object, uncachedToCachedFiles.Object, fileBundle.Path, fileBundle).ShouldBeNull();
            fileHelper.Verify();
        }

        [Fact]
        public void DiskBackedBundleCache_GetBundle_PresentIfNoAssets()
        {
            uncachedToCachedFiles.Setup(d => d.ContainsKey(It.IsAny<string>()))
                .Returns(false)
                .Verifiable();
            diskBackedBundleCache.GetBundle(fileHelper.Object, uncachedToCachedFiles.Object, emptyBundle.Path, emptyBundle).ShouldNotBeNull();
            fileHelper.Verify();
        }
        
        [Fact]
        public void DiskBackedBundleCache_GetBundle_PresentWithFileBundle()
        {
            /*fileHelper.Setup(fh => fh.GetAssetReferencesFromDisk(It.IsAny<FileAsset>(), It.IsAny<string>()))
                                                                .Verifiable();*/
            fileHelper.Setup(fh => fh.Exists(It.IsAny<string>()))
                .Returns(true)
                .Verifiable();
            fileHelper.Setup(fh => fh.GetLastAccessTime(It.IsAny<string>()))
                .Returns(DateTime.Today)
                .Verifiable();
            fileHelper.Setup(fh => fh.GetFileName(It.IsAny<IAsset>(), It.IsAny<Bundle>(), It.IsAny<string>()))
                .Returns(fileAsset.SourceFile.FullPath);
            uncachedToCachedFiles.Setup(d => d.ContainsKey(It.IsAny<string>()))
                .Returns(false)
                .Verifiable();
            uncachedToCachedFiles.Setup(d => d.Add(It.IsAny<string>(), It.IsAny<string>()))
                .Verifiable();
            //File asset should have same file, but it should not point be the same one, as fileBundle.Assets should have been cleared
            Path.GetFileName(diskBackedBundleCache.GetBundle(fileHelper.Object, uncachedToCachedFiles.Object, fileBundle.Path, fileBundle)
                .Assets.First().SourceFile.FullPath).Trim().ShouldContain(Path.GetFileName(fileAsset.SourceFile.FullPath).Trim());
            fileBundle.Assets.First().ShouldNotBeSameAs(fileAsset);

            fileHelper.Verify();
            uncachedToCachedFiles.Verify();
        }

        [Fact]
        public void DiskBackedBundleCache_GetBundle_FailWithConcatenatedBundle()
        {
            /*fileHelper.Setup(fh => fh.GetAssetReferencesFromDisk(It.IsAny<FileAsset>(), It.IsAny<string>()))
                                                                .Verifiable();*/
            fileHelper.Setup(fh => fh.Exists(It.IsAny<string>()))
                .Returns(true);
            fileHelper.Setup(fh => fh.GetLastAccessTime(It.IsAny<string>()))
                .Returns(DateTime.Today);
            fileHelper.Setup(fh => fh.GetFileName(It.IsAny<IAsset>(), It.IsAny<Bundle>(), It.IsAny<string>()))
                .Returns("test string");
            uncachedToCachedFiles.Setup(d => d.ContainsKey(It.IsAny<string>()))
                .Returns(false);
            uncachedToCachedFiles.Setup(d => d.Add(It.IsAny<string>(), It.IsAny<string>()));
            Assert.Throws<NotImplementedException>(() => diskBackedBundleCache.GetBundle(fileHelper.Object, uncachedToCachedFiles.Object, concatenatedBundle.Path, concatenatedBundle));
        }
        
        #endregion

        #region ContainsKey
        /// Will not check if a bundle is present or not present as that is covered with GetBundle testing.
      
        [Fact]
        public void DiskBackedBundleCache_ContainsKey_PresentInMemory()
        {
            fileHelper.Setup(fh => fh.Exists(It.IsAny<string>()))
                .Returns(true);
            fileHelper.Setup(fh => fh.GetLastAccessTime(It.IsAny<string>()))
                .Returns(DateTime.Today);
            fileHelper.Setup(fh => fh.GetFileName(It.IsAny<IAsset>(), It.IsAny<Bundle>(), It.IsAny<string>()))
                .Returns(fileAsset.SourceFile.FullPath);
            uncachedToCachedFiles.Setup(d => d.ContainsKey(It.IsAny<string>()))
                .Returns(false);
            uncachedToCachedFiles.Setup(d => d.Add(It.IsAny<string>(), It.IsAny<string>()));

            diskBackedBundleCache.ContainsKey(fileHelper.Object, uncachedToCachedFiles.Object, fileBundle.Path, fileBundle);
            
            fileHelper.Setup(fh => fh.Exists(It.IsAny<string>()))
                .Throws(new Exception("Did not find bundle that should have been in memory."));
            fileHelper.Setup(fh => fh.GetLastAccessTime(It.IsAny<string>()))
                .Throws(new Exception("Did not find bundle that should have been in memory."));
            fileHelper.Setup(fh => fh.GetFileName(It.IsAny<IAsset>(), It.IsAny<Bundle>(), It.IsAny<string>()))
                .Throws(new Exception("Did not find bundle that should have been in memory."));

            diskBackedBundleCache.ContainsKey(fileHelper.Object, uncachedToCachedFiles.Object, fileBundle.Path, fileBundle);
        }

        [Fact]
        public void DiskBackedBundleCache_ContainsKey_FailWithConcatenatedBundle()
        {
            /*fileHelper.Setup(fh => fh.GetAssetReferencesFromDisk(It.IsAny<FileAsset>(), It.IsAny<string>()))
                                                                .Verifiable();*/
            fileHelper.Setup(fh => fh.Exists(It.IsAny<string>()))
                .Returns(true);
            fileHelper.Setup(fh => fh.GetLastAccessTime(It.IsAny<string>()))
                .Returns(DateTime.Today);
            fileHelper.Setup(fh => fh.GetFileName(It.IsAny<IAsset>(), It.IsAny<Bundle>(), It.IsAny<string>()))
                .Returns("test string");
            uncachedToCachedFiles.Setup(d => d.ContainsKey(It.IsAny<string>()))
                .Returns(false);
            uncachedToCachedFiles.Setup(d => d.Add(It.IsAny<string>(), It.IsAny<string>()));
            Assert.Throws<NotImplementedException>(() => diskBackedBundleCache.ContainsKey(fileHelper.Object, uncachedToCachedFiles.Object, concatenatedBundle.Path, concatenatedBundle));
        }
        #endregion

        #region FixReferences

        [Fact]
        public void DiskBackedBundleCache_FixReferences_FileAndEmptyBundles()
        {
            uncachedToCachedFiles.Setup(d => d.ContainsKey(It.IsAny<string>()))
                                                                .Returns(true);
            diskBackedBundleCache.AddBundle(fileHelper.Object, uncachedToCachedFiles.Object, fileBundle.Path, fileBundle, unprocessedAssetPaths);
            unprocessedAssetPaths.Add("wasd");
            uncachedToCachedFiles.Setup(d => d.ContainsKey(It.IsAny<string>()))
                .Returns(true);
            diskBackedBundleCache.AddBundle(fileHelper.Object, uncachedToCachedFiles.Object, emptyBundle.Path, emptyBundle, unprocessedAssetPaths);
            diskBackedBundleCache.FixReferences(new List<Bundle> { fileBundle });
            diskBackedBundleCache.FixReferences(new List<Bundle> { emptyBundle });
        }

        #endregion

        #region AddToDisk
        ///just checking unprocessedAssetPath as other code paths checked by AddBundle tests
        
        [Fact]
        public void DiskBackedBundleCache_AddToDisk_UnprocessedAssetPaths()
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

        ///None necessary. All code paths are checked by the GetBundle and ContainsKey test

        #endregion
    }
}
