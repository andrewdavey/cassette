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
using Cassette.Utilities;

namespace Cassette.IO
{
    public class FileSystemFile : IFile
    {
        readonly string name;
        readonly string systemAbsoluteFilename;
        readonly IDirectory directory;

        public FileSystemFile(string name, IDirectory directory, string systemAbsoluteFilename)
        {
            this.name = name;
            this.directory = directory;
            this.systemAbsoluteFilename = systemAbsoluteFilename;
        }

        public IDirectory Directory
        {
            get { return directory; }
        }

        public string FullPath
        {
            get { return PathUtilities.CombineWithForwardSlashes(directory.FullPath, name); }
        }

        public Stream Open(FileMode mode, FileAccess access, FileShare fileShare)
        {
            return File.Open(systemAbsoluteFilename, mode, access, fileShare);
        }

        public bool Exists
        {
            get { return File.Exists(systemAbsoluteFilename); }
        }

        public DateTime LastWriteTimeUtc
        {
            get { return File.GetLastWriteTimeUtc(systemAbsoluteFilename); }
        }

        public void Delete()
        {
            File.Delete(systemAbsoluteFilename);
        }
    }
}

