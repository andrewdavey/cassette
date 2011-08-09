using System;

namespace Cassette
{
    public interface IModuleCache<T>
        where T : Module
    {
        bool IsUpToDate(DateTime dateTime);
        IModuleContainer<T> LoadModuleContainer();
        void SaveModuleContainer(IModuleContainer<T> moduleContainer);
    }
}
