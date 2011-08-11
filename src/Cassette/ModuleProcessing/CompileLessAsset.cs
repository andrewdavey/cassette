using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Cassette.Less;

namespace Cassette.ModuleProcessing
{
    public class CompileLessAsset : IAssetTransformer
    {
        public CompileLessAsset(ILessCompiler compiler)
        {
            this.compiler = compiler;
        }

        readonly ILessCompiler compiler;

        public Func<Stream> Transform(Func<Stream> openSourceStream, IAsset asset)
        {
            throw new NotImplementedException();
        }
    }
}
