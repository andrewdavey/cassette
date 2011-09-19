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
using Cassette.HtmlTemplates;
using Cassette.Scripts;
using Cassette.Stylesheets;

namespace Cassette.UI
{
    public static class Assets
    {
        public static Func<ICassetteApplication> GetApplication;

        public static IReferenceBuilder<ScriptModule> Scripts
        {
            get { return Application.GetReferenceBuilder<ScriptModule>(); }
        }

        public static IReferenceBuilder<StylesheetModule> Stylesheets
        {
            get { return Application.GetReferenceBuilder<StylesheetModule>(); }
        }

        public static IReferenceBuilder<HtmlTemplateModule> HtmlTemplates
        {
            get { return Application.GetReferenceBuilder<HtmlTemplateModule>(); }
        }

        static ICassetteApplication Application
        {
            get
            {
                if (GetApplication == null)
                {
                    // We rely on Cassette.Web (or some other) integration library to hook up its application object.
                    // If the delegate is null then the developer probably forgot to reference the integration library.
                    throw new InvalidOperationException("A Cassette application has not been assigned. Make sure a Cassette integration library has been referenced. For example, reference Cassette.Web.dll");
                }
                return GetApplication();
            }
        }
    }
}

