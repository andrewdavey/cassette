using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cassette.Scripts;
using Cassette.Utilities;
using Microsoft.Ajax.Utilities;

namespace Cassette.RequireJS
{
    /// <summary>
    /// Configures script bundles to work as AMD modules.
    /// </summary>
    public class AmdConfiguration : IAmdModuleCollection
    {
        readonly Dictionary<string, IAmdModule> modules = new Dictionary<string, IAmdModule>();

        public string MainBundlePath { get; set; }

        public void InitializeModulesFromBundles(IEnumerable<Bundle> bundles, string requireJsScriptPath)
        {
            requireJsScriptPath = PathUtilities.AppRelative(requireJsScriptPath);
            modules.Clear();

            var scriptBundles = GetScriptBundles(bundles);
            foreach (var bundle in scriptBundles)
            {
                foreach (var asset in bundle.Assets)
                {
                    if (asset.Path.Equals(requireJsScriptPath))
                    {
                        MainBundlePath = bundle.Path;
                    }
                    else
                    {
                        modules[asset.Path] = GetModule(asset, bundle);
                    }
                }
            }

            if (MainBundlePath == null)
            {
                modules.Clear();
                throw new ArgumentException("Cannot find a bundle that contains " + requireJsScriptPath);
            }
        }

        IEnumerable<ScriptBundle> GetScriptBundles(IEnumerable<Bundle> bundles)
        {
            return bundles
                .OfType<ScriptBundle>()
                // TODO: Find a cleaner way to exclude Cassette's own bundles
                .Where(b => !b.Path.StartsWith("~/Cassette."));
        }

        Module GetModule(IAsset asset, Bundle bundle)
        {
            var moduleDefinitionParser = ParseJavaScriptForModuleDefinition(asset);
            if (moduleDefinitionParser.FoundModuleDefinition)
            {
                if (moduleDefinitionParser.ModulePath == null)
                {
                    return new AnonymousModule(asset, bundle);
                }
                else
                {
                    return new NamedModule(asset, bundle, moduleDefinitionParser.ModulePath);
                }
            }
            else
            {
                return new PlainScript(asset, bundle, this);
            }
        }

        static ModuleDefinitionParser ParseJavaScriptForModuleDefinition(IAsset asset)
        {
            var tree = ParseJavaScript(asset);
            var moduleDefinitionParser = new ModuleDefinitionParser();
            tree.Accept(moduleDefinitionParser);
            return moduleDefinitionParser;
        }

        static Block ParseJavaScript(IAsset asset)
        {
            var source = asset.OpenStream().ReadToEnd();
            var parser = new JSParser(source);
            return parser.Parse(new CodeSettings());
        }

        public void SetImportAlias(string scriptPath, string importAlias)
        {
            scriptPath = PathUtilities.AppRelative(scriptPath);

            IAmdModule module;
            if (modules.TryGetValue(scriptPath, out module))
            {
                module.Alias = importAlias;
            }
            else
            {
                throw new ArgumentException("Script not found: " + scriptPath);
            }
        }

        public void SetModuleReturnExpression(string path, string moduleReturnExpression)
        {
            var module = this[path] as PlainScript;
            if (module != null)
            {
                module.ModuleReturnExpression = moduleReturnExpression;
            }
            else
            {
                throw new ArgumentException("Cannot change the return expression of a predefined AMD module: " + path);
            }
        }

        public IAmdModule this[string path]
        {
            get
            {
                IAmdModule module;
                if (modules.TryGetValue(path, out module)) return module;
                throw new ArgumentException("Module not found: " + path);
            }
        }

        public IEnumerator<IAmdModule> GetEnumerator()
        {
            return modules.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}