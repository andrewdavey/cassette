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
using Cassette.ModuleProcessing;

namespace Cassette.HtmlTemplates
{
    public class JQueryTmplPipeline : MutablePipeline<HtmlTemplateModule>
    {
        protected override IEnumerable<IModuleProcessor<HtmlTemplateModule>> CreatePipeline(HtmlTemplateModule module, ICassetteApplication application)
        {
            yield return new AddTransformerToAssets(
                new CompileAsset(new JQueryTmplCompiler())
            );
            yield return new Customize<HtmlTemplateModule>(
                m => m.ContentType = "text/javascript"
            );
            yield return new AddTransformerToAssets(
                new WrapJQueryTemplateInInitializer(module)
            );
            yield return new ConcatenateAssets();
            yield return new AssignRenderer(
                new RemoteHtmlTemplateModuleRenderer(application.UrlGenerator)
            );
        }
    }
}
