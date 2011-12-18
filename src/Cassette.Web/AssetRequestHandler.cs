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

using System;
using System.Web;
using System.Web.Routing;
using Cassette.Utilities;

namespace Cassette.Web
{
    class AssetRequestHandler : IHttpHandler
    {
        public AssetRequestHandler(RequestContext requestContext, Func<IBundleContainer> getBundleContainer)
        {
            this.requestContext = requestContext;
            this.getBundleContainer = getBundleContainer;
        }

        readonly RequestContext requestContext;
        readonly Func<IBundleContainer> getBundleContainer;

        public bool IsReusable
        {
            get { return false; }
        }

        public void ProcessRequest(HttpContext _)
        {
            var path = "~/" + requestContext.RouteData.GetRequiredString("path");
            Trace.Source.TraceInformation("Handling asset request for path \"{0}\".", path);
            requestContext.HttpContext.DisableHtmlRewriting();
            var response = requestContext.HttpContext.Response;
            IAsset asset;
            Bundle bundle;
            if (!getBundleContainer().TryGetAssetByPath(path, out asset, out bundle))
            {
                Trace.Source.TraceInformation("Bundle asset not found with path \"{0}\".", path);
                NotFound(response);
                return;
            }

            var request = requestContext.HttpContext.Request;
            SendAsset(request, response, bundle, asset);
        }

        void SendAsset(HttpRequestBase request, HttpResponseBase response, Bundle bundle, IAsset asset)
        {
            response.ContentType = bundle.ContentType;

            var actualETag = "\"" + asset.Hash.ToHexString() + "\"";
            response.Cache.SetCacheability(HttpCacheability.Public);
            response.Cache.SetETag(actualETag);

            var givenETag = request.Headers["If-None-Match"];
            if (givenETag == actualETag)
            {
                SendNotModified(response);
            }
            else
            {
                using (var stream = asset.OpenStream())
                {
                    stream.CopyTo(response.OutputStream);
                }
            }
        }

        void SendNotModified(HttpResponseBase response)
        {
            response.StatusCode = 304; // Not Modified
            response.SuppressContent = true;
        }

        void NotFound(HttpResponseBase response)
        {
            response.StatusCode = 404;
            response.SuppressContent = true;
        }
    }
}