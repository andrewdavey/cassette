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
using System.Diagnostics;
using Cassette.IO;
using Jurassic;

namespace Cassette.Scripts
{
    public class JurassicCoffeeScriptCompiler : ICompiler
    {
        static JurassicCoffeeScriptCompiler()
        {
            scriptEngine = new Lazy<ScriptEngine>(CreateScriptEngineWithCoffeeScriptLoaded);
        }

        readonly static Lazy<ScriptEngine> scriptEngine;

        public string Compile(string coffeeScriptSource, IFile sourceFile)
        {
            Trace.Source.TraceInformation("Compiling {0}", sourceFile.FullPath);
            lock (ScriptEngine) // ScriptEngine is NOT thread-safe, so we MUST lock.
            {
                try
                {
                    Trace.Source.TraceInformation("Compiled {0}", sourceFile.FullPath);
                    return ScriptEngine.CallGlobalFunction<string>("compile", coffeeScriptSource);
                }
                catch (Exception ex)
                {
                    var message = ex.Message + " in " + sourceFile.FullPath;
                    Trace.Source.TraceEvent(TraceEventType.Critical, 0, message);
                    throw new CoffeeScriptCompileException(message, sourceFile.FullPath, ex);
                }
            }
        }

        static ScriptEngine CreateScriptEngineWithCoffeeScriptLoaded()
        {
            var engine = new ScriptEngine();
            engine.Execute(Properties.Resources.coffeescript);
            engine.Execute("function compile(c) { return CoffeeScript.compile(c); }");
            return engine;
        }

        ScriptEngine ScriptEngine
        {
            get { return scriptEngine.Value; }
        }
    }
}

