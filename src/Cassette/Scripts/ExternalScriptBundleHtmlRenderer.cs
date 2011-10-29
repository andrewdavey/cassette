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
using System.Web;

namespace Cassette.Scripts
{
    public class ExternalScriptBundleHtmlRenderer : IBundleHtmlRenderer<ExternalScriptBundle>
    {
        readonly IBundleHtmlRenderer<ScriptBundle> fallbackScriptRenderer;
        readonly ICassetteApplication application;

        public ExternalScriptBundleHtmlRenderer(IBundleHtmlRenderer<ScriptBundle> fallbackScriptRenderer, ICassetteApplication application)
        {
            this.fallbackScriptRenderer = fallbackScriptRenderer;
            this.application = application;
        }

        public IHtmlString Render(ExternalScriptBundle bundle)
        {
            var externalScriptHtml = string.Format(HtmlConstants.ScriptHtml, bundle.Url);

            if (application.IsDebuggingEnabled)
            {
                if (bundle.Assets.Any())
                {
                    return fallbackScriptRenderer.Render(bundle);                    
                }
                else
                {
                    return new HtmlString(externalScriptHtml);
                }
            }
            else
            {
                if (bundle.Assets.Any())
                {
                    return new HtmlString(
                        string.Format(
                            "{1}{0}<script type=\"text/javascript\">{0}if({2}){{{0}{3}{0}}}{0}</script>",
                            Environment.NewLine,
                            externalScriptHtml,
                            bundle.FallbackCondition,
                            CreateFallbackScripts(bundle)
                        )
                    );
                }
                else
                {
                    return new HtmlString(externalScriptHtml);                    
                }
            }
        }

        string CreateFallbackScripts(ExternalScriptBundle bundle)
        {
            var scripts = fallbackScriptRenderer.Render(bundle).ToHtmlString();
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

