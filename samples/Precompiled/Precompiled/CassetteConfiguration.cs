using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Cassette.Configuration;
using Cassette.Scripts;
using Cassette.Stylesheets;

namespace Precompiled
{
    public class CassetteConfiguration : ICassetteConfiguration
    {
        public void Configure(BundleCollection bundles, CassetteSettings settings)
        {
            bundles.Add<StylesheetBundle>("Content");
            bundles.Add<ScriptBundle>("Scripts");
        }
    }
}