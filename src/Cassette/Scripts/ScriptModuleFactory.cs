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

using System.Linq;
using Cassette.Utilities;

namespace Cassette.Scripts
{
    public class ScriptBundleFactory : IBundleFactory<ScriptBundle>
    {
        public ScriptBundle CreateBundle(string directory)
        {
            return new ScriptBundle(directory);
        }

        public ScriptBundle CreateExternalBundle(string url)
        {
            return new ExternalScriptBundle(url);
        }

        public ScriptBundle CreateExternalBundle(string name, BundleDescriptor bundleDescriptor)
        {
            var bundle = new ExternalScriptBundle(name, bundleDescriptor.ExternalUrl);
            if (bundleDescriptor.FallbackCondition != null)
            {
                var assets = bundleDescriptor.AssetFilenames.Select(
                    filename => new Asset(
                        PathUtilities.CombineWithForwardSlashes(name, filename),
                        bundle,
                        bundleDescriptor.SourceFile.Directory.GetFile(filename)
                    )
                );
                bundle.AddFallback(bundleDescriptor.FallbackCondition, assets);
            }
            return bundle;
        }
    }
}
