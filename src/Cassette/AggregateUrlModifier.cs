using System.Linq;

namespace Cassette
{
    class AggregateUrlModifier : IUrlModifier
    {
        readonly IUrlModifier[] urlModifiers;

        public AggregateUrlModifier(params IUrlModifier[] urlModifiers)
        {
            this.urlModifiers = urlModifiers;
        }

        public string Modify(string url)
        {
            return urlModifiers.Aggregate(url, (currentUrl, modifier) => modifier.Modify(currentUrl));
        }
    }
}