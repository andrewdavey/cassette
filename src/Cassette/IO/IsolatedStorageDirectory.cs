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
using System.IO.IsolatedStorage;
using System.Linq;

namespace Cassette.IO
{
    /// <remarks>
    /// This class only implements enough of IDirectory to support BundleCache.
    /// Other methods simply throw NotSupportException for now.
    /// </remarks>
    class IsolatedStorageDirectory : IDirectory
    {
        public IsolatedStorageDirectory(IsolatedStorageFile storage)
            : this(storage, "~/")
        {
        }

        IsolatedStorageDirectory(IsolatedStorageFile storage, string basePath)
        {
            this.storage = storage;
            this.basePath = basePath;
        }

        readonly IsolatedStorageFile storage;
        readonly string basePath;

        public string FullPath
        {
            get { return basePath; }
        }

        public IFile GetFile(string filename)
        {
            return new IsolatedStorageFileWrapper(GetAbsolutePath(filename), storage, this);
        }

        string GetAbsolutePath(string path)
        {
            return Path.Combine(basePath, path);
        }

        public bool DirectoryExists(string path)
        {
            throw new NotSupportedException();
        }

        public FileAttributes Attributes
        {
            get { throw new NotSupportedException(); }
        }

        public IDirectory GetDirectory(string path)
        {
            throw new NotSupportedException();
        }

        public IEnumerable<IFile> GetFiles(string searchPattern, SearchOption searchOption)
        {
            return storage.GetFileNames(searchPattern)
                          .Select(filename => new IsolatedStorageFileWrapper(GetAbsolutePath(filename), storage, this));
        }

        public IEnumerable<IDirectory> GetDirectories()
        {
            throw new NotSupportedException();
        }
    }
}