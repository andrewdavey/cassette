using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml.Linq;

namespace Cassette.Persistence
{
    public class ModuleContainerWriter<T> : IModuleContainerWriter<T>
        where T : Module
    {
        public ModuleContainerWriter(IFileSystem fileSystem)
        {
            this.fileSystem = fileSystem;
        }

        readonly IFileSystem fileSystem;

        public void Save(IModuleContainer<T> moduleContainer)
        {
            fileSystem.DeleteAll();
            SaveContainerXml(moduleContainer);
            foreach (var module in moduleContainer)
            {
                SaveModule(module, moduleContainer);
            }
        }

        void SaveContainerXml(IModuleContainer<T> moduleContainer)
        {
            var createManifestVisitor = new CreateManifestVisitor();
            var xml = new XDocument(
                new XElement("container",
                    new XAttribute("lastWriteTime", moduleContainer.LastWriteTime.Ticks),
                    moduleContainer.Select(createManifestVisitor.CreateManifest)
                )
            );
            using (var fileStream = fileSystem.OpenFile("container.xml", FileMode.OpenOrCreate, FileAccess.Write))
            {
                xml.Save(fileStream);
            }
        }

        void SaveModule(T module, IModuleContainer<T> moduleContainer)
        {
            if (module.Assets.Count > 1)
            {
                throw new InvalidOperationException("Cannot save a module when assets have not been concatenated into a single asset.");
            }
            var filename = module.Directory + ".module";
            using (var fileStream = fileSystem.OpenFile(filename, FileMode.OpenOrCreate, FileAccess.Write))
            {
                using (var dataStream = module.Assets[0].OpenStream())
                {
                    dataStream.CopyTo(fileStream);
                }
                fileStream.Flush();
            }
        }
    }
}
