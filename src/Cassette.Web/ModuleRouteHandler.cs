using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Routing;
using System.Web;

namespace Cassette.Web
{
    public class ModuleRouteHandler<T> : IRouteHandler
        where T : Module
    {
        public ModuleRouteHandler(IModuleContainer<T> moduleContainer)
        {
            this.moduleContainer = moduleContainer;
        }

        readonly IModuleContainer<T> moduleContainer;

        public IHttpHandler GetHttpHandler(RequestContext requestContext)
        {
            return new ModuleRequestHandler<T>(moduleContainer, requestContext);
        }
    }
}
