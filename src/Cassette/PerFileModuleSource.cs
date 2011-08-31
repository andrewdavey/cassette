using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text.RegularExpressions;

namespace Cassette
{
    public class PerFileModuleSource<T> : IModuleSource<T>
        where T : Module
    {
        readonly string basePath;

        public PerFileModuleSource(string basePath)
        {
            this.basePath = basePath;
        }

        public string FilePattern { get; set; }

        public Regex Exclude { get; set; }

        public IEnumerable<T> GetModules(IModuleFactory<T> moduleFactory, ICassetteApplication application)
        {
            var directory = application.RootDirectory.NavigateTo(basePath, false);
            var filenames = GetFilenames(directory);

            return from filename in filenames
                   select CreateModule(filename, moduleFactory, directory);
        }

        IEnumerable<string> GetFilenames(IFileSystem directory)
        {
            IEnumerable<string> filenames;
            if (string.IsNullOrEmpty(FilePattern))
            {
                filenames = directory.GetFiles("");
            }
            else
            {
                var patterns = FilePattern.Split(';', ',');
                filenames = patterns.SelectMany(
                    pattern => directory.GetFiles("", pattern)
                );
            }

            if (Exclude != null)
            {
                filenames = filenames.Where(f => Exclude.IsMatch(f) == false);
            }
            return filenames;
        }

        T CreateModule(string filename, IModuleFactory<T> moduleFactory, IFileSystem directory)
        {
            var module = moduleFactory.CreateModule(Path.Combine(basePath, filename));
            module.AddAssets(new[] { new Asset("", module, directory.GetFile(filename)) }, true);
            return module;
        }
    }
}
