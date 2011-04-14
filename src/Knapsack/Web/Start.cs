using Microsoft.Web.Infrastructure.DynamicModuleHelper;

[assembly: WebActivator.PreApplicationStartMethod(typeof(Knapsack.Web.Start), "AddHttpModule")]

namespace Knapsack.Web
{
    public static class Start
    {
        public static void AddHttpModule()
        {
            DynamicModuleUtility.RegisterModule(typeof(KnapsackHttpModule));
        }
    }
}
