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

using System.Linq;
using System.Web.Mvc;
using Example.Models;

namespace Example.Controllers
{
    public class ColorController : Controller
    {
        // Demo code - please don't do this for real!
        static ColorRepository colors = new ColorRepository();

        public ActionResult List()
        {
            return Json(
                colors.Select(color => new 
                {
                    url = Url.RouteUrl("Color", new { id = color.Id }),
                    red = color.Red,
                    green = color.Green,
                    blue = color.Blue
                }), 
                JsonRequestBehavior.AllowGet
            );
        }

        [HttpPost, ActionName("list")]
        public ActionResult Add(byte red, byte green, byte blue)
        {
            var newColor = colors.Add(red, green, blue);

            var colorUrl = Url.RouteUrl("Color", new { id = newColor.Id });
            Response.StatusCode = 201; // Created
            Response.AddHeader("Location", colorUrl);
            return new EmptyResult();
        }

        [HttpDelete, ActionName("item")]
        public void Delete(int id)
        {
            colors.Delete(id);
        }
    }
}

