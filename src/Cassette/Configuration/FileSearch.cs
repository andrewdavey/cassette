#region License
/*
Copyright 2011 Andrew Davey

This file is part of Cassette.

Cassette is free software: you can redistribute it and/or modify it under the 
terms of the GNU General Public License as published by the Free Software 
Foundation, either version 3 of the License, or (at your option) any later 
version.

Cassette is distributed in the hope that it will be useful, but WITHOUT ANY 
WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS 
FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.

You should have received a copy of the GNU General Public License along with 
Cassette. If not, see http://www.gnu.org/licenses/.
*/
#endregion

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
            return from pattern in GetFilePatterns()
                   from file in directory.GetFiles(pattern, SearchOption)
                   where IsAssetFile(file)
                   select file;
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
