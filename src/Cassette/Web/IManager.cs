namespace Cassette.Web
{
    public interface IManager
    {
        Cassette.CoffeeScript.ICoffeeScriptCompiler CoffeeScriptCompiler { get; }
        Cassette.Configuration.CassetteSection Configuration { get; }
        ModuleContainer ScriptModuleContainer { get; }
        ModuleContainer StylesheetModuleContainer { get; }
        System.Web.Caching.CacheDependency CreateCacheDependency();
    }
}