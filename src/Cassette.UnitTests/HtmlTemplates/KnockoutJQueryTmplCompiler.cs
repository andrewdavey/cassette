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

using Cassette.IO;
using Moq;
using Should;
using Xunit;

namespace Cassette.HtmlTemplates
{
    public class KnockoutJQueryTmplCompiler_Tests
    {
        [Fact]
        public void WhenCompileHtmlWithSingleQuotesInDataBindAttribute_ThenSingleQuotesAreNotEscaped()
        {
            var compiler = new KnockoutJQueryTmplCompiler();
            var output = compiler.Compile("<div data-bind=\"test: 'a'\"></div>", Mock.Of<IFile>());
            output.ShouldEqual(@"function(jQuery, $item) {var $=jQuery,call,__=[],$data=$item.data;with($data){__.push('');__.push(((function() { return ko.templateRewriting.applyMemoizedBindingsToNextSibling(function() {                     return (function() { return { test: 'a' } })()                 }) })()) || '');__.push('<div ></div>');}return __;}");
        }
    }
}
