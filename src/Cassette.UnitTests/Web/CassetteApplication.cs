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

using System.Collections.Generic;
using System.Web;
using System.Web.Routing;
using Cassette.HtmlTemplates;
using Cassette.IO;
using Cassette.Scripts;
using Cassette.Stylesheets;
using Cassette.UI;
using Moq;
using Should;
using Xunit;

namespace Cassette.Web
{
    public class CassetteApplication_OnPostMapRequestHandler_Tests
    {
        [Fact]
        public void GivenHtmlRewritingEnabled_WhenOnPostMapRequestHandler_ThenPlaceholderTrackerAddedToContextItems()
        {
            var application = StubApplication();
            application.HtmlRewritingEnabled = true;

            var context = new Mock<HttpContextBase>();
            var items = new Dictionary<string, object>();
            context.SetupGet(c => c.Items).Returns(items);

            application.OnPostMapRequestHandler(context.Object);

            items[typeof(IPlaceholderTracker).FullName].ShouldBeType<PlaceholderTracker>();
        }

        [Fact]
        public void GivenHtmlRewritingDisabled_WhenOnPostMapRequestHandler_ThenNullPlaceholderTrackerAddedToContextItems()
        {
            var application = StubApplication();
            application.HtmlRewritingEnabled = false;

            var context = new Mock<HttpContextBase>();
            var items = new Dictionary<string, object>();
            context.SetupGet(c => c.Items).Returns(items);

            application.OnPostMapRequestHandler(context.Object);

            items[typeof(IPlaceholderTracker).FullName].ShouldBeType<NullPlaceholderTracker>();
        }

        static CassetteApplication StubApplication()
        {
            return new CassetteApplication(
                new[] { new StubConfig() },
                Mock.Of<IDirectory>(),
                Mock.Of<IDirectory>(),
                false,
                "",
                new UrlGenerator(""),
                new RouteCollection(),
                () => null
            );
        }

        class StubConfig : ICassetteConfiguration
        {
            public void Configure(ModuleConfiguration moduleConfiguration, ICassetteApplication application)
            {
                moduleConfiguration.Add(Mock.Of<IModuleSource<ScriptModule>>());
                moduleConfiguration.Add(Mock.Of<IModuleSource<StylesheetModule>>());
                moduleConfiguration.Add(Mock.Of<IModuleSource<HtmlTemplateModule>>());
            }
        }
    }
}

