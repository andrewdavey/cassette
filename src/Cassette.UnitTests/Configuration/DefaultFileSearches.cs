using System.IO;
using Should;
using Xunit;

namespace Cassette.Configuration
{
    public abstract class FileSearch_Tests<T>
        where T : Bundle
    {
        internal readonly FileSearch FileSearch;

        protected FileSearch_Tests()
        {
            FileSearch = GetDefaultFileSearch();
        }

        static FileSearch GetDefaultFileSearch()
        {
            var settings = new CassetteSettings("");
            return settings.GetDefaultFileSearch<T>() as FileSearch;
        }
    }

    public class FileSearchConfiguration_ScriptBundle_Tests : FileSearch_Tests<Scripts.ScriptBundle>
    {
        [Fact]
        public void FilePatternIsJs()
        {
            FileSearch.Pattern.ShouldEqual("*.js");
        }

        [Fact]
        public void ExcludeMatchesVSDocFiles()
        {
            FileSearch.Exclude.IsMatch("foo-vsdoc.js").ShouldBeTrue();
            FileSearch.Exclude.IsMatch("foo.js").ShouldBeFalse();
        }

        [Fact]
        public void SearchOptionIsAllDirectories()
        {
            FileSearch.SearchOption.ShouldEqual(SearchOption.AllDirectories);
        }
    }

    public class FileSourceConfiguration_StylesheetBundle_Tests : FileSearch_Tests<Stylesheets.StylesheetBundle>
    {
        [Fact]
        public void FilePatternIsCss()
        {
            FileSearch.Pattern.ShouldEqual("*.css");
        }

        [Fact]
        public void SearchOptionIsAllDirectories()
        {
            FileSearch.SearchOption.ShouldEqual(SearchOption.AllDirectories);
        }
    }

    public class FileSearchConfiguration_HtmlTemplateBundle_Tests : FileSearch_Tests<HtmlTemplates.HtmlTemplateBundle>
    {
        [Fact]
        public void FilePatternIsHtmAndHtml()
        {
            FileSearch.Pattern.ShouldEqual("*.htm;*.html");
        }

        [Fact]
        public void SearchOptionIsAllDirectories()
        {
            FileSearch.SearchOption.ShouldEqual(SearchOption.AllDirectories);
        }
    }
}