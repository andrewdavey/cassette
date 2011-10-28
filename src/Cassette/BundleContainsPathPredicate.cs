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
using Cassette.Utilities;

namespace Cassette
{
    class BundleContainsPathPredicate : IBundleVisitor
    {
        public bool BundleContainsPath(string path, Bundle bundle)
        {
            pathToFind = path.IsUrl() ? path : NormalizePath(path, bundle);
            bundle.Accept(this);
            return isFound;
        }

        string pathToFind;
        bool isFound;

        void IBundleVisitor.Visit(Bundle bundle)
        {
            if (IsMatch(bundle.Path))
            {
                isFound = true;
            }
        }

        void IBundleVisitor.Visit(IAsset asset)
        {
            if (IsMatch(asset.SourceFile.FullPath))
            {
                isFound = true;
            }
        }

        string NormalizePath(string path, Bundle bundle)
        {
            path = path.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            if (path.StartsWith("~"))
            {
                return path;
            }
            else
            {
                return PathUtilities.CombineWithForwardSlashes(bundle.Path, path);
            }
        }

        bool IsMatch(string path)
        {
            return PathUtilities.PathsEqual(path, pathToFind);
        }
    }
}

