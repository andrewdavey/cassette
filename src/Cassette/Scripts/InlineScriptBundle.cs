using System;
using Cassette.Configuration;

namespace Cassette.Scripts
{
    class InlineScriptBundle : ScriptBundle
    {
        readonly string scriptContent;

        public InlineScriptBundle(string scriptContent)
        {
            this.scriptContent = scriptContent;
        }

        internal override void Process(CassetteSettings settings)
        {
        }

        internal override string Render()
        {
            return "<script type=\"text/javascript\">" + Environment.NewLine + 
                   scriptContent + Environment.NewLine + 
                   "</script>";
        }
    }
}