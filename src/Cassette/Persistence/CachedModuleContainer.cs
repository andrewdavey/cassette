using System;
using System.Collections.Generic;
using System.Linq;

namespace Cassette.Persistence
{
    class CachedModuleContainer<T> : ModuleContainer<T>
        where T : Module
    {
        public CachedModuleContainer(IEnumerable<T> modules) : base(modules)
        {
        }

        public bool IsUpToDate(int expectedAssetCount, DateTime dateTime, IFileSystem sourceDirectory)
        {
            if (expectedAssetCount != Modules.SelectMany(m => m.Assets).Count()) return false;

            var maxLastWriteTimeCollector = new MaxLastWriteCollector(sourceDirectory);
            foreach (var module in Modules)
            {
                module.Accept(maxLastWriteTimeCollector);
            }
            return maxLastWriteTimeCollector.Max <= dateTime;
        }

        class MaxLastWriteCollector : IAssetVisitor
        {
            readonly IFileSystem rootDirectory;
            IFileSystem currentDirectory;

            public MaxLastWriteCollector(IFileSystem rootDirectory)
            {
                this.rootDirectory = rootDirectory;
            }

            DateTime max = DateTime.MinValue;

            public DateTime Max
            {
                get { return max; }
            }

            public bool FilesMissing { get; set; }

            public void Visit(Module module)
            {
                if (module.Assets.Count == 0) return; // Skip external modules

                if (rootDirectory.DirectoryExists(module.Path))
                {
                    currentDirectory = rootDirectory.NavigateTo(module.Path, false);
                }
                else
                {
                    FilesMissing = true;
                }
            }

            public void Visit(IAsset asset)
            {
                if (FilesMissing) return;

                if (currentDirectory.FileExists(asset.SourceFilename))
                {
                    var writeTime = currentDirectory.GetLastWriteTimeUtc(asset.SourceFilename);
                    if (writeTime > max)
                    {
                        max = writeTime;
                    }
                }
                else
                {
                    FilesMissing = true;
                }
            }
        }
    }
}
