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
using System.IO;
using Cassette.Utilities;

namespace Cassette.HtmlTemplates
{
    public class RegisterTemplateWithJQueryTmpl : IAssetTransformer
    {
        readonly HtmlTemplateBundle bundle;

        public RegisterTemplateWithJQueryTmpl(HtmlTemplateBundle bundle)
        {
            this.bundle = bundle;
        }

        public Func<Stream> Transform(Func<Stream> openSourceStream, IAsset asset)
        {
            return delegate
            {
                var id = bundle.GetTemplateId(asset);
                var template = openSourceStream().ReadToEnd();
                return string.Format("$.template('{0}', {1});{2}", id, template, Environment.NewLine).AsStream();
            };
        }
    }
}