﻿#region License
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
    public class FileSystemFile : IFile
    {
        readonly string filename;
        readonly IDirectory directory;

        public FileSystemFile(string filename, IDirectory directory)
        {
            this.filename = filename;
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

        public Stream Open(FileMode mode, FileAccess access, FileShare fileShare)
        {
            return File.Open(filename, mode, access, fileShare);
        }

        public bool Exists
        {
            get { return File.Exists(filename); }
        }

        public DateTime LastWriteTimeUtc
        {
            get { return File.GetLastWriteTimeUtc(filename); }
        }
    }
}

