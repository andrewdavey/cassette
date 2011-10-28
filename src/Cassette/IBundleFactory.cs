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

using System.Collections.Generic;
using System.Linq;
using Cassette.IO;

namespace Cassette
{
    interface IBundleFactory<out T>
        where T : Bundle
    {
        T CreateBundle(string path, IEnumerable<IFile> allFiles, BundleDescriptor bundleDescriptor);
    }

    static class BundleFactoryExtensions
    {
        public static T CreateExternalBundle<T>(this IBundleFactory<T> bundleFactory, string url)
            where T : Bundle
        {
            return bundleFactory.CreateBundle(url, Enumerable.Empty<IFile>(), new BundleDescriptor
            {
                ExternalUrl = url
            });
        }
    }
}