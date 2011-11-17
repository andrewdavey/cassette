using System;
using System.Collections.Generic;
using System.Linq;

namespace Cassette
{
    class ReferenceParser
    {
        readonly ICommentParser commentParser;

        public ReferenceParser(ICommentParser commentParser)
        {
            this.commentParser = commentParser;
        }

        public IEnumerable<AssetReference> Parse(string code, IAsset sourceAsset)
        {
            var comments = commentParser.Parse(code);
            return from comment in comments
                   from reference in ParseReferences(comment.SourceLineNumber, comment.Value, sourceAsset)
                   select reference;
        }

        IEnumerable<AssetReference> ParseReferences(int lineNumber, string comment, IAsset sourceAsset)
        {
            var lines = comment.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            return from line in lines
                   let parts = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                   from path in parts.Skip(1)
                   select new AssetReference(path.TrimEnd(';'), sourceAsset, lineNumber, AssetReferenceType.SameBundle);            
        }
    }
}