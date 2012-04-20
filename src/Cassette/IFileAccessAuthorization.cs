using System;

namespace Cassette
{
    public interface IFileAccessAuthorization
    {
        void AllowAccess(string path);
        void AllowAccess(Func<string, bool> pathPredicate);
        bool CanAccess(string path);
    }
}