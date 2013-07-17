using System;

namespace Cassette.RequireJS
{
    class NamedModule : Module
    {
        public NamedModule(IAsset asset, Bundle bundle, string modulePath)
            : base(asset, bundle,null)
        {
            if (modulePath == null) throw new ArgumentNullException("modulePath");

            ModulePath = modulePath;
        }
    }
}