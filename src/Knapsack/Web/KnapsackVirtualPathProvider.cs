using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Hosting;
using System.IO.IsolatedStorage;
using System.IO;

namespace Knapsack.Web
{
    public class KnapsackVirtualPathProvider : VirtualPathProvider
    {
        readonly ModuleContainer moduleContainer;
        readonly IsolatedStorageFile storage;

        public KnapsackVirtualPathProvider(ModuleContainer moduleContainer, IsolatedStorageFile storage)
        {
            this.moduleContainer = moduleContainer;
            this.storage = storage;
        }

        public override bool FileExists(string virtualPath)
        {
            return moduleContainer.Contains(virtualPath);
        }

        public override VirtualFile GetFile(string virtualPath)
        {
            var module = moduleContainer.FindModuleContainingScript(virtualPath);
            return new KnapsackVirtualFile(module, storage);
        }
    }
}
