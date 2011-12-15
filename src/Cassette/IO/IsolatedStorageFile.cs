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

namespace Cassette.IO
{
    public class IsolatedStorageFile : IFile
    {
        readonly string filename;
        readonly System.IO.IsolatedStorage.IsolatedStorageFile storage;
        readonly IsolatedStorageDirectory directory;
        readonly string systemFilename;

        public IsolatedStorageFile(string filename, System.IO.IsolatedStorage.IsolatedStorageFile storage, IsolatedStorageDirectory directory)
        {
            this.filename = filename;
            this.storage = storage;
            this.directory = directory;
            systemFilename = filename.Substring(2); // Skip the "~/" prefix.
        }

        public IDirectory Directory
        {
            get { return directory; }
        }

        public string FullPath
        {
            get { return filename; }
        }

        public Stream Open(FileMode mode, FileAccess access, FileShare fileShare)
        {
            return storage.OpenFile(systemFilename, mode, access, fileShare);
        }

        public bool Exists
        {
            get { return storage.FileExists(systemFilename); }
        }

        public DateTime LastWriteTimeUtc
        {
            get { return storage.GetLastWriteTime(systemFilename).UtcDateTime; }
        }

        public void Delete()
        {
            storage.DeleteFile(systemFilename);
        }
    }
}

