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
using Cassette.Scripts;

namespace Cassette.UI
{
    public static class ScriptModuleExtensions
    {
        public static void AddInline(this IReferenceBuilder<ScriptModule> referenceBuilder, string scriptContent, string location = null)
        {
            referenceBuilder.Reference(new InlineScriptModule(scriptContent), location);
        }

        public static void AddPageData(this IReferenceBuilder<ScriptModule> referenceBuilder, string globalVariable, object data, string location = null)
        {
            referenceBuilder.Reference(new PageDataScriptModule(globalVariable, data), location);
        }

        public static void AddPageData(this IReferenceBuilder<ScriptModule> referenceBuilder, string globalVariable, IDictionary<string, object> data, string location = null)
        {
            referenceBuilder.Reference(new PageDataScriptModule(globalVariable, data), location);
        }
    }
}
