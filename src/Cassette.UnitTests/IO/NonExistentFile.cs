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

using System.IO;
using Should;
using Xunit;

namespace Cassette.IO
{
    public class NonExistentFile_Tests
    {
        readonly NonExistentFile file = new NonExistentFile("c:\\fail.txt");

        [Fact]
        public void DirectoryPropertyThrowsFileNotFoundException()
        {
            Assert.Throws<FileNotFoundException>(() => file.Directory);
        }

        [Fact]
        public void LastWriteTimeUtcThrowsFileNotFoundException()
        {
            Assert.Throws<FileNotFoundException>(() => file.LastWriteTimeUtc);
        }

        [Fact]
        public void OpenThrowsFileNotFoundException()
        {
            Assert.Throws<FileNotFoundException>(() => file.Open(FileMode.Open, FileAccess.Read, FileShare.ReadWrite));
        }

        [Fact]
        public void ExistsReturnsFalse()
        {
            file.Exists.ShouldEqual(false);
        }

        [Fact]
        public void FullPathReturnsPathPassedToConstructor()
        {
            file.FullPath.ShouldEqual("c:\\fail.txt");
        }
    }
}

