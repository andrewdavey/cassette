using Microsoft.Web.Infrastructure.DynamicModuleHelper;

[assembly: WebActivator.PreApplicationStartMethod(typeof(Knapsack.Integration.Web.Start), "AddHttpModule")]

namespace Knapsack.Integration.Web
{
    public static class Start
    {
        public static void AddHttpModule()
        {
            DynamicModuleUtility.RegisterModule(typeof(KnapsackHttpModule));
        }
    }
}
