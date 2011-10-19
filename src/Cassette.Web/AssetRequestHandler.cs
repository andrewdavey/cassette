#region License
/*
Copyright 2011 Andrew Davey

This file is part of Cassette.

Cassette is free software: you can redistribute it and/or modify it under the 
terms of the GNU General Public License as published by the Free Software 
Foundation, either version 3 of the License, or (at your option) any later 
version.

Cassette is distributed in the hope that it will be useful, but WITHOUT ANY 
WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS 
FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.

You should have received a copy of the GNU General Public License along with 
Cassette. If not, see http://www.gnu.org/licenses/.
*/
#endregion

using System.Web;
using System.Web.Routing;

namespace Cassette.Web
{
    public class AssetRequestHandler : IHttpHandler
    {
        public AssetRequestHandler(RequestContext requestContext, IBundleContainer bundleContainer)
        {
            this.requestContext = requestContext;
            this.bundleContainer = bundleContainer;
        }

        readonly RequestContext requestContext;
        readonly IBundleContainer bundleContainer;

        public bool IsReusable
        {
            get { return false; }
        }

        public void ProcessRequest(HttpContext _)
        {
            var path = "~/" + requestContext.RouteData.GetRequiredString("path");
            Trace.Source.TraceInformation("Handling asset request for path \"{0}\".", path);
            var response = requestContext.HttpContext.Response;
            var bundle = bundleContainer.FindBundleContainingPath(path);
            if (bundle == null)
            {
                Trace.Source.TraceInformation("Bundle not found for asset path \"{0}\".", path);
                NotFound(response);
                return;
            }
            
            var asset = bundle.FindAssetByPath(path);
            if (asset == null)
            {
                Trace.Source.TraceInformation("Asset not found \"{0}\".", path);
                NotFound(response);
                return;
            }

            SendAsset(response, bundle, asset);
        }

        void SendAsset(HttpResponseBase response, Bundle bundle, IAsset asset)
        {
            response.ContentType = bundle.ContentType;
            using (var stream = asset.OpenStream())
            {
                stream.CopyTo(response.OutputStream);
            }
        }

        void NotFound(HttpResponseBase response)
        {
            response.StatusCode = 404;
            response.End();
        }
    }
}
