using System;
using System.Web.Caching;
using Knapsack.CoffeeScript;
using Knapsack.Configuration;

namespace Knapsack.Web
{
    // We don't want to throw the exception during lazy initialization because
    // there is no chance to recreate the object, even if, say, a broken file is fixed.
    // Instead, we cache the exception in this special implementation of IManager
    // and throw it when trying to call members. The CreateCacheDependency()
    // method creates a special dependency that will be triggered just before 
    // the exception is thrown. This provides us with a chance to re-create the
    // lazy object with fixed (we hope!) files.
    public class ExceptionCachedManager : IManager
    {
        public ExceptionCachedManager(Exception exception)
        {
            this.exception = exception;
            cacheClearer = new CacheClearer();
        }

        readonly Exception exception;
        readonly CacheClearer cacheClearer;

        public ICoffeeScriptCompiler CoffeeScriptCompiler
        {
            get
            {
                cacheClearer.Clear();
                throw exception;
            }
        }

        public KnapsackSection Configuration
        {
            get 
            {
                cacheClearer.Clear();
                throw exception;
            }
        }

        public ModuleContainer ScriptModuleContainer
        {
            get
            {
                cacheClearer.Clear();
                throw exception;
            }
        }

        public ModuleContainer StylesheetModuleContainer
        {
            get
            {
                cacheClearer.Clear();
                throw exception;
            }
        }

        public CacheDependency CreateCacheDependency()
        {
            return cacheClearer;
        }

        class CacheClearer : CacheDependency
        {
            public void Clear()
            {
                NotifyDependencyChanged(this, EventArgs.Empty);
            }
        }
    }
}
