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
using Cassette.IO;

namespace Cassette
{
    public interface ICassetteApplication : IDisposable
    {
        /// <summary>
        /// The directory containing the original bundle asset files.
        /// </summary>
        IDirectory SourceDirectory { get; }

        /// <summary>
        /// When this property is true, Cassette will output debug-friendly assets. When false, combined, minified bundles are used instead.
        /// </summary>
        bool IsDebuggingEnabled { get; }

        /// <summary>
        /// Gets the <see cref="IUrlGenerator"/> used to generate the URLs of bundle and assets.
        /// </summary>
        IUrlGenerator UrlGenerator { get; }

        /// <summary>
        /// When true (the default), Cassette buffers page output and rewrites to allow bundle references to be inserted into &lt;head&gt;
        /// after it has already been rendered.
        /// </summary>
        bool IsHtmlRewritingEnabled { get; }
    }
}