namespace Knapsack.Web
{
    public interface IManager
    {
        Knapsack.CoffeeScript.ICoffeeScriptCompiler CoffeeScriptCompiler { get; }
        Knapsack.Configuration.KnapsackSection Configuration { get; }
        ModuleContainer ScriptModuleContainer { get; }
        ModuleContainer StylesheetModuleContainer { get; }
    }
}