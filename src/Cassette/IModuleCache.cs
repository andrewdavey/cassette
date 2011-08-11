using System;

namespace Cassette
{
    public interface IModuleCache<T>
        where T : Module
    {
        bool IsUpToDate(DateTime dateTime, string version);
        IModuleContainer<T> LoadModuleContainer();
        void SaveModuleContainer(IModuleContainer<T> moduleContainer, string version);
    }
}
