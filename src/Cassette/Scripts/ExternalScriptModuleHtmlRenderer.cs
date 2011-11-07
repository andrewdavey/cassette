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

namespace Cassette.Scripts
{
    public class ExternalScriptModuleHtmlRenderer : IModuleHtmlRenderer<ExternalScriptModule>
    {
        readonly IModuleHtmlRenderer<ScriptModule> fallbackScriptRenderer;
        readonly ICassetteApplication application;

        public ExternalScriptModuleHtmlRenderer(IModuleHtmlRenderer<ScriptModule> fallbackScriptRenderer, ICassetteApplication application)
        {
            this.fallbackScriptRenderer = fallbackScriptRenderer;
            this.application = application;
        }

        public string Render(ExternalScriptModule module)
        {
            var externalScriptHtml = string.Format(HtmlConstants.ScriptHtml, module.Url);

            if (string.IsNullOrEmpty(module.FallbackCondition))
            {
                return externalScriptHtml;
            }
            else
            {
                if (application.IsOutputOptimized)
                {
                    return string.Format(
                            "{1}{0}<script type=\"text/javascript\">{0}if({2}){{{0}{3}{0}}}{0}</script>",
                            Environment.NewLine,
                            externalScriptHtml,
                            module.FallbackCondition,
                            CreateFallbackScripts(module)
                            );
                }
                else
                {
                    return fallbackScriptRenderer.Render(module);
                }
            }
        }

        string CreateFallbackScripts(ExternalScriptModule module)
        {
            var scripts = fallbackScriptRenderer.Render(module);
            return ConvertToDocumentWriteCalls(scripts);
        }

        string ConvertToDocumentWriteCalls(string scriptElements)
        {
            var scripts = scriptElements.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            
            return string.Join(
                Environment.NewLine,
                from script in scripts
                select "document.write(unescape('" + Escape(script) + "'));"
            );
        }

        string Escape(string script)
        {
            return script.Replace("<", "%3C").Replace(">", "%3E");
        }
    }
}

