using System.Collections.Generic;

namespace Cassette
{
    interface ICommentParser
    {
        IEnumerable<Comment> Parse(string code);
    }
}