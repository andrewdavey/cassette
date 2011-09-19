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
    public class ScriptModuleFactory : IModuleFactory<ScriptModule>
    {
        public ScriptModule CreateModule(string directory)
        {
            return new ScriptModule(directory);
        }

        public ScriptModule CreateExternalModule(string url)
        {
            return new ExternalScriptModule(url);
        }

        public ScriptModule CreateExternalModule(string name, ModuleDescriptor moduleDescriptor)
        {
            var module = new ExternalScriptModule(name, moduleDescriptor.ExternalUrl);
            if (moduleDescriptor.FallbackCondition != null)
            {
                var assets = moduleDescriptor.AssetFilenames.Select(
                    filename => new Asset(
                        PathUtilities.CombineWithForwardSlashes(name, filename),
                        module,
                        moduleDescriptor.SourceFile.Directory.GetFile(filename)
                    )
                );
                module.AddFallback(moduleDescriptor.FallbackCondition, assets);
            }
            return module;
        }
    }
}
