#if !NET35
using System;
using System.IO;
using Cassette.Configuration;
using Cassette.IO;
using Cassette.Utilities;

namespace Cassette.Web
{
    class UrlHelperExtensions : IStartUpTask, IUrlHelperExtensions
    {
        readonly CassetteSettings settings;
        readonly IUrlGenerator urlGenerator;

        public UrlHelperExtensions(CassetteSettings settings, IUrlGenerator urlGenerator)
        {
            this.settings = settings;
            this.urlGenerator = urlGenerator;
        }

        void IStartUpTask.Run()
        {
            UrlHelperExtensionMethods.Implementation = this;
        }

        public string CassetteFile(string applicationRelativeFilePath)
        {
            applicationRelativeFilePath = PathUtilities.AppRelative(applicationRelativeFilePath);

            var file = settings.SourceDirectory.GetFile(applicationRelativeFilePath);
            ThrowIfFileNotFound(applicationRelativeFilePath, file);
            ThrowIfCannotRequestRawFile(applicationRelativeFilePath, file, settings);

            using (var stream = file.OpenRead())
            {
                var hash = stream.ComputeSHA1Hash().ToHexString();
                return urlGenerator.CreateRawFileUrl(applicationRelativeFilePath, hash);
            }
        }

        static void ThrowIfCannotRequestRawFile(string applicationRelativeFilePath, IFile file, CassetteSettings settings)
        {
            if (settings.CanRequestRawFile(file.FullPath)) return;

            throw new Exception(
                string.Format(
                    "The file {0} cannot be requested. In CassetteConfiguration, use the settings.AllowRawFileAccess method to tell Cassette which files are safe to request.",
                    applicationRelativeFilePath
                )
            );
        }

        static void ThrowIfFileNotFound(string applicationRelativeFilePath, IFile file)
        {
            if (file.Exists) return;
            throw new FileNotFoundException(
                "Cannot find file " + applicationRelativeFilePath,
                applicationRelativeFilePath
            );
        }
    }
}
#endif