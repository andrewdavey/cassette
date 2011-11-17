using System.Collections.Generic;

namespace Cassette.BundleProcessing
{
    interface ICommentParser
    {
        IEnumerable<Comment> Parse(string code);
    }
}