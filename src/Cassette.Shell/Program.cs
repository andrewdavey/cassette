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
using System.IO;
using System.Linq;
using Cassette.Assets.Scripts;
using Cassette.CoffeeScript;
using Cassette.ModuleBuilding;

namespace Cassette.Shell
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Usage: cassette {path} {output-directory}");
                return;
            }
            var path = args[0];
            if (path.EndsWith("*"))
            {
                path = Path.GetFullPath(path.Substring(0, path.Length - 2)) + "\\";
                var builder = new ScriptModuleContainerBuilder(null, path, new CoffeeScriptCompiler(File.ReadAllText));
                builder.AddModuleForEachSubdirectoryOf("", "");
                var container = builder.Build();
                foreach (var module in container)
                {
                    var outputFilename = Path.GetFullPath(Path.Combine(args[1], module.Path + ".js"));
                    using (var file = new StreamWriter(outputFilename))
                    {
                        var writer = new ScriptModuleWriter(file, path, File.ReadAllText, new CoffeeScriptCompiler(File.ReadAllText));
                        writer.Write(module);
                        file.Flush();
                    }
                }
            }
            else
            {
                path = Path.GetFullPath(path);
                var builder = new UnresolvedScriptModuleBuilder(path);
                var unresolvedModule = builder.Build("", null); // path is the module, so no extra path is required.
                var module = UnresolvedModule.ResolveAll(new[] { unresolvedModule }).First();

                var writer = new ScriptModuleWriter(Console.Out, path, File.ReadAllText, new CoffeeScriptCompiler(File.ReadAllText));
                writer.Write(module);
            }
        }

    }
}

