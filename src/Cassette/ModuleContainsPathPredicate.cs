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
    public class ModuleContainsPathPredicate : IAssetVisitor
    {
        public bool ModuleContainsPath(string path, Module module)
        {
            pathToFind = path.IsUrl() ? path : NormalizePath(path, module);
            module.Accept(this);
            return isFound;
        }

        string pathToFind;
        bool isFound;

        void IAssetVisitor.Visit(Module module)
        {
            if (IsMatch(module.Path))
            {
                isFound = true;
            }
        }

        void IAssetVisitor.Visit(IAsset asset)
        {
            if (IsMatch(asset.SourceFilename))
            {
                isFound = true;
            }
        }

        string NormalizePath(string path, Module module)
        {
            path = path.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            if (path.StartsWith("~"))
            {
                return path;
            }
            else
            {
                return PathUtilities.CombineWithForwardSlashes(module.Path, path);
            }
        }

        bool IsMatch(string path)
        {
            return PathUtilities.PathsEqual(path, pathToFind);
        }
    }
}

