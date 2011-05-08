using System;
using System.Web;

namespace Knapsack.Integration.Web
{
    public class KnapsackHttpModule : IHttpModule
    {
        // Using Lazy<T> means we get a singleton Manager which is created in a thread-safe manner.
        static Lazy<Manager> manager = new Lazy<Manager>();
        
        public static Manager Manager
        {
            get 
            {
                return manager.Value;
            }
        }

        public void Init(HttpApplication application)
        {
            application.BeginRequest += HandleBeginRequest;
        }

        void HandleBeginRequest(object sender, EventArgs e)
        {
            StoreReferenceBuilderInHttpContextItems();
        }

        void StoreReferenceBuilderInHttpContextItems()
        {
            HttpContext.Current.Items["Knapsack.ReferenceBuilder"] =
                new ReferenceBuilder(Manager.ModuleContainer);
        }

        public void Dispose()
        {
        }
    }
}
