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
            
            return RemoveMinifiedFilesWhereNonMinExist(files);
        }

        IEnumerable<IFile> RemoveMinifiedFilesWhereNonMinExist(IEnumerable<IFile> files)
        {
            var filenameRegex = new Regex(@"^(?<name>.*?)(?<type>(\.|-)min|(\.|-)debug|)(?<extension>\.(js|css))$", RegexOptions.IgnoreCase);
            var items = (
                from file in files
                let name = file.FullPath.Split('/', '\\').Last()
                select new
                {
                    Match = filenameRegex.Match(name),
                    File = file,
                    Name = name
                }
            ).ToArray();

            // Any non-matching filenames don't receive special filtering, so just return them all.
            foreach (var item in items)
            {
                if (!item.Match.Success) yield return item.File;
            }

            // Create set of filenames for quick look-up.
            var filenames = new HashSet<string>(
                from item in items
                where item.Match.Success
                select item.Name,
                StringComparer.OrdinalIgnoreCase
            );

            foreach (var item in items)
            {
                var name = item.Match.Groups["name"].Value;
                var type = item.Match.Groups["type"].Value.TrimStart('.', '-');
                var extension = item.Match.Groups["extension"].Value;

                if (type.Equals("min", StringComparison.OrdinalIgnoreCase))
                {
                    var nonMinFileExists = filenames.Contains(name + extension);
                    if (!nonMinFileExists)
                    {
                        yield return item.File;
                    }
                }
                else if (type.Equals("debug", StringComparison.OrdinalIgnoreCase))
                {
                    yield return item.File;
                }
                else
                {
                    var debugFileExists = filenames.Contains(name + ".debug" + extension) 
                                       || filenames.Contains(name + "-debug" + extension);
                    if (!debugFileExists)
                    {
                        yield return item.File;
                    }
                }
            }
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
