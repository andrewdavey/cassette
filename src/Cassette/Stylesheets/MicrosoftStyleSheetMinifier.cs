﻿using System;
using System.IO;
using Cassette.Utilities;
using Microsoft.Ajax.Utilities;

namespace Cassette.Stylesheets
{
    public class MicrosoftStylesheetMinifier : IStylesheetMinifier
    {
        public MicrosoftStylesheetMinifier()
            : this(new CssSettings())
        {
        }

        public MicrosoftStylesheetMinifier(CssSettings cssSettings)
        {
            this.cssSettings = cssSettings;
        }

        readonly CssSettings cssSettings;

        public Func<Stream> Transform(Func<Stream> openSourceStream, IAsset asset)
        {
            return delegate
            {
                using (var reader = new StreamReader(openSourceStream()))
                {
                    var output = new Minifier().MinifyStyleSheet(reader.ReadToEnd(), cssSettings);
                    return output.AsStream();
                }
            };
        }
    }
}
