using System.Collections;

namespace Cassette.BundleProcessing
{
    public static class BundlePipelineHelpers
    {
        public static int IndexOf<T>(this IEnumerable pipeline)
        {
            var enumerator = pipeline.GetEnumerator();
            var index = 0;
            while (enumerator.MoveNext())
            {
                if (enumerator.Current is T)
                {
                    return index;
                }
                index++;
            }
            return -1;
        }
    }
}