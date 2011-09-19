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
using Cassette.HtmlTemplates;
using Cassette.Scripts;
using Cassette.Stylesheets;
using Moq;
using Should;
using Xunit;

namespace Cassette.UI
{
    public class Assets_Test
    {
        [Fact]
        public void ScriptsReturnsPageAssetManager()
        {
            var application = new Mock<ICassetteApplication>();
            Assets.GetApplication = () => application.Object;
            var manager = new Mock<IReferenceBuilder<ScriptModule>>();
            application.Setup(a => a.GetReferenceBuilder<ScriptModule>())
                       .Returns(manager.Object);

            Assets.Scripts.ShouldEqual(manager.Object);
        }

        [Fact]
        public void StylesheetsReturnsPageAssetManager()
        {
            var application = new Mock<ICassetteApplication>();
            Assets.GetApplication = () => application.Object;
            var manager = new Mock<IReferenceBuilder<StylesheetModule>>();
            application.Setup(a => a.GetReferenceBuilder<StylesheetModule>())
                       .Returns(manager.Object);

            Assets.Stylesheets.ShouldEqual(manager.Object);
        }

        [Fact]
        public void HtmlTemplatesReturnsPageAssetManager()
        {
            var application = new Mock<ICassetteApplication>();
            Assets.GetApplication = () => application.Object;
            var manager = new Mock<IReferenceBuilder<HtmlTemplateModule>>();
            application.Setup(a => a.GetReferenceBuilder<HtmlTemplateModule>())
                       .Returns(manager.Object);

            Assets.HtmlTemplates.ShouldEqual(manager.Object);
        }

        [Fact]
        public void GivenGetApplicationIsNull_WhenGetScripts_ThenThrowInvalidOperationException()
        {
            Assets.GetApplication = null;
            Assert.Throws<InvalidOperationException>(delegate
            {
                var _ = Assets.Scripts;
            });
        }

        [Fact]
        public void GivenGetApplicationIsNull_WhenGetStylesheets_ThenThrowInvalidOperationException()
        {
            Assets.GetApplication = null;
            Assert.Throws<InvalidOperationException>(delegate
            {
                var _ = Assets.Stylesheets;
            });
        }

        [Fact]
        public void GivenGetApplicationIsNull_WhenGetHtmlTemplates_ThenThrowInvalidOperationException()
        {
            Assets.GetApplication = null;
            Assert.Throws<InvalidOperationException>(delegate
            {
                var _ = Assets.HtmlTemplates;
            });
        }
    }
}

