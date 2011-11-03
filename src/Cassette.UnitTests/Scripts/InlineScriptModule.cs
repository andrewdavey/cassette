﻿#region License
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
using Moq;
using Should;
using Xunit;

namespace Cassette.Scripts
{
    public class InlineScriptModule_Tests
    {
        [Fact]
        public void GivenInlineScriptModuleWithContent_WhenRender_ThenScriptElementCreatedWithContent()
        {
            var module = new InlineScriptModule("var x = 1;");
            var html = module.Render(Mock.Of<ICassetteApplication>());
            html.ShouldEqual(
                "<script type=\"text/javascript\">" + Environment.NewLine + 
                "var x = 1;" + Environment.NewLine + 
                "</script>"
            );
        }
    }
}

