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
            FileSearch = GetDefaultInitializer();
        }

        static FileSearch GetDefaultInitializer()
        {
            var settings = new CassetteSettings("");
            return settings.DefaultFileSearches[typeof(T)] as FileSearch;
        }
    }

    public class FileSourceConfiguration_ScriptBundle_Tests : FileSearch_Tests<Scripts.ScriptBundle>
    {
        [Fact]
        public void FilePatternIsJsAndCoffee()
        {
            FileSearch.Pattern.ShouldEqual("*.js;*.coffee");
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
        public void FilePatternIsCssAndLess()
        {
            FileSearch.Pattern.ShouldEqual("*.css;*.less;*.scss;*.sass");
        }

        [Fact]
        public void SearchOptionIsAllDirectories()
        {
            FileSearch.SearchOption.ShouldEqual(SearchOption.AllDirectories);
        }
    }

    public class FileSourceConfiguration_HtmlTemplateBundle_Tests : FileSearch_Tests<HtmlTemplates.HtmlTemplateBundle>
    {
        [Fact]
        public void FilePatternIsHtmHtmlJstTmplMustache()
        {
            FileSearch.Pattern.ShouldEqual("*.htm;*.html;*.jst;*.tmpl;*.mustache");
        }

        [Fact]
        public void SearchOptionIsAllDirectories()
        {
            FileSearch.SearchOption.ShouldEqual(SearchOption.AllDirectories);
        }
    }
}
