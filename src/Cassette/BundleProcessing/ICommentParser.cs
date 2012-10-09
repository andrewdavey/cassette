using System.Collections.Generic;

namespace Cassette.BundleProcessing
{
    public interface ICommentParser
    {
        IEnumerable<Comment> Parse(string code);
    }
}
