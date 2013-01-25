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
        readonly IConfigurationScriptBuilder configurationScriptBuilder;
        
        public RequireJsConfigAsset(IEnumerable<IAmdModule> modules, IConfigurationScriptBuilder configurationScriptBuilder)
        {
            this.modules = modules;
            this.configurationScriptBuilder = configurationScriptBuilder;
        }

        public string Content
        {
            get { return configurationScriptBuilder.BuildConfigurationScript(modules); }
        }

        public Type AssetCacheValidatorType
        {
            get { return typeof(AlwaysValidAssetCacheValidator); }
        }

        public byte[] Hash
        {
            get
            {
                return Content.ComputeSHA1Hash();
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

        public string GetTransformedContent()
        {
            return Content;
        }
    }
}