﻿using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// See src\SharedAssemblyInfo.cs for assembly metadata shared by all projects.

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("Cassette.Aspnet")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("1038439c-5eae-4483-9051-adca689d2532")]

[assembly: InternalsVisibleTo("Cassette.Aspnet.UnitTests")]
[assembly: InternalsVisibleTo("Cassette.Aspnet.Jasmine")]
[assembly: InternalsVisibleTo("Cassette.IntegrationTests")]

// To allow mocking of internal types, using Moq, the following is required.
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]