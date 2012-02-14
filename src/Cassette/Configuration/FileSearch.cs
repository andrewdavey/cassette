using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Cassette.IO;

namespace Cassette.Configuration
{
    public class FileSearch : IFileSearch
    {
        /// <summary>
        /// The file search pattern. For example, "*.js;*.coffee".
        /// </summary>
        public string Pattern { get; set; }
        /// <summary>
        /// Files with full paths matching this regular expression will be excluded.
        /// </summary>
        public Regex Exclude { get; set; }
        /// <summary>
        /// Specifies if all sub-directories are searched recursively, or only the top-level directory is searched.
        /// </summary>
        public SearchOption SearchOption { get; set; }

        /// <summary>
        /// Searches the given directory for files matching the search parameters of this object.
        /// </summary>
        /// <param name="directory">The directory to search.</param>
        /// <returns>A collection of files.</returns>
        public IEnumerable<IFile> FindFiles(IDirectory directory)
        {
            var files =
                from pattern in GetFilePatterns()
                from file in directory.GetFiles(pattern, SearchOption)
                where IsAssetFile(file)
                select file;

            files = files.Distinct(new FilePathComparer());
            return RemoveMinifiedFilesWhereNonMinExist(files);
        }

        IEnumerable<IFile> RemoveMinifiedFilesWhereNonMinExist(IEnumerable<IFile> files)
        {
            var filter = new ConventionalMinifiedFileFilter();
            return filter.Apply(files);
        }

        IEnumerable<string> GetFilePatterns()
        {
            return string.IsNullOrWhiteSpace(Pattern)
                       ? new[] { "*" }
                       : Pattern.Split(new[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries);
        }

        bool IsAssetFile(IFile file)
        {
            return !IsDescriptorFilename(file)
                   && (Exclude == null || !Exclude.IsMatch(file.FullPath));
        }

        static bool IsDescriptorFilename(IFile file)
        {
            // TODO: Remove legacy support for module.txt
            return file.FullPath.EndsWith("/bundle.txt", StringComparison.OrdinalIgnoreCase)
                   || file.FullPath.EndsWith("/module.txt", StringComparison.OrdinalIgnoreCase);
        }
    }
}