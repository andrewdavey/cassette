using System;

namespace Cassette.RequireJS
{
    public static class ModuleInitializerExtensions
    {
        public static void SetImportAlias(this IModuleInitializer configuration, string scriptPath, string importAlias)
        {
            var module = configuration[scriptPath];
            module.Alias = importAlias;
        }

        public static void SetModuleReturnExpression(this IModuleInitializer configuration, string scriptPath, string moduleReturnExpression)
        {
            var module = configuration[scriptPath] as PlainScript;
            if (module != null)
            {
                module.ModuleReturnExpression = moduleReturnExpression;
            }
            else
            {
                throw new ArgumentException("Cannot change the return expression of a predefined AMD module: " + scriptPath);
            }
        }
    }
}