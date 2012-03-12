using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Cassette.IO;
using Cassette.Utilities;

namespace Cassette.Stylesheets
{
    abstract class CssUrlMatchTransformer
    {
        protected CssUrlMatchTransformer(Match match, IAsset asset)
        {
            matchIndex = match.Index;
            matchLength = match.Length;
            path = match.Groups["path"].Value;
            url = (path.StartsWith("/") ? "~" : "") + path + "." + match.Groups["extension"].Value;
            extension = match.Groups["extension"].Value;
            file = asset.SourceFile.Directory.GetFile(url);
        }

        readonly int matchIndex;
        readonly int matchLength;
        readonly string path;
        readonly string url;
        readonly string extension;
        readonly IFile file;

        public string Url
        {
            get { return url; }
        }

        protected int MatchIndex
        {
            get { return matchIndex; }
        }

        protected int MatchLength
        {
            get { return matchLength;  }
        }

        protected IFile File
        {
            get { return file; }
        }

        protected string DataUri
        {
            get
            {
                return string.Format(
                    "data:{0};base64,{1}",
                    GetContentType(extension),
                    GetBase64EncodedData()
                );
            }
        }

        protected abstract string GetContentType(string extension);

        string GetBase64EncodedData()
        {
            using (var fileStream = file.OpenRead())
            using (var temp = new MemoryStream())
            using (var base64Stream = new CryptoStream(temp, new ToBase64Transform(), CryptoStreamMode.Write))
            {
                fileStream.CopyTo(base64Stream);
                base64Stream.FlushFinalBlock();
                base64Stream.Flush();
                temp.Position = 0;
                var reader = new StreamReader(temp);
                return reader.ReadToEnd();
            }
        }

        public abstract void Transform(StringBuilder css);

        public virtual bool CanTransform
        {
            get { return file.Exists; }
        }
    }
}