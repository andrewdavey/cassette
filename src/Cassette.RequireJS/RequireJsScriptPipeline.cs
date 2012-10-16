using Cassette.Scripts;
using Cassette.TinyIoC;

namespace Cassette.RequireJS
{
    public class RequireJsScriptPipeline : ScriptPipeline
    {
        public RequireJsScriptPipeline(TinyIoCContainer container, CassetteSettings settings)
            : base(container, settings)
        {
            Insert<AddDefineCallTransformerToAssets>(0);
        }
    }
}