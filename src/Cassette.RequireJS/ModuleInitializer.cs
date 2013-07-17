using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cassette.Scripts;
using Cassette.Utilities;
using Microsoft.Ajax.Utilities;

namespace Cassette.RequireJS
{
    public class ModuleInitializer : IModuleInitializer
    {
        readonly IConfigurationScriptBuilder configurationScriptBuilder;
        readonly Dictionary<string, IAmdModule> modules = new Dictionary<string, IAmdModule>();
        RequireJSConfiguration requireJsConfiguration = new RequireJSConfiguration();

        public ModuleInitializer(IConfigurationScriptBuilder configurationScriptBuilder)
        {
            this.configurationScriptBuilder = configurationScriptBuilder;
        }

        public string MainBundlePath { get; private set; }
        public string RequireJsScriptPath { get; private set; }

        public RequireJSConfiguration RequireJsConfiguration 
        { 
            get
            {
                return requireJsConfiguration;
            }
            set
            {
                requireJsConfiguration = value;
            }
        }

        public void InitializeModules(IEnumerable<Bundle> bundles, string requireJsScriptPath, string baseUrl = null)
        {
            RequireJsConfiguration.BaseUrl = baseUrl;
            RequireJsScriptPath = PathUtilities.AppRelative(requireJsScriptPath);


            modules.Clear();

            var scriptBundles = GetScriptBundles(bundles);
            foreach (var bundle in scriptBundles)
            {
                if (!bundle.Assets.Any())
                {
                    var externalScriptBundle = bundle as ExternalScriptBundle;
                    if (externalScriptBundle != null)
                    {
                        modules[externalScriptBundle.Path] = new ExternalScriptModule(externalScriptBundle, this,baseUrl);
                    }
                }

                foreach (var asset in bundle.Assets)
                {
                    if (asset.Path.Equals(RequireJsScriptPath))
                    {
                        MainBundlePath = bundle.Path;
                    }
                    else
                    {
                        modules[asset.Path] = GetModule(asset, bundle,baseUrl);
                    }
                }
            }

            if (MainBundlePath == null)
            {
                modules.Clear();
                throw new ArgumentException("Cannot find a bundle that contains " + RequireJsScriptPath);
            }
        }

        IEnumerable<ScriptBundle> GetScriptBundles(IEnumerable<Bundle> bundles)
        {
            return bundles
                .OfType<ScriptBundle>()
                // TODO: Find a cleaner way to exclude Cassette's own bundles
                .Where(b => !b.Path.StartsWith("~/Cassette."));
        }

        IAmdModule GetModule(IAsset asset, Bundle bundle, string baseUrl)
        {
            
            var moduleDefinitionParser = ParseJavaScriptForModuleDefinition(asset);
            if (moduleDefinitionParser.FoundModuleDefinition)
            {
                if (moduleDefinitionParser.ModulePath == null)
                {
                    return new AnonymousModule(asset, bundle,baseUrl);
                }
                else
                {
                    return new NamedModule(asset, bundle, moduleDefinitionParser.ModulePath);
                }
            }
            else
            {
                return new PlainScript(asset, bundle, this, baseUrl);
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

        public IAmdModule this[string scriptPath]
        {
            get
            {
                scriptPath = PathUtilities.AppRelative(scriptPath);

                IAmdModule module;
                if (modules.TryGetValue(scriptPath, out module)) return module;
                throw new ArgumentException("Module not found: " + scriptPath);
            }
        }

        public void AddRequireJsConfigAssetToMainBundle(ScriptBundle bundle)
        {
            bundle.Assets.Add(new RequireJsConfigAsset(modules.Values, requireJsConfiguration, configurationScriptBuilder));
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

    public class RequireJSConfiguration
    {
        public bool? CatchError { get; set; }
        public string OnErrorModule { get; set; }
        public bool? EnforceDefine { get; set; }
        public object Config { get; set; }
        public int? WaitSeconds { get; set; }

        internal string BaseUrl { get; set; }
    }
}