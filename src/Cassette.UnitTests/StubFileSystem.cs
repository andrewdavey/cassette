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
using Cassette.IO;

namespace Cassette
{
    class StubFileSystem : IDirectory
    {
        private readonly Dictionary<string, Stream> fileStreams;

        public StubFileSystem(Dictionary<string, Stream> fileStreams)
        {
            this.fileStreams = fileStreams;
        }

        public void DeleteContents()
        {
            fileStreams.Clear();
        }

        public IDirectory GetDirectory(string path, bool createIfNotExists)
        {
            return new StubFileSystem(
                fileStreams
                    .Where(f => f.Key.StartsWith(path))
                    .ToDictionary(
                        kvp => kvp.Key.Substring(path.Length == 0 ? 0 : path.Length + 1),
                        kvp => kvp.Value
                    )
                );
        }

        public IEnumerable<string> GetDirectoryPaths(string relativePath)
        {
            throw new NotImplementedException();
        }

        public IFile GetFile(string filename)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<string> GetFilePaths(string directory, SearchOption searchOption, string searchPattern)
        {
            if (searchPattern == "*")
            {
                return fileStreams.Keys;
            }
            else
            {
                return fileStreams.Keys.Where(key => key.EndsWith(searchPattern.Substring(1)));
            }
        }

        public FileAttributes GetAttributes(string path)
        {
            throw new NotImplementedException();
        }
    }
}

