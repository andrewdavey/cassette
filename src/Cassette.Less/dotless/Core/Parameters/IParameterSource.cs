namespace dotless.Core.Parameters
{
    using System.Collections.Generic;

    interface IParameterSource
    {
        IDictionary<string, string> GetParameters();
    }
}