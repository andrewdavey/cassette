namespace dotless.Core
{
    using System.Collections.Generic;

    interface ILessEngine
    {
        string TransformToCss(string source, string fileName);
        void ResetImports();
        IEnumerable<string> GetImports();
    }
}