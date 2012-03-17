namespace dotless.Core.Cache
{
    using System.Collections.Generic;

    interface ICache
    {
        void Insert(string cacheKey, IEnumerable<string> fileDependancies, string css);
        bool Exists(string cacheKey);
        string Retrieve(string cacheKey);
    }
}