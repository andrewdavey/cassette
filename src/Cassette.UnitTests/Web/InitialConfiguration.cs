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
using System.IO.IsolatedStorage;
using Cassette.Configuration;
using Should;
using Xunit;

namespace Cassette.Web
{
    public class InitialConfiguration_Tests : IDisposable
    {
        readonly IsolatedStorageFile storage;
        readonly CassetteConfigurationSection section;
        readonly CassetteSettings settings;

        public InitialConfiguration_Tests()
        {
            storage = IsolatedStorageFile.GetUserStoreForAssembly();
            section = new CassetteConfigurationSection();
            settings = new CassetteSettings("");
        }

        [Fact]
        public void GivenSectionDebugNullAndGlobalDebugFalse_WhenConfigure_ThenIsDebuggedEnabledIsFalse()
        {
            section.Debug = null;
            var config = new InitialConfiguration(section, false, "/", "/", storage);
            
            config.Configure(new BundleCollection(settings), settings);

            settings.IsDebuggingEnabled.ShouldBeFalse();
        }

        [Fact]
        public void GivenSectionDebugNullAndGlobalDebugTrue_WhenConfigure_ThenIsDebuggedEnabledIsTrue()
        {
            section.Debug = null;
            var config = new InitialConfiguration(section, true, "/", "/", storage);

            config.Configure(new BundleCollection(settings), settings);

            settings.IsDebuggingEnabled.ShouldBeTrue();
        }

        [Fact]
        public void GivenSectionDebugFalseAndGlobalDebugTrue_WhenConfigure_ThenIsDebuggedEnabledIsFalse()
        {
            section.Debug = false;
            var config = new InitialConfiguration(section, true, "/", "/", storage);

            config.Configure(new BundleCollection(settings), settings);

            settings.IsDebuggingEnabled.ShouldBeFalse();
        }
        
        [Fact]
        public void GivenSectionDebugTrueAndGlobalDebugFalse_WhenConfigure_ThenIsDebuggedEnabledIsTrue()
        {
            section.Debug = true;
            var config = new InitialConfiguration(section, true, "/", "/", storage);

            config.Configure(new BundleCollection(settings), settings);

            settings.IsDebuggingEnabled.ShouldBeTrue();
        }

        public void Dispose()
        {
            storage.Dispose();
        }
    }
}

