using Should;
using Xunit;

namespace Cassette.RequireJS.Tests
{
    public class AmdModuleRewriterTests
    {
        [Fact]
        public void ModulePathArgumentIsInsertedIntoDefineCall()
        {
            var rewriter = new AmdModuleRewriter();
            var output = rewriter.InsertModulePathIntoDefineCall("define([],function(){})", "example");
            output.ShouldEqual("define(\"example\",[],function(){})");
        }

        [Fact]
        public void OnlyDefineFunctionCallIsRewritten()
        {
            var rewriter = new AmdModuleRewriter();
            var output = rewriter.InsertModulePathIntoDefineCall("other([],function(){})", "example");
            output.ShouldEqual("other([],function(){})");
        }

        [Fact]
        public void NonTopLevelDefineCallIsRewritten()
        {
            var rewriter = new AmdModuleRewriter();
            var output = rewriter.InsertModulePathIntoDefineCall("(function(){define([],function(){})})()", "example");
            output.ShouldEqual("(function(){define(\"example\",[],function(){})})()");
        }

        [Fact]
        public void DontInsertModulePathWhenItsAlreadyPresent()
        {
            var rewriter = new AmdModuleRewriter();
            var output = rewriter.InsertModulePathIntoDefineCall("define(\"defined/path\",[],function(){})", "example");
            output.ShouldEqual("define(\"defined/path\",[],function(){})");
        }
    }
}