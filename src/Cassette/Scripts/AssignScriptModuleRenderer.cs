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

using Cassette.ModuleProcessing;

namespace Cassette.Scripts
{
    public class AssignScriptModuleRenderer : IModuleProcessor<ScriptModule>
    {
        public void Process(ScriptModule module, ICassetteApplication application)
        {
            if (application.IsOutputOptimized)
            {
                module.Renderer = new ScriptModuleHtmlRenderer(application.UrlGenerator);
            }
            else
            {
                module.Renderer = new DebugScriptModuleHtmlRenderer(application.UrlGenerator);
            }
        }
    }
}
