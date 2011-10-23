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
using System.Linq;

namespace Cassette.BundleProcessing
{
    public abstract class BundleProcessorOfAssetsMatchingFileExtension<T> : IBundleProcessor<T>
        where T : Bundle
    {
        protected BundleProcessorOfAssetsMatchingFileExtension(string fileExtension)
        {
            filenameEndsWith = "." + fileExtension;
        }

        readonly string filenameEndsWith;

        public void Process(T bundle, ICassetteApplication application)
        {
            var assets = bundle.Assets.Where(ShouldProcessAsset);
            foreach (var asset in assets)
            {
                Process(asset, bundle);
            }
        }

        protected abstract void Process(IAsset asset, Bundle bundle);

        bool ShouldProcessAsset(IAsset asset)
        {
            return asset.SourceFile.FullPath.EndsWith(filenameEndsWith, StringComparison.OrdinalIgnoreCase);
        }
    }
}