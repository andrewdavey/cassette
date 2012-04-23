using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Cassette.IO;
using Cassette.Utilities;

namespace Cassette
{
    class ConventionalMinifiedFileFilter
    {
        readonly Regex filenameRegex = new Regex(
            @"^(?<name>.*?)(?<mode>(\.|-)min|(\.|-)debug|)(?<extension>\.(js|css))$",
            RegexOptions.IgnoreCase
        );

        FileMatch[] matches;
        List<IFile> outputFile;
        HashedCompareSet<string> filenames;

        public IEnumerable<IFile> Apply(IEnumerable<IFile> files)
        {
            outputFile = new List<IFile>();
            BuildFileMatches(files);
            AddNonMatchingFilesToOutput();
            BuildFilenameSet();
            AddMatchingFilesToOutput();
            return outputFile;
        }

        void BuildFileMatches(IEnumerable<IFile> files)
        {
            matches = (
                from file in files
                let name = file.FullPath.Split('/', '\\').Last()
                let match = filenameRegex.Match(name)
                select new FileMatch(match, file, name)
            ).ToArray();
        }

        void AddNonMatchingFilesToOutput()
        {
            var unsuccessfulMatches = matches.Where(item => !item.Success);
            foreach (var item in unsuccessfulMatches)
            {
                outputFile.Add(item.OriginalFile);
            }
        }

        void BuildFilenameSet()
        {
            filenames = new HashedCompareSet<string>(
                (from item in matches
                where item.Success
                select item.FileName).ToArray(),
                StringComparer.OrdinalIgnoreCase
            );
        }

        void AddMatchingFilesToOutput()
        {
            foreach (var fileMatch in matches.Where(m => m.Success))
            {
                AddFileToOutput(fileMatch);
            }
        }

        void AddFileToOutput(FileMatch fileMatch)
        {
            if (fileMatch.IsMinContentMode)
            {
                AddToOutputIfNonMinFileDoesNotExist(fileMatch);
            }
            else if (fileMatch.IsDebugContentMode)
            {
                outputFile.Add(fileMatch.OriginalFile);
            }
            else
            {
                AddToOutputIfDebugFileDoesNotExist(fileMatch);
            }
        }

        void AddToOutputIfNonMinFileDoesNotExist(FileMatch fileMatch)
        {
            var sourceFileExists = filenames.Contains(fileMatch.BasicName + fileMatch.FileExtension);
            if (!sourceFileExists)
            {
                outputFile.Add(fileMatch.OriginalFile);
            }
        }

        void AddToOutputIfDebugFileDoesNotExist(FileMatch fileMatch)
        {
            var debugFileExists = filenames.Contains(fileMatch.BasicName + ".debug" + fileMatch.FileExtension)
                               || filenames.Contains(fileMatch.BasicName + "-debug" + fileMatch.FileExtension);
            if (!debugFileExists)
            {
                outputFile.Add(fileMatch.OriginalFile);
            }
        }

        class FileMatch
        {
            public readonly bool Success;
            public readonly IFile OriginalFile;
            public readonly string FileName;
            public string BasicName;
            public string FileExtension;
            string contentMode;

            public FileMatch(Match match, IFile originalFile, string fileName)
            {
                Success = match.Success;
                OriginalFile = originalFile;
                FileName = fileName;
                if (Success)
                {
                    InitializeFieldsFromMatchGroups(match);
                }
            }

            void InitializeFieldsFromMatchGroups(Match match)
            {
                BasicName = match.Groups["name"].Value;
                contentMode = match.Groups["mode"].Value.TrimStart('.', '-');
                FileExtension = match.Groups["extension"].Value;
            }

            public bool IsMinContentMode
            {
                get { return contentMode.Equals("min", StringComparison.OrdinalIgnoreCase); }
            }

            public bool IsDebugContentMode
            {
                get { return contentMode.Equals("debug", StringComparison.OrdinalIgnoreCase); }
            }
        }
    }
}