using System.Collections.Generic;
using System.Linq;

namespace DotNet35.Models
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
