using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Cassette.Caching;
using Cassette.IO;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Cassette.MSBuild
{
    public class MSBuildHost : HostBase
    {
        readonly string sourceDirectory;
        readonly string binDirectory;
        readonly string outputDirectory;
        readonly string appVirtualPath;
        readonly bool includeRawFiles;
        readonly TaskLoggingHelper taskLoggingHelper;

        public MSBuildHost(string sourceDirectory, string binDirectory, string outputDirectory, string appVirtualPath, bool includeRawFiles, TaskLoggingHelper taskLoggingHelper)
        {
            if (!Path.IsPathRooted(sourceDirectory)) throw new ArgumentException("sourceDirectory must be an absolute path.", "sourceDirectory");
            if (!Path.IsPathRooted(binDirectory)) throw new ArgumentException("binDirectory must be an absolute path.", "binDirectory");
            if (!Path.IsPathRooted(outputDirectory)) throw new ArgumentException("outputDirectory must be an absolute path.", "outputDirectory");

            this.sourceDirectory = sourceDirectory;
            this.binDirectory = binDirectory;
            this.outputDirectory = outputDirectory;
            this.appVirtualPath = appVirtualPath;
            this.includeRawFiles = includeRawFiles;
            this.taskLoggingHelper = taskLoggingHelper;
        }

        protected override void ConfigureContainer()
        {
            Container.Register<IUrlModifier>(new PathPrepender(appVirtualPath));
            base.ConfigureContainer();
        }

        protected override IEnumerable<Assembly> LoadAssemblies()
        {
            return Directory
                .GetFiles(binDirectory, "*.dll")
                .Select(TryLoadAssembly)
                .Where(assembly => assembly != null);
        }

        Assembly TryLoadAssembly(string assemblyFilename)
        {
            try
            {
                return Assembly.LoadFrom(assemblyFilename);
            }
            catch
            {
                return null;
            }
        }

        protected override bool CanCreateRequestLifetimeProvider
        {
            get { return false; }
        }

        protected override void RegisterBundleCollectionInitializer()
        {
            Container.Register<IBundleCollectionInitializer, BundleCollectionInitializer>();
            Container.Register<IBundleCollectionCache>((c, p) =>
            {
                var cacheDirectory = new FileSystemDirectory(Path.GetFullPath(outputDirectory));
                return new BundleCollectionCache(
                    cacheDirectory,
                    bundleTypeName => ResolveBundleDeserializer(bundleTypeName, c)
                );
            });
        }

        public void Execute()
        {
            taskLoggingHelper.LogMessage(MessageImportance.High, "Starting bundling");

            var stopwatch = new Stopwatch();
            stopwatch.Start();
            try
            {
                var bundles = Container.Resolve<BundleCollection>();

                WriteCache(bundles);
                if (includeRawFiles)
                {
                    CopyRawFileToOutputDirectory(bundles);
                }
            }
            catch (Exception exception)
            {
                taskLoggingHelper.LogError(exception.ToString());
                throw;
            }
            finally
            {
                stopwatch.Stop();
                taskLoggingHelper.LogMessage(MessageImportance.High, "Finished bundling - (took {0}ms)", stopwatch.ElapsedMilliseconds);
            }
        }

        void WriteCache(IEnumerable<Bundle> bundles)
        {
            var cache = Container.Resolve<IBundleCollectionCache>();
            cache.Write(Manifest.Static(bundles));
        }

        void CopyRawFileToOutputDirectory(BundleCollection bundles)
        {
            bundles.Accept(new RawFileCopier(sourceDirectory, outputDirectory));
        }

        protected override IConfiguration<CassetteSettings> CreateHostSpecificSettingsConfiguration()
        {
            return new MsBuildHostSettingsConfiguration(sourceDirectory);
        }
    }
}