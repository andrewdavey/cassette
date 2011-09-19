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
using System.IO;
using System.IO.IsolatedStorage;

namespace Cassette.IO
{
    public class IsolatedStorageFileWrapper : IFile
    {
        readonly string filename;
        readonly IsolatedStorageFile storage;
        readonly IsolatedStorageDirectory directory;

        public IsolatedStorageFileWrapper(string filename, IsolatedStorageFile storage, IsolatedStorageDirectory directory)
        {
            this.filename = filename;
            this.storage = storage;
            this.directory = directory;
        }

        public IDirectory Directory
        {
            get { return directory; }
        }

        public string FullPath
        {
            get { return filename; }
        }

        public Stream Open(FileMode mode, FileAccess access)
        {
            return storage.OpenFile(filename, mode, access);
        }

        public bool Exists
        {
            get { return storage.FileExists(filename); }
        }

        public DateTime LastWriteTimeUtc
        {
            get { return storage.GetLastWriteTime(filename).UtcDateTime; }
        }
    }
}

