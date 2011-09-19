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

using System.Collections.Generic;
using System.Linq;

namespace Example.Models
{
    public class ColorRepository : IEnumerable<Color>
    {
        List<Color> colors = new List<Color>();

        public Color Add(byte red, byte green, byte blue)
        {
            var id = 1 + (colors.Any() ? colors.Max(c => c.Id) : 0);
            var color = new Color
            {
                Id = id,
                Red = red,
                Green = green,
                Blue = blue
            };
            colors.Add(color);
            return color;
        }

        public void Delete(int id)
        {
            var color = colors.FirstOrDefault(c => c.Id == id);
            if (color != null)
            {
                colors.Remove(color);
            }
        }

        public IEnumerator<Color> GetEnumerator()
        {
            return colors.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
