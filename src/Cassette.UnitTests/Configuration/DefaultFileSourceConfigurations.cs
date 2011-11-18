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

using System.IO;
using Should;
using Xunit;

namespace Cassette.Configuration
{
    public abstract class FileSourceConfiguration_Tests<T>
        where T : Bundle
    {
        internal readonly FileSearch FileSource;

        protected FileSourceConfiguration_Tests()
        {
            FileSource = GetDefaultInitializer();
        }

        static FileSearch GetDefaultInitializer()
        {
            var settings = new CassetteSettings();
            return settings.DefaultFileSearches[typeof(T)] as FileSearch;
        }
    }

    public class FileSourceConfiguration_ScriptBundle_Tests : FileSourceConfiguration_Tests<Scripts.ScriptBundle>
    {
        [Fact]
        public void FilePatternIsJsAndCoffee()
        {
            FileSource.Pattern.ShouldEqual("*.js;*.coffee");
        }

        [Fact]
        public void ExcludeMatchesVSDocFiles()
        {
            FileSource.Exclude.IsMatch("foo-vsdoc.js").ShouldBeTrue();
            FileSource.Exclude.IsMatch("foo.js").ShouldBeFalse();
        }

        [Fact]
        public void SearchOptionIsAllDirectories()
        {
            FileSource.SearchOption.ShouldEqual(SearchOption.AllDirectories);
        }
    }

    public class FileSourceConfiguration_StylesheetBundle_Tests : FileSourceConfiguration_Tests<Stylesheets.StylesheetBundle>
    {
        [Fact]
        public void FilePatternIsCssAndLess()
        {
            FileSource.Pattern.ShouldEqual("*.css;*.less");
        }

        [Fact]
        public void SearchOptionIsAllDirectories()
        {
            FileSource.SearchOption.ShouldEqual(SearchOption.AllDirectories);
        }
    }

    public class FileSourceConfiguration_HtmlTemplateBundle_Tests : FileSourceConfiguration_Tests<HtmlTemplates.HtmlTemplateBundle>
    {
        [Fact]
        public void FilePatternIsHtmAndHtml()
        {
            FileSource.Pattern.ShouldEqual("*.htm;*.html");
        }

        [Fact]
        public void SearchOptionIsAllDirectories()
        {
            FileSource.SearchOption.ShouldEqual(SearchOption.AllDirectories);
        }
    }
}
