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
using Cassette.BundleProcessing;

namespace Cassette.Scripts
{
    public class ScriptPipeline : MutablePipeline<ScriptBundle>
    {
        public ScriptPipeline()
        {
            Minifier = new MicrosoftJavaScriptMinifier();
            CompileCoffeeScript = true;
        }

        public bool CompileCoffeeScript { get; set; }
        public IAssetTransformer Minifier { get; set; }

        protected override IEnumerable<IBundleProcessor<ScriptBundle>> CreatePipeline(ScriptBundle bundle, ICassetteApplication application)
        {
            yield return new ParseJavaScriptReferences();
            if (CompileCoffeeScript)
            {
                yield return new ParseCoffeeScriptReferences();
                yield return new CompileCoffeeScript(new CoffeeScriptCompiler());
            }
            yield return new SortAssetsByDependency();
            if (!application.IsDebuggingEnabled)
            {
                yield return new ConcatenateAssets();
                yield return new MinifyAssets(Minifier);
            }
            yield return new AssignScriptRenderer();
        }
    }
}
