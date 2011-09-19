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

namespace Cassette.ModuleProcessing
{
    public abstract class ModuleProcessorOfAssetsMatchingFileExtension<T> : IModuleProcessor<T>
        where T : Module
    {
        protected ModuleProcessorOfAssetsMatchingFileExtension(string fileExtension)
        {
            filenameEndsWith = "." + fileExtension;
        }

        readonly string filenameEndsWith;

        public void Process(T module, ICassetteApplication application)
        {
            var assets = module.Assets.Where(ShouldProcessAsset);
            foreach (var asset in assets)
            {
                Process(asset, module);
            }
        }

        protected virtual bool ShouldProcessAsset(IAsset asset)
        {
            return asset.SourceFilename.EndsWith(filenameEndsWith, StringComparison.OrdinalIgnoreCase);
        }

        protected abstract void Process(IAsset asset, Module module);
    }
}

