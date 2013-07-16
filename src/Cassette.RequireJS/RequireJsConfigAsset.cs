using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cassette.Utilities;

namespace Cassette.RequireJS
{
    public class RequireJsConfigAsset : IAsset
    {
        readonly IEnumerable<IAmdModule> modules;

        private readonly RequireJSConfiguration requireJsConfiguration;

        readonly IConfigurationScriptBuilder configurationScriptBuilder;
        
        public RequireJsConfigAsset(IEnumerable<IAmdModule> modules, RequireJSConfiguration requireJsConfiguration, IConfigurationScriptBuilder configurationScriptBuilder)
        {
            this.modules = modules;
            this.requireJsConfiguration = requireJsConfiguration;
            this.configurationScriptBuilder = configurationScriptBuilder;
        }

        public string Content
        {
            get { return configurationScriptBuilder.BuildConfigurationScript(modules, requireJsConfiguration); }
        }

        public Type AssetCacheValidatorType
        {
            get { return typeof(AlwaysValidAssetCacheValidator); }
        }

        public byte[] Hash
        {
            get
            {
                using (var stream = OpenStream())
                {
                    return stream.ComputeSHA1Hash();
                }
            }
        }

        public string Path
        {
            get { return "~/Cassette.RequireJs"; }
        }

        public IEnumerable<AssetReference> References
        {
            get { return Enumerable.Empty<AssetReference>(); }
        }

        public void Accept(IBundleVisitor visitor)
        {
            visitor.Visit(this);
        }

        public void AddAssetTransformer(IAssetTransformer transformer)
        {
            throw new NotImplementedException();
        }

        public void AddReference(string assetRelativePath, int lineNumber)
        {
            throw new NotImplementedException();
        }

        public void AddRawFileReference(string relativeFilename)
        {
            throw new NotImplementedException();
        }

        public Stream OpenStream()
        {
            return Content.AsStream();
        }
    }
}