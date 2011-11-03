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

using Cassette.ModuleProcessing;

namespace Cassette.Scripts
{
    public class ScriptModule : Module
    {
        public ScriptModule(string directory)
            : base(directory)
        {
            ContentType = "text/javascript";
            Processor = new ScriptPipeline();
        }

        public IModuleProcessor<ScriptModule> Processor { get; set; }

        public IModuleHtmlRenderer<ScriptModule> Renderer { get; set; }

        public override void Process(ICassetteApplication application)
        {
            Processor.Process(this, application);
        }

        public override string Render(ICassetteApplication application)
        {
            return Renderer.Render(this);
        }
    }
}

