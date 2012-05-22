using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// See src\SharedAssemblyInfo.cs for assembly metadata shared by all projects.

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("Cassette")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("da355e7d-f445-4c8d-856c-c7b6ae19bb7d")]

[assembly: InternalsVisibleTo("Cassette.CoffeeScript")]
[assembly: InternalsVisibleTo("Cassette.Hogan")]
[assembly: InternalsVisibleTo("Cassette.JQueryTmpl")]
[assembly: InternalsVisibleTo("Cassette.KnockoutJQueryTmpl")]
[assembly: InternalsVisibleTo("Cassette.Less")]
[assembly: InternalsVisibleTo("Cassette.Sass")]
[assembly: InternalsVisibleTo("Cassette.Aspnet")]
[assembly: InternalsVisibleTo("Cassette.Views")]
[assembly: InternalsVisibleTo("Cassette.MSBuild")]
[assembly: InternalsVisibleTo("Cassette.Nancy")]

[assembly: InternalsVisibleTo("Cassette.UnitTests")]
[assembly: InternalsVisibleTo("Cassette.MSBuild.UnitTests")]
[assembly: InternalsVisibleTo("Cassette.Views.UnitTests")]
[assembly: InternalsVisibleTo("Cassette.Aspnet.UnitTests")]
[assembly: InternalsVisibleTo("Cassette.Aspnet.Jasmine.UnitTests")]
[assembly: InternalsVisibleTo("Cassette.CoffeeScript.UnitTests")]
[assembly: InternalsVisibleTo("Cassette.Less.UnitTests")]
[assembly: InternalsVisibleTo("Cassette.Sass.UnitTests")]
[assembly: InternalsVisibleTo("Cassette.IntegrationTests")]

// To allow mocking of internal types, using Moq, the following is required.
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]