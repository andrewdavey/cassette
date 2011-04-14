using System.IO;
using System.IO.IsolatedStorage;
using System.Web.Hosting;

namespace Knapsack.Web
{
    class KnapsackVirtualFile : VirtualFile
    {
        readonly Module module;
        readonly IsolatedStorageFile storage;

        public KnapsackVirtualFile(Module module, IsolatedStorageFile storage)
            : base(module.Path)
        {
            this.module = module;
            this.storage = storage;
        }

        public override System.IO.Stream Open()
        {
            return storage.OpenFile(module.Path, FileMode.Open, FileAccess.Read);
        }
    }
}
